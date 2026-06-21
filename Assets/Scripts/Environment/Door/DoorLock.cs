using DG.Tweening;
using UnityEngine;

public class DoorLock : MonoBehaviour
{
    [Header("Split Door")]
    [SerializeField] private Transform leftBody;
    [SerializeField] private Transform rightBody;
    [SerializeField] private Transform centerLock;

    [Header("Blocking")]
    [SerializeField] private Collider2D[] blockingColliders;
    [SerializeField] private float blockingDisableDelay = 0.35f;
    [SerializeField] private float openDuration = 1.5f;
    [SerializeField] private Ease openEase = Ease.InOutSine;

    private bool isOpen;
    private bool blockingDisabled;
    private Vector3 leftBodyClosedScale;
    private Vector3 rightBodyClosedScale;
    private Vector3 centerLockClosedScale;

    private void Awake()
    {
        if (leftBody != null)
        {
            leftBodyClosedScale = leftBody.localScale;
        }

        if (rightBody != null)
        {
            rightBodyClosedScale = rightBody.localScale;
        }

        if (centerLock != null)
        {
            centerLockClosedScale = centerLock.localScale;
        }

        if (blockingColliders == null || blockingColliders.Length == 0)
        {
            blockingColliders = GetComponentsInChildren<Collider2D>();
        }
    }

    public void Open()
    {
        if (isOpen)
        {
            return;
        }

        if (leftBody == null || rightBody == null)
        {
            Debug.LogWarning("DoorLock cannot open because door body transforms are missing.", this);
            return;
        }

        isOpen = true;
        blockingDisabled = false;
        leftBody.DOKill();
        rightBody.DOKill();
        centerLock?.DOKill();
        leftBody.localScale = leftBodyClosedScale;
        rightBody.localScale = rightBodyClosedScale;

        float duration = Mathf.Max(0.01f, openDuration);
        Sequence sequence = DOTween.Sequence()
            .SetEase(openEase)
            .SetLink(gameObject);

        if (centerLock != null)
        {
            centerLock.localScale = centerLockClosedScale;
            sequence.Join(
                centerLock
                    .DOScale(Vector3.zero, Mathf.Min(0.25f, duration * 0.35f))
                    .SetEase(Ease.InBack)
            );
        }

        sequence.Join(
            leftBody.DOScaleX(0f, duration).SetEase(openEase)
        );
        sequence.Join(
            rightBody.DOScaleX(0f, duration).SetEase(openEase)
        );
        sequence.InsertCallback(Mathf.Min(blockingDisableDelay, duration), DisableBlockingColliders);
        sequence.OnComplete(DisableBlockingColliders);
    }

    private void DisableBlockingColliders()
    {
        if (blockingDisabled)
        {
            return;
        }

        blockingDisabled = true;

        foreach (Collider2D blockingCollider in blockingColliders)
        {
            if (blockingCollider != null)
            {
                blockingCollider.enabled = false;
            }
        }
    }

    private void OnValidate()
    {
        blockingDisableDelay = Mathf.Max(0f, blockingDisableDelay);
        openDuration = Mathf.Max(0.01f, openDuration);
    }
}
