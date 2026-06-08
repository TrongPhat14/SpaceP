#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Unity.Profiling;
using UnityEngine;

[RequireComponent(typeof(ProjectilePool))]
public class ProjectilePoolingBenchmark : MonoBehaviour
{
    public enum BenchmarkScenario
    {
        Gameplay,
        Stress
    }

    [Header("Run")]
    [SerializeField] private bool runOnStart;
    [SerializeField] private BenchmarkScenario scenario = BenchmarkScenario.Gameplay;
    [SerializeField, Min(1)] private int repetitions = 3;
    [SerializeField, Min(1f)] private float warmupSeconds = 15f;
    [SerializeField, Min(1f)] private float measurementSeconds = 60f;
    [SerializeField, Min(0f)] private float cooldownSeconds = 5f;
    [SerializeField, Min(1f)] private float poolDrainTimeoutSeconds = 10f;

    [Header("Synthetic Projectile Load")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField, Min(0.01f)] private float gameplaySpawnInterval = 1.5f;
    [SerializeField, Min(0.01f)] private float stressSpawnInterval = 0.05f;
    [SerializeField, Min(0.1f)] private float projectileSpeed = 8f;

    private const string CsvFileName = "projectile-pooling-benchmark.csv";

    private readonly List<float> frameTimesMs = new List<float>(10000);
    private readonly List<string> completedRunSummaries = new List<string>();

    private ProjectilePool projectilePool;
    private ProfilerRecorder gcAllocatedRecorder;
    private ProfilerRecorder systemMemoryRecorder;
    private Coroutine benchmarkCoroutine;
    private string currentStatus = "Benchmark disabled";
    private double totalGcAllocatedBytes;
    private long peakGcAllocatedBytes;
    private long memoryAtStartBytes;
    private long peakMemoryBytes;
    private long memoryAtEndBytes;
    private float spawnAccumulator;
    private bool isMeasuring;
    private float previousTimeScale;
    private bool environmentPrepared;
    private PlayerFollowerShooter[] playerFollowerShooters;
    private bool[] shooterEnabledStates;

    private bool CanRunBenchmark =>
        Debug.isDebugBuild || Application.isEditor;

    private void Awake()
    {
        projectilePool = GetComponent<ProjectilePool>();
    }

    private void Start()
    {
        if (runOnStart)
        {
            StartBenchmark();
        }
    }

    [ContextMenu("Start Projectile Benchmark")]
    public void StartBenchmark()
    {
        if (!CanRunBenchmark)
        {
            Debug.LogWarning(
                "Projectile benchmark only runs in the Editor or a Development Build."
            );
            return;
        }

        if (benchmarkCoroutine != null)
        {
            Debug.LogWarning("Projectile benchmark is already running.");
            return;
        }

        benchmarkCoroutine = StartCoroutine(RunBenchmark());
    }

    private IEnumerator RunBenchmark()
    {
        PrepareBenchmarkEnvironment();
        completedRunSummaries.Clear();
        currentStatus =
            $"Preparing {projectilePool.LifecycleMode} / {scenario}";

        yield return WaitForProjectilesToDrain();

        for (int run = 1; run <= repetitions; run++)
        {
            currentStatus =
                $"Run {run}/{repetitions}: warm-up ({warmupSeconds:F0}s)";
            yield return RunLoadForDuration(warmupSeconds, false);

            ProjectilePoolStatistics startStats =
                projectilePool.GetStatistics();

            ResetMeasurements();
            projectilePool.ResetPeakActiveCount();
            StartRecorders();
            isMeasuring = true;

            currentStatus =
                $"Run {run}/{repetitions}: measuring ({measurementSeconds:F0}s)";
            yield return RunLoadForDuration(measurementSeconds, true);

            isMeasuring = false;
            StopRecorders();

            ProjectilePoolStatistics endStats =
                projectilePool.GetStatistics();
            BenchmarkResult result = BuildResult(run, startStats, endStats);
            string summary = FormatSummary(result);

            completedRunSummaries.Add(summary);
            AppendCsv(result);
            Debug.Log(summary);

            if (run < repetitions && cooldownSeconds > 0f)
            {
                currentStatus =
                    $"Run {run}/{repetitions}: cooldown ({cooldownSeconds:F0}s)";
                yield return new WaitForSecondsRealtime(cooldownSeconds);
            }

            yield return WaitForProjectilesToDrain();
        }

        currentStatus =
            $"Completed. CSV: {Path.Combine(Application.persistentDataPath, CsvFileName)}";
        benchmarkCoroutine = null;
        RestoreBenchmarkEnvironment();
    }

