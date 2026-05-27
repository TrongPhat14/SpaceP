using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private int initialSize = 10;

    private readonly Queue<EnemyProjectile> availableProjectiles = new Queue<EnemyProjectile>();

    private void Awake()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("ProjectilePool is missing a projectile prefab.");
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

        EnemyProjectile projectile = availableProjectiles.Count > 0
            ? availableProjectiles.Dequeue()
            : CreateProjectile();

        projectile.transform.SetPositionAndRotation(position, rotation);
        projectile.gameObject.SetActive(true);
        return projectile;
    }

    public void Release(EnemyProjectile projectile)
    {
        if (projectile == null)
        {
            return;
        }

        projectile.ResetProjectile();
        projectile.gameObject.SetActive(false);
        availableProjectiles.Enqueue(projectile);
    }

    private EnemyProjectile CreateProjectile()
    {
        EnemyProjectile projectile = Instantiate(projectilePrefab, transform);
        projectile.SetPool(this);
        return projectile;
    }
}
