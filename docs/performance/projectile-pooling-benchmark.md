# Projectile Pooling Android Benchmark

This benchmark compares `ProjectilePool` against an `Instantiate`/`Destroy`
baseline on the same Android phone. Do not use benchmark results from the Unity
Editor in the CV.

## Automated build

1. Switch the active Unity Build Profile to Android.
2. Exit Play Mode.
3. Confirm the ignored local file `Assets/google-services.json` exists.
4. Select:

   `Tools > Performance > Build Projectile Pooling Benchmarks`

The builder creates these Development APKs with `ConnectWithProfiler` enabled:

| Order | APK | Mode | Scenario | Spawn interval |
|---:|---|---|---|---:|
| 1 | `SpaceP-Pooling-Gameplay.apk` | Pooling | Gameplay | 1.50 s |
| 2 | `SpaceP-Instantiate-Gameplay.apk` | InstantiateDestroy | Gameplay | 1.50 s |
| 3 | `SpaceP-Instantiate-Stress.apk` | InstantiateDestroy | Stress | 0.05 s |
| 4 | `SpaceP-Pooling-Stress.apk` | Pooling | Stress | 0.05 s |

Output:

`Builds/Benchmark`

The builder configures three runs, 15-second warm-up, 60-second measurement,
5-second cooldown and 10-second drain timeout. It restores the exact original
`Level_1.prefab` bytes after success or failure. Release builds always use
pooling and contain no active benchmark logic.

## Wireless ADB

The computer and phone must use the same Wi-Fi. On Android, enable Developer
Options and open:

`Wireless debugging > Pair device with pairing code`

From the project root:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\projectile-benchmark.ps1 `
  -Action Pair -Endpoint "PHONE_IP:PAIRING_PORT"
```

Enter the pairing code shown by Android. Then use the separate debugging port:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\projectile-benchmark.ps1 `
  -Action Connect -Endpoint "PHONE_IP:DEBUG_PORT"

powershell -ExecutionPolicy Bypass -File .\tools\projectile-benchmark.ps1 `
  -Action Devices
```

The device status must be `device`, not `offline` or `unauthorized`. Android can
change the debugging port after Wi-Fi or the phone restarts.

## Test preparation

- Keep phone battery above 50%.
- Disable battery saver and close background applications.
- Keep brightness, display resolution and refresh rate unchanged.
- Do not charge the phone during a measured run.
- Use the same phone, Android version and Quality Level for all APKs.
- Android Quality Level 2 uses VSync, so prioritize P95 frame time, 1% low FPS,
  GC and memory in addition to average FPS.
- Open `Window > Analysis > Profiler`, select the Android Player, and record CPU
  Usage and Memory screenshots for each configuration.

Create a clean CSV only before the first configuration:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\projectile-benchmark.ps1 `
  -Action Install `
  -ApkPath ".\Builds\Benchmark\SpaceP-Pooling-Gameplay.apk"

powershell -ExecutionPolicy Bypass -File .\tools\projectile-benchmark.ps1 `
  -Action Clear
```

Do not uninstall the application or clear its data between configurations.
Install each following APK with `Install`, which uses `adb install -r`.

## Run procedure

For each APK:

1. Install it with the helper script.
2. Start the game and choose `New Game` so Level 1 is loaded.
3. Do not touch the screen, pause, background the app, or change settings.
4. Wait for the completion overlay. One configuration takes about four minutes.
5. Capture the Unity Profiler CPU and Memory views.
6. Pull a cumulative CSV snapshot.
7. Let the phone cool for three to five minutes before the next APK.

The benchmark forces `Time.timeScale = 1`, disables all
`PlayerFollowerShooter` components, disables synthetic projectile collisions,
and waits for active projectiles to drain between runs. All states are restored
when the benchmark completes or is disabled.

Pull snapshots using these names:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\projectile-benchmark.ps1 `
  -Action Pull -Name "01-pooling-gameplay"

powershell -ExecutionPolicy Bypass -File .\tools\projectile-benchmark.ps1 `
  -Action Pull -Name "02-instantiate-gameplay"

powershell -ExecutionPolicy Bypass -File .\tools\projectile-benchmark.ps1 `
  -Action Pull -Name "03-instantiate-stress"