    private void PrepareBenchmarkEnvironment()
    {
        if (environmentPrepared)
        {
            return;
        }

        previousTimeScale = Time.timeScale;
        Time.timeScale = 1f;

        playerFollowerShooters = FindObjectsByType<PlayerFollowerShooter>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
        shooterEnabledStates = new bool[playerFollowerShooters.Length];

        for (int i = 0; i < playerFollowerShooters.Length; i++)
        {
            PlayerFollowerShooter shooter = playerFollowerShooters[i];
            shooterEnabledStates[i] = shooter != null && shooter.enabled;

            if (shooter != null)
            {
                shooter.enabled = false;
            }
        }

        environmentPrepared = true;
    }

    private void RestoreBenchmarkEnvironment()
    {
        if (!environmentPrepared)
        {
            return;
        }

        Time.timeScale = previousTimeScale;

        for (int i = 0; i < playerFollowerShooters.Length; i++)
        {
            PlayerFollowerShooter shooter = playerFollowerShooters[i];

            if (shooter != null)
            {
                shooter.enabled = shooterEnabledStates[i];
            }
        }

        playerFollowerShooters = null;
        shooterEnabledStates = null;
        environmentPrepared = false;
    }

    private IEnumerator WaitForProjectilesToDrain()
    {
        float elapsed = 0f;

        while (projectilePool.GetStatistics().Active > 0 &&
               elapsed < poolDrainTimeoutSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        int remainingProjectiles = projectilePool.GetStatistics().Active;
        if (remainingProjectiles > 0)
        {
            Debug.LogWarning(
                $"Projectile benchmark drain timed out with {remainingProjectiles} active projectile(s)."
            );
        }
    }

    private IEnumerator RunLoadForDuration(float duration, bool collectMetrics)
    {
        float elapsed = 0f;
        spawnAccumulator = 0f;
        float spawnInterval = scenario == BenchmarkScenario.Stress
            ? stressSpawnInterval
            : gameplaySpawnInterval;

        while (elapsed < duration)
        {
            float deltaTime = Time.unscaledDeltaTime;
            elapsed += deltaTime;
            spawnAccumulator += deltaTime;

            while (spawnAccumulator >= spawnInterval)
            {
                spawnAccumulator -= spawnInterval;
                SpawnProjectile();
            }

            if (collectMetrics)
            {
                CollectFrameMetrics(deltaTime);
            }

            yield return null;
        }
    }

    private void SpawnProjectile()
    {
        Vector3 position = spawnPoint != null
            ? spawnPoint.position
            : GetDefaultSpawnPosition();

        float angle = Time.frameCount * 137.5f * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        EnemyProjectile projectile =
            projectilePool.Get(position, Quaternion.identity);

        if (projectile != null)
        {
            projectile.SetCollisionEnabled(false);
            projectile.Launch(direction, projectileSpeed);
        }
    }

    private Vector3 GetDefaultSpawnPosition()
    {
        if (Camera.main != null)
        {
            Vector3 cameraPosition = Camera.main.transform.position;
            return new Vector3(cameraPosition.x, cameraPosition.y, 0f);
        }

        return Vector3.zero;
    }

    private void ResetMeasurements()
    {
        frameTimesMs.Clear();
        totalGcAllocatedBytes = 0d;
        peakGcAllocatedBytes = 0;
        memoryAtStartBytes = 0;
        peakMemoryBytes = 0;
        memoryAtEndBytes = 0;
    }

    private void StartRecorders()
    {
        DisposeRecorders();

        gcAllocatedRecorder = ProfilerRecorder.StartNew(
            ProfilerCategory.Memory,
            "GC Allocated In Frame"
        );
        systemMemoryRecorder = ProfilerRecorder.StartNew(
            ProfilerCategory.Memory,
            "System Used Memory"
        );

        if (systemMemoryRecorder.Valid)
        {
            long memoryBytes = systemMemoryRecorder.LastValue;

            RecordMemorySample(memoryBytes);
        }
    }

    private void CollectFrameMetrics(float unscaledDeltaTime)
    {
        frameTimesMs.Add(unscaledDeltaTime * 1000f);

        if (gcAllocatedRecorder.Valid)
        {
            long gcBytes = Math.Max(0L, gcAllocatedRecorder.LastValue);
            totalGcAllocatedBytes += gcBytes;
            peakGcAllocatedBytes = Math.Max(peakGcAllocatedBytes, gcBytes);
        }

        if (systemMemoryRecorder.Valid)
        {
            RecordMemorySample(systemMemoryRecorder.LastValue);
        }
    }

    private void RecordMemorySample(long memoryBytes)
    {
        if (memoryBytes <= 0L)
        {
            return;
        }

        if (memoryAtStartBytes <= 0L)
        {
            memoryAtStartBytes = memoryBytes;
        }

        memoryAtEndBytes = memoryBytes;
        peakMemoryBytes = Math.Max(peakMemoryBytes, memoryBytes);
    }

    private void StopRecorders()
    {
        if (gcAllocatedRecorder.Valid)
        {
            gcAllocatedRecorder.Stop();
        }

        if (systemMemoryRecorder.Valid)
        {
            systemMemoryRecorder.Stop();
        }
    }

    private BenchmarkResult BuildResult(
        int run,
        ProjectilePoolStatistics startStats,
        ProjectilePoolStatistics endStats
    )
    {
        frameTimesMs.Sort();

        float averageFrameTime = CalculateAverage(frameTimesMs);
        float p95FrameTime = CalculatePercentile(frameTimesMs, 0.95f);
        float maximumFrameTime = frameTimesMs.Count > 0
            ? frameTimesMs[frameTimesMs.Count - 1]
            : 0f;
        float onePercentLowFps = CalculateOnePercentLowFps(frameTimesMs);
        return new BenchmarkResult
        {
            TimestampUtc = DateTime.UtcNow.ToString("O"),
            Device = SystemInfo.deviceModel,
            UnityVersion = Application.unityVersion,
            Mode = projectilePool.LifecycleMode.ToString(),
            Scenario = scenario.ToString(),
            Run = run,
            DurationSeconds = measurementSeconds,
            SampleCount = frameTimesMs.Count,
            AverageFps = averageFrameTime > 0f
                ? 1000f / averageFrameTime
                : 0f,
            OnePercentLowFps = onePercentLowFps,
            AverageFrameTimeMs = averageFrameTime,
            P95FrameTimeMs = p95FrameTime,
            MaximumFrameTimeMs = maximumFrameTime,
            TotalGcAllocatedBytes = gcAllocatedRecorder.Valid
                ? (long)totalGcAllocatedBytes
                : -1L,
            PeakGcAllocatedPerFrameBytes = gcAllocatedRecorder.Valid
                ? peakGcAllocatedBytes
                : -1L,
            MemoryAtStartBytes = systemMemoryRecorder.Valid
                ? memoryAtStartBytes
                : -1L,
            PeakMemoryBytes = systemMemoryRecorder.Valid
                ? peakMemoryBytes
                : -1L,
            MemoryAtEndBytes = memoryAtEndBytes > 0L
                ? memoryAtEndBytes
                : -1L,
            ProjectilesCreated =
                endStats.TotalCreated - startStats.TotalCreated,
            ProjectilesReused =
                endStats.TotalReused - startStats.TotalReused,
            PeakActiveProjectiles = endStats.PeakActive
        };
    }

    private static float CalculateAverage(List<float> values)
    {
        if (values.Count == 0)
        {
            return 0f;
        }

        double total = 0d;
        for (int i = 0; i < values.Count; i++)
        {
            total += values[i];
        }

        return (float)(total / values.Count);
    }

    private static float CalculatePercentile(
        List<float> sortedValues,
        float percentile
    )
    {
        if (sortedValues.Count == 0)
        {
            return 0f;
        }

        int index = Mathf.Clamp(
            Mathf.CeilToInt(sortedValues.Count * percentile) - 1,
            0,
            sortedValues.Count - 1
        );
        return sortedValues[index];
    }

    private static float CalculateOnePercentLowFps(
        List<float> sortedFrameTimes
    )
    {
        if (sortedFrameTimes.Count == 0)
        {
            return 0f;
        }

        int worstFrameCount = Mathf.Max(
            1,
            Mathf.CeilToInt(sortedFrameTimes.Count * 0.01f)
        );
        double worstFrameTotal = 0d;

        for (
            int i = sortedFrameTimes.Count - worstFrameCount;
            i < sortedFrameTimes.Count;
            i++
        )
        {
            worstFrameTotal += sortedFrameTimes[i];
        }

        double averageWorstFrameTime = worstFrameTotal / worstFrameCount;
        return averageWorstFrameTime > 0d
            ? (float)(1000d / averageWorstFrameTime)
            : 0f;
    }

    private void AppendCsv(BenchmarkResult result)
    {
        string path = Path.Combine(
            Application.persistentDataPath,
            CsvFileName
        );
        bool writeHeader = !File.Exists(path);

        using (StreamWriter writer = new StreamWriter(path, true, Encoding.UTF8))
        {
            if (writeHeader)
            {
                writer.WriteLine(BenchmarkResult.CsvHeader);
            }

            writer.WriteLine(result.ToCsv());
        }
    }

    private static string FormatSummary(BenchmarkResult result)
    {
        return
            $"[Projectile Benchmark] {result.Mode}/{result.Scenario} " +
            $"run {result.Run}: avg {result.AverageFps:F1} FPS, " +
            $"1% low {result.OnePercentLowFps:F1} FPS, " +
            $"P95 {result.P95FrameTimeMs:F2} ms, " +
            $"GC {FormatBytes(result.TotalGcAllocatedBytes)} total, " +
            $"memory peak {FormatBytes(result.PeakMemoryBytes)}, " +
            $"created {result.ProjectilesCreated}, " +
            $"reused {result.ProjectilesReused}.";
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 0)
        {
            return "N/A";
        }

        return (bytes / (1024f * 1024f)).ToString("F2") + " MB";
    }

