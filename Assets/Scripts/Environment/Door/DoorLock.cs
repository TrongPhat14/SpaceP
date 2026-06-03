using DG.Tweening;
using UnityEngine;

public class DoorLock : MonoBehaviour
{
    [SerializeField] private Transform doorTransform;
    [SerializeField] private Collider2D[] blockingColliders;
    [SerializeField] private Vector3 openLocalOffset = new Vector3(2.5f, 0f, 0f);
    [SerializeField] private float openDuration = 1.5f;
    [SerializeField] private Ease openEase = Ease.InOutSine;

    private bool isOpen;

    private void Awake()
    {
        if (doorTransform == null)
        {
            doorTransform = transform;
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

        if (doorTransform == null)
        {
            Debug.LogWarning("DoorLock cannot open because doorTransform is missing.", this);
            return;
        }

        isOpen = true;

        doorTransform.DOKill();

        Tween openTween;
        float duration = Mathf.Max(0.01f, openDuration);

        if (doorTransform is RectTransform doorRectTransform)
        {
            Vector2 targetPosition = doorRectTransform.anchoredPosition + new Vector2(openLocalOffset.x, openLocalOffset.y);

            openTween = doorRectTransform.DOAnchorPos(targetPosition, duration);
        }
        else
        {
            Vector3 targetPosition = doorTransform.localPosition + openLocalOffset;

            openTween = doorTransform.DOLocalMove(targetPosition, duration);
        }

        openTween
            .SetEase(openEase)
            .SetLink(doorTransform.gameObject)
            .OnComplete(DisableBlockingColliders);
    }

    private void DisableBlockingColliders()
    {
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
        openDuration = Mathf.Max(0.01f, openDuration);
    }
}
