using DG.Tweening;
using UnityEngine;

public class CoinPickUp : MonoBehaviour
{
    private bool isDestroying;

    public void DestroyCoin()
    {
        if (isDestroying)
        {
            return;
        }

        isDestroying = true;
        DisableCollider();

        transform.DOKill();
        Sequence sequence = DOTween.Sequence().SetLink(gameObject);
        sequence.Join(transform.DOScale(Vector3.zero, 0.22f).SetEase(Ease.InBack));
        sequence.Join(transform.DORotate(new Vector3(0f, 0f, 360f), 0.22f, RotateMode.FastBeyond360));
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