    private void OnGUI()
    {
        if (!CanRunBenchmark ||
            isMeasuring ||
            (!runOnStart && benchmarkCoroutine == null))
        {
            return;
        }

        const int width = 760;
        int height = 90 + completedRunSummaries.Count * 34;
        Rect area = new Rect(20f, 20f, width, height);

        GUI.Box(area, GUIContent.none);
        GUILayout.BeginArea(new Rect(32f, 30f, width - 24f, height - 20f));
        GUILayout.Label(
            $"Projectile Benchmark: {projectilePool.LifecycleMode} / {scenario}"
        );
        GUILayout.Label(currentStatus);

        for (int i = 0; i < completedRunSummaries.Count; i++)
        {
            GUILayout.Label(completedRunSummaries[i]);
        }

        GUILayout.EndArea();
    }

    private void OnDisable()
    {
        if (benchmarkCoroutine != null)
        {
            StopCoroutine(benchmarkCoroutine);
            benchmarkCoroutine = null;
        }

        isMeasuring = false;
        DisposeRecorders();
        RestoreBenchmarkEnvironment();
    }

    private void OnDestroy()
    {
        DisposeRecorders();
        RestoreBenchmarkEnvironment();
    }

    private void DisposeRecorders()
    {
        if (gcAllocatedRecorder.Valid)
        {
            gcAllocatedRecorder.Dispose();
        }

        if (systemMemoryRecorder.Valid)
        {
            systemMemoryRecorder.Dispose();
        }
    }

