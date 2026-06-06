using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public enum ProjectileLifecycleMode
    {
        Pooling,
        InstantiateDestroy
    }

    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private int initialSize = 10;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [Header("Development Benchmark")]
    [SerializeField] private ProjectileLifecycleMode lifecycleMode =
        ProjectileLifecycleMode.Pooling;
#endif

    private readonly Queue<EnemyProjectile> availableProjectiles = new Queue<EnemyProjectile>();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private readonly HashSet<EnemyProjectile> activeProjectiles = new HashSet<EnemyProjectile>();

    private long totalCreated;
    private long totalReused;
    private int peakActive;
#endif

    public ProjectileLifecycleMode LifecycleMode
    {
        get
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return lifecycleMode;
#else
            return ProjectileLifecycleMode.Pooling;
#endif
        }
    }

    private void Awake()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("ProjectilePool is missing a projectile prefab.");
            return;
        }

        if (LifecycleMode != ProjectileLifecycleMode.Pooling)
        {
            return;
        }

        for (int i = 0; i < initialSize; i++)
        {
            EnemyProjectile projectile = CreateProjectile();
            projectile.gameObject.SetActive(false);
            availableProjectiles.Enqueue(projectile);
        }
    }

    public EnemyProjectile Get(Vector3 position, Quaternion rotation)
    {
        if (projectilePrefab == null)
        {
            return null;
        }

        EnemyProjectile projectile;

        if (LifecycleMode == ProjectileLifecycleMode.Pooling &&
            availableProjectiles.Count > 0)
        {
            projectile = availableProjectiles.Dequeue();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            totalReused++;
#endif
        }
        else
        {
            projectile = CreateProjectile();
        }

        projectile.transform.SetPositionAndRotation(position, rotation);
        projectile.gameObject.SetActive(true);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        activeProjectiles.Add(projectile);
        peakActive = Mathf.Max(peakActive, activeProjectiles.Count);
#endif

        return projectile;
    }

    public void Release(EnemyProjectile projectile)
    {
        if (projectile == null)
        {
            return;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        activeProjectiles.Remove(projectile);
#endif

        projectile.ResetProjectile();

        if (LifecycleMode == ProjectileLifecycleMode.Pooling)
        {
            projectile.gameObject.SetActive(false);
            availableProjectiles.Enqueue(projectile);
            return;
        }

        Destroy(projectile.gameObject);
    }

    public ProjectilePoolStatistics GetStatistics()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        return new ProjectilePoolStatistics(
            totalCreated,
            totalReused,
            activeProjectiles.Count,
            peakActive
        );
#else
        return default;
#endif
    }

    public void ResetPeakActiveCount()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        peakActive = activeProjectiles.Count;
#endif
    }

    private EnemyProjectile CreateProjectile()
    {
        EnemyProjectile projectile = Instantiate(projectilePrefab, transform);
        projectile.SetPool(this);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        totalCreated++;
#endif

        return projectile;
    }
}

public readonly struct ProjectilePoolStatistics
{
    public readonly long TotalCreated;
    public readonly long TotalReused;
    public readonly int Active;
    public readonly int PeakActive;

    public ProjectilePoolStatistics(
        long totalCreated,
        long totalReused,
        int active,
        int peakActive
    )
    {
        TotalCreated = totalCreated;
        TotalReused = totalReused;
        Active = active;
        PeakActive = peakActive;
    }
}
