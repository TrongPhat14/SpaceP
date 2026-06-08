using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class ProjectilePoolingBenchmarkBuilder
{
    private const string PrefabPath = "Assets/Prefabs/Level_1.prefab";
    private const string OutputDirectory = "Builds/Benchmark";
    private const string FirebaseConfigPath = "Assets/google-services.json";

    private static readonly BenchmarkBuild[] Builds =
    {
        new BenchmarkBuild(
            "SpaceP-Pooling-Gameplay.apk",
            ProjectilePool.ProjectileLifecycleMode.Pooling,
            ProjectilePoolingBenchmark.BenchmarkScenario.Gameplay
        ),
        new BenchmarkBuild(
            "SpaceP-Instantiate-Gameplay.apk",
            ProjectilePool.ProjectileLifecycleMode.InstantiateDestroy,
            ProjectilePoolingBenchmark.BenchmarkScenario.Gameplay
        ),
        new BenchmarkBuild(
            "SpaceP-Instantiate-Stress.apk",
            ProjectilePool.ProjectileLifecycleMode.InstantiateDestroy,
            ProjectilePoolingBenchmark.BenchmarkScenario.Stress
        ),
        new BenchmarkBuild(
            "SpaceP-Pooling-Stress.apk",
            ProjectilePool.ProjectileLifecycleMode.Pooling,
            ProjectilePoolingBenchmark.BenchmarkScenario.Stress
        )
    };

    [MenuItem("Tools/Performance/Build Projectile Pooling Benchmarks")]
    public static void BuildAllFromMenu()
    {
        try
        {
            BuildAll();
            EditorUtility.DisplayDialog(
                "Projectile Benchmark",
                "Four Android benchmark APKs were built in Builds/Benchmark.",
                "OK"
            );
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorUtility.DisplayDialog(
                "Projectile Benchmark Build Failed",
                exception.Message,
                "OK"
            );
        }
    }

    public static void BuildAllFromCommandLine()
    {
        BuildAll();
    }

    private static void BuildAll()
    {
        ValidateBuildEnvironment();

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string prefabAbsolutePath = Path.Combine(projectRoot, PrefabPath);
        string outputAbsolutePath = Path.Combine(projectRoot, OutputDirectory);
        byte[] originalPrefabBytes = File.ReadAllBytes(prefabAbsolutePath);
        bool previousBuildAppBundle = EditorUserBuildSettings.buildAppBundle;

        Directory.CreateDirectory(outputAbsolutePath);

        try
        {
            EditorUserBuildSettings.buildAppBundle = false;

            foreach (BenchmarkBuild build in Builds)
            {
                ApplyBenchmarkConfiguration(build);
                BuildApk(build, outputAbsolutePath);
            }

            WriteBuildManifest(outputAbsolutePath);
            Debug.Log(
                $"Projectile benchmark builds completed: {outputAbsolutePath}"
            );
        }
        finally
        {
            EditorUserBuildSettings.buildAppBundle = previousBuildAppBundle;
            File.WriteAllBytes(prefabAbsolutePath, originalPrefabBytes);
            AssetDatabase.ImportAsset(
                PrefabPath,
                ImportAssetOptions.ForceUpdate
            );
        }
    }

    private static void ValidateBuildEnvironment()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            throw new InvalidOperationException(
                "Exit Play Mode before building benchmark APKs."
            );
        }

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            throw new InvalidOperationException(
                "Switch the active Build Profile to Android first."
            );
        }

        if (!File.Exists(Path.Combine(projectRoot, PrefabPath)))
        {
            throw new FileNotFoundException(
                "Level_1 prefab was not found.",
                PrefabPath
            );
        }

        if (!File.Exists(Path.Combine(projectRoot, FirebaseConfigPath)))
        {
            throw new FileNotFoundException(
                "The local Firebase Android config is missing.",
                FirebaseConfigPath
            );
        }

        if (GetEnabledScenePaths().Length == 0)
        {
            throw new InvalidOperationException(
                "No enabled scenes were found in Build Settings."
            );
        }
    }

    private static void ApplyBenchmarkConfiguration(BenchmarkBuild build)
    {
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);

        try
        {
            ProjectilePool pool =
                prefabRoot.GetComponentInChildren<ProjectilePool>(true);
            ProjectilePoolingBenchmark benchmark =
                prefabRoot.GetComponentInChildren<ProjectilePoolingBenchmark>(
                    true
                );

            if (pool == null || benchmark == null)
            {
                throw new InvalidOperationException(
                    "Level_1 must contain ProjectilePool and ProjectilePoolingBenchmark."
                );
            }

            SerializedObject poolObject = new SerializedObject(pool);
            SetEnum(
                poolObject,
                "lifecycleMode",
                (int)build.LifecycleMode
            );
            poolObject.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject benchmarkObject = new SerializedObject(benchmark);
            SetBool(benchmarkObject, "runOnStart", true);
            SetEnum(benchmarkObject, "scenario", (int)build.Scenario);
            SetInt(benchmarkObject, "repetitions", 3);
            SetFloat(benchmarkObject, "warmupSeconds", 15f);
            SetFloat(benchmarkObject, "measurementSeconds", 60f);
            SetFloat(benchmarkObject, "cooldownSeconds", 5f);
            SetFloat(benchmarkObject, "poolDrainTimeoutSeconds", 10f);
            SetFloat(benchmarkObject, "gameplaySpawnInterval", 1.5f);
            SetFloat(benchmarkObject, "stressSpawnInterval", 0.05f);
            SetFloat(benchmarkObject, "projectileSpeed", 8f);
            benchmarkObject.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        AssetDatabase.SaveAssets();
        Debug.Log(
            $"Configured benchmark: {build.LifecycleMode}/{build.Scenario}"
        );
    }

    private static void BuildApk(
        BenchmarkBuild build,
        string outputAbsolutePath
    )
    {
        string apkPath = Path.Combine(outputAbsolutePath, build.FileName);
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = GetEnabledScenePaths(),
            locationPathName = apkPath,
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = BuildOptions.Development |
                BuildOptions.ConnectWithProfiler
        };

        Debug.Log($"Building benchmark APK: {apkPath}");
        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Build failed for {build.FileName}: {report.summary.result}"
            );
        }

        Debug.Log(
            $"Built {build.FileName} in {report.summary.totalTime}. " +
            $"Size: {report.summary.totalSize / (1024f * 1024f):F2} MB"
        );
    }

    private static string[] GetEnabledScenePaths()
    {
        return EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
    }

    private static void WriteBuildManifest(string outputAbsolutePath)
    {
        List<string> lines = new List<string>
        {
            $"Built UTC: {DateTime.UtcNow:O}",
            $"Unity: {Application.unityVersion}",
            $"Package: {PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android)}",
            "Build options: Development, ConnectWithProfiler",
            "Deep profiling: disabled",
            "Script debugging: disabled",
            ""
        };

        lines.AddRange(
            Builds.Select(
                build =>
                    $"{build.FileName}: {build.LifecycleMode}/{build.Scenario}"
            )
        );

        File.WriteAllLines(
            Path.Combine(outputAbsolutePath, "build-manifest.txt"),
            lines
        );
    }

    private static void SetBool(
        SerializedObject serializedObject,
        string propertyName,
        bool value
    )
    {
        SerializedProperty property = RequireProperty(
            serializedObject,
            propertyName
        );
        property.boolValue = value;
    }

    private static void SetInt(
        SerializedObject serializedObject,
        string propertyName,
        int value
    )
    {
        SerializedProperty property = RequireProperty(
            serializedObject,
            propertyName
        );
        property.intValue = value;
    }

    private static void SetFloat(
        SerializedObject serializedObject,
        string propertyName,
        float value
    )
    {
        SerializedProperty property = RequireProperty(
            serializedObject,
            propertyName
        );
        property.floatValue = value;
    }

    private static void SetEnum(
        SerializedObject serializedObject,
        string propertyName,
        int value
    )
    {
        SerializedProperty property = RequireProperty(
            serializedObject,
            propertyName
        );
        property.enumValueIndex = value;
    }

    private static SerializedProperty RequireProperty(
        SerializedObject serializedObject,
        string propertyName
    )
    {
        SerializedProperty property =
            serializedObject.FindProperty(propertyName);

        if (property == null)
        {
            throw new InvalidOperationException(
                $"Missing serialized property '{propertyName}' on " +
                $"{serializedObject.targetObject.GetType().Name}."
            );
        }

        return property;
    }

    private readonly struct BenchmarkBuild
    {
        public readonly string FileName;
        public readonly ProjectilePool.ProjectileLifecycleMode LifecycleMode;
        public readonly ProjectilePoolingBenchmark.BenchmarkScenario Scenario;

        public BenchmarkBuild(
            string fileName,
            ProjectilePool.ProjectileLifecycleMode lifecycleMode,
            ProjectilePoolingBenchmark.BenchmarkScenario scenario
        )
        {
            FileName = fileName;
            LifecycleMode = lifecycleMode;
            Scenario = scenario;
        }
    }
}
