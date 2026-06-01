using DG.Tweening;
using UnityEngine;

public class FuelPickUp : MonoBehaviour
{
    private bool isDestroying;

    public void DestroyFuel()
    {
        if (isDestroying)
        {
            return;
        }

        isDestroying = true;
        DisableCollider();

        transform.DOKill();
        Sequence sequence = DOTween.Sequence().SetLink(gameObject);
        sequence.Append(transform.DOPunchScale(Vector3.one * 0.18f, 0.12f, 8, 0.8f));
        sequence.Append(transform.DOScale(Vector3.zero, 0.18f).SetEase(Ease.InBack));
        sequence.OnComplete(() => Destroy(gameObject));
    }

    private void DisableCollider()
    {
        Collider2D pickupCollider = GetComponent<Collider2D>();

        if (pickupCollider != null)
        {
            pickupCollider.enabled = false;
        }
    }
}