powershell -ExecutionPolicy Bypass -File .\tools\projectile-benchmark.ps1 `
  -Action Pull -Name "04-pooling-stress-final"
```

Snapshots are written to:

`Temp/BenchmarkResults`

On Android, the source CSV is stored at:

`/sdcard/Android/data/com.trongphat.spacep/files/projectile-pooling-benchmark.csv`

The final CSV must contain 12 rows: four configurations times three runs.

## Calculate medians

Generate the median table and percentage comparisons from the final snapshot:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\projectile-benchmark.ps1 `
  -Action Summarize `
  -InputPath ".\Temp\BenchmarkResults\04-pooling-stress-final.csv"
```

Output:

`Temp/BenchmarkResults/benchmark-summary.md`

The summary calculates median average FPS, 1% low FPS, P95 frame time, total
GC, peak GC/frame, memory delta, created/reused count and peak active count. It
also reports:

```text
FPS improvement =
(Pooling FPS - Baseline FPS) / Baseline FPS * 100

P95 reduction =
(Baseline P95 - Pooling P95) / Baseline P95 * 100

GC reduction =
(Baseline GC - Pooling GC) / Baseline GC * 100
```

## Expected validation

- Each Mode/Scenario group contains exactly three runs.
- Pooling reports reused projectiles.
- InstantiateDestroy reports zero reused projectiles.
- Pooling creates approximately zero new projectiles during measurement after
  warm-up.
- Gameplay creates/reuses approximately 40 projectiles per measured run.
- Stress creates/reuses approximately 1,200 projectiles per measured run.
- Memory does not increase continuously across all three runs.
- The Console contains no drain timeout.
- FPS/P95 direction is consistent across the three runs.

Do not claim a memory leak from `System Used Memory` alone because Android and
other systems affect that counter. Do not claim `0 B GC Alloc/frame` because
Coroutine, `WaitForSeconds`, DOTween and other game systems may allocate.

## Result record

| Field | Value |
|---|---|
| Phone | realme RMX3195 |
| Android version | Android 13 (API 33) |
| Unity version | 6000.3.10f1 |
| Quality Level | 2 |
| Runs per configuration | 3 |
| Warm-up / measurement | 15 s / 60 s |

### Median results

Raw measurements:
[projectile-pooling-results.csv](results/projectile-pooling-results.csv)

| Mode | Scenario | Avg FPS | 1% Low | P95 ms | GC total bytes | GC peak/frame | Created | Reused |
|---|---|---:|---:|---:|---:|---:|---:|---:|
| InstantiateDestroy | Gameplay | 30.47 | 28.44 | 32.94 | 2,382,630 | 2,384 | 40 | 0 |
| Pooling | Gameplay | 30.44 | 23.55 | 33.09 | 2,374,454 | 2,792 | 0 | 40 |
| InstantiateDestroy | Stress | 30.20 | 15.29 | 33.06 | 3,477,590 | 8,960 | 1,200 | 0 |
| Pooling | Stress | 30.44 | 25.21 | 33.03 | 3,271,950 | 3,160 | 0 | 1,200 |

### Interpretation

- Gameplay load showed no meaningful FPS, P95, or GC difference.
- Stress load improved median average FPS by only `0.77%` and P95 by `0.10%`,
  so no FPS improvement should be claimed.
- Stress pooling reused `1,200` projectiles per 60-second run and avoided
  `1,200` measured `Instantiate`/`Destroy` cycles.
- Stress median total GC was `5.91%` lower and median peak GC/frame was about
  `64.7%` lower, but whole-frame GC includes Coroutine, DOTween and other game
  systems.
- Some runs contained isolated frame spikes. Median P95 remained close to
  `33 ms`, but the 1% low result is not strong enough for a CV claim.
- System memory varied significantly between runs due to Android and Profiler
  behavior, so memory delta is retained in the raw CSV but excluded from the
  conclusion.

Only claim a percentage in the CV when median FPS improves by roughly 5% or
more, all three runs have the same direction, and P95 or 1% low also improves.

Use this verified wording:

> Implemented Object Pooling to reduce repeated enemy projectile instantiation
> and runtime allocations, reusing 1,200 projectiles during a 60-second Android
> stress test instead of creating and destroying each shot.
