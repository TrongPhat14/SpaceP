using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 4f;

    private Rigidbody2D rb;
    private ProjectilePool projectilePool;
    private float lifeTimer;
    private bool isReleased;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        lifeTimer = lifeTime;
        isReleased = false;
    }

    private void Update()
    {
        lifeTimer -= Time.deltaTime;

        if (lifeTimer <= 0f)
        {
            Release();
        }
    }

    public void SetPool(ProjectilePool projectilePool)
    {
        this.projectilePool = projectilePool;
    }

    public void Launch(Vector2 direction, float speed)
    {
        if (rb == null)
        {
            return;
        }

        rb.linearVelocity = direction.normalized * speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Release();
    }

    public void ResetProjectile()
    {
        if (rb == null)
        {
            return;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void Release()
    {
        if (isReleased)
        {
            return;
        }

        isReleased = true;

        if (projectilePool != null)
        {
            projectilePool.Release(this);
            return;
        }

        Destroy(gameObject);
    }
}