    [Serializable]
    private struct BenchmarkResult
    {
        public const string CsvHeader =
            "timestamp_utc,device,unity_version,mode,scenario,run," +
            "duration_seconds,samples,average_fps,one_percent_low_fps," +
            "average_frame_ms,p95_frame_ms,max_frame_ms,total_gc_bytes," +
            "peak_gc_per_frame_bytes,memory_start_bytes,peak_memory_bytes," +
            "memory_end_bytes,projectiles_created,projectiles_reused," +
            "peak_active_projectiles";

        public string TimestampUtc;
        public string Device;
        public string UnityVersion;
        public string Mode;
        public string Scenario;
        public int Run;
        public float DurationSeconds;
        public int SampleCount;
        public float AverageFps;
        public float OnePercentLowFps;
        public float AverageFrameTimeMs;
        public float P95FrameTimeMs;
        public float MaximumFrameTimeMs;
        public long TotalGcAllocatedBytes;
        public long PeakGcAllocatedPerFrameBytes;
        public long MemoryAtStartBytes;
        public long PeakMemoryBytes;
        public long MemoryAtEndBytes;
        public long ProjectilesCreated;
        public long ProjectilesReused;
        public int PeakActiveProjectiles;

        public string ToCsv()
        {
            CultureInfo culture = CultureInfo.InvariantCulture;

            return string.Join(
                ",",
                Escape(TimestampUtc),
                Escape(Device),
                Escape(UnityVersion),
                Escape(Mode),
                Escape(Scenario),
                Run.ToString(culture),
                DurationSeconds.ToString("F2", culture),
                SampleCount.ToString(culture),
                AverageFps.ToString("F3", culture),
                OnePercentLowFps.ToString("F3", culture),
                AverageFrameTimeMs.ToString("F3", culture),
                P95FrameTimeMs.ToString("F3", culture),
                MaximumFrameTimeMs.ToString("F3", culture),
                TotalGcAllocatedBytes.ToString(culture),
                PeakGcAllocatedPerFrameBytes.ToString(culture),
                MemoryAtStartBytes.ToString(culture),
                PeakMemoryBytes.ToString(culture),
                MemoryAtEndBytes.ToString(culture),
                ProjectilesCreated.ToString(culture),
                ProjectilesReused.ToString(culture),
                PeakActiveProjectiles.ToString(culture)
            );
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
#else
using UnityEngine;

[RequireComponent(typeof(ProjectilePool))]
public class ProjectilePoolingBenchmark : MonoBehaviour
{
}
#endif
