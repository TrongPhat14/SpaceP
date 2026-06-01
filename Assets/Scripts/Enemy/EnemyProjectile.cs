using System.Collections;
using DG.Tweening;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 4f;

    private Rigidbody2D rb;
    private ProjectilePool projectilePool;
    private Coroutine lifeCoroutine;
    private Vector3 originalScale;
    private bool isReleased;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
