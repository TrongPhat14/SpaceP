# Projectile Pooling Benchmark

This benchmark compares the current projectile pool with an
`Instantiate`/`Destroy` baseline. Run both configurations on the same Android
device before making performance claims in the CV.

## Build setup

1. Open `Assets/Prefabs/Level_1.prefab`.
2. Select the `ProjectilePool` object.
3. In `Projectile Pool`, choose one `Lifecycle Mode`:
   - `Pooling`
   - `Instantiate Destroy`
4. In `Projectile Pooling Benchmark`:
   - Enable `Run On Start`.
   - Keep `Repetitions = 3`.
   - Keep `Warmup Seconds = 15`.
   - Keep `Measurement Seconds = 60`.
   - Select `Gameplay` or `Stress`.
5. Build an Android **Development Build** with **Autoconnect Profiler** enabled
   and **Deep Profiling** disabled.

The benchmark is disabled in non-development builds. Release builds always use
pooling even if the prefab contains the baseline enum value.

## Test matrix

Build and run all four configurations:

| Mode | Scenario | Spawn interval | Runs |
|---|---|---:|---:|
| Pooling | Gameplay | 1.50 s | 3 |
| InstantiateDestroy | Gameplay | 1.50 s | 3 |
| Pooling | Stress | 0.05 s | 3 |
| InstantiateDestroy | Stress | 0.05 s | 3 |

Use the same phone, quality level, resolution, and game version. Close
background applications and let the phone cool down before each configuration.
Do not interact with the screen during the 60-second measurement.

## Output

Each run is printed to the Console and appended to:

`Application.persistentDataPath/projectile-pooling-benchmark.csv`

The in-game overlay shows completed runs and the CSV location. The CSV contains:

- Average FPS and 1% low FPS
- Average, P95, and maximum frame time
- Total GC allocation and peak GC allocation per frame
- Memory at start, peak memory, and memory at end
- Projectiles created, reused, and peak active projectiles

Synthetic benchmark projectiles have collision disabled so they always complete
the same four-second lifetime instead of colliding with one another. Normal
enemy projectiles continue to use their collider.

Use the median of the three runs for each metric. Keep screenshots or Profiler
captures for both modes as evidence.

## Acceptance criteria

- Pooling creates no additional projectiles during measurement after warm-up
  when the pool has reached the required capacity.
- Pooling reports reused projectiles while the baseline reports zero reuse.
- Memory does not increase continuously across runs.
- Compare P95 frame time and 1% low FPS instead of relying only on average FPS.
- Only claim an FPS percentage if the difference is consistent across all
  three runs.

If FPS is effectively equal, use this CV wording:

> Implemented Object Pooling to reduce repeated projectile instantiation and
> runtime allocations during Android gameplay.

After verified measurements, replace it with a quantified statement that names
the device and test duration.
