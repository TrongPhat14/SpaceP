using UnityEngine;

public class PlayerFollowerShooter : MonoBehaviour
{
    [SerializeField] private Transform aimTransform;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float followDistance = 6f;
    [SerializeField] private float stopDistance = 3f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float fireInterval = 1.5f;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float aimRotationOffset = -90f;

    private Transform target;
    private ProjectilePool projectilePool;
    private float fireTimer;
    private bool loggedMissingPool;

    private void Start()
    {
        if (PlayerMovement.Instance != null)
        {
            target = PlayerMovement.Instance.transform;
        }

        if (projectilePool == null)
        {
            projectilePool = FindFirstObjectByType<ProjectilePool>();
        }
    }

    private void Update()
    {
        if (target == null || firePoint == null)
        {
            return;
        }

        if (projectilePool == null)
        {
            if (!loggedMissingPool)
            {
                Debug.LogError("PlayerFollowerShooter needs a ProjectilePool in this level.");
                loggedMissingPool = true;
            }

            return;
        }

        Vector2 toTarget = target.position - transform.position;
        float distance = toTarget.magnitude;

        RotateTowardTarget(toTarget);

        if (distance > stopDistance && distance < followDistance)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                target.position,
                moveSpeed * Time.deltaTime
            );
        }

        if (distance <= followDistance)
        {
            fireTimer -= Time.deltaTime;

            if (fireTimer <= 0f)
            {
                Fire(toTarget.normalized);
                fireTimer = fireInterval;
            }
        }
    }

    private void RotateTowardTarget(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Transform targetTransform = aimTransform != null ? aimTransform : transform;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        targetTransform.rotation = Quaternion.Euler(0f, 0f, angle + aimRotationOffset);
    }

    private void Fire(Vector2 direction)
    {
        EnemyProjectile projectile = projectilePool.Get(firePoint.position, Quaternion.identity);

        if (projectile != null)
        {
            projectile.Launch(direction, projectileSpeed);
        }
    }
}
