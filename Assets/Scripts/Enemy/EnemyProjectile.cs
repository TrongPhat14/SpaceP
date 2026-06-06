using System.Collections;
using DG.Tweening;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 4f;

    private Rigidbody2D rb;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private Collider2D projectileCollider;
#endif

    private ProjectilePool projectilePool;
    private Coroutine lifeCoroutine;
    private Vector3 originalScale;
    private bool isReleased;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        projectileCollider = GetComponent<Collider2D>();
#endif

        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        isReleased = false;
        transform.DOKill();
        transform.localScale = originalScale * 0.7f;
        transform.DOScale(originalScale, 0.12f).SetLink(gameObject).SetEase(Ease.OutBack);
        lifeCoroutine = StartCoroutine(LifeRoutine());
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

    public void SetCollisionEnabled(bool isEnabled)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (projectileCollider != null)
        {
            projectileCollider.enabled = isEnabled;
        }
#endif
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
        transform.DOKill();
        transform.localScale = originalScale;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        SetCollisionEnabled(true);
#endif
    }

    private void Release()
    {
        if (isReleased)
        {
            return;
        }

        isReleased = true;

        if (lifeCoroutine != null)
        {
            StopCoroutine(lifeCoroutine);
            lifeCoroutine = null;
        }

        if (projectilePool != null)
        {
            projectilePool.Release(this);
            return;
        }

        Destroy(gameObject);
    }

    private IEnumerator LifeRoutine()
    {
        yield return new WaitForSeconds(lifeTime);

        Release();
    }
}
