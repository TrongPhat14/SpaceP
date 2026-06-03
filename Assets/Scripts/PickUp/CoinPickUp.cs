using DG.Tweening;
using UnityEngine;

public class CoinPickUp : MonoBehaviour
{
    [SerializeField] private Vector3 rewardPopupOffset = new Vector3(0f, 0.65f, 0f);
    [SerializeField] private Vector3 rewardPopupFloatOffset = new Vector3(0f, 0.75f, 0f);
    [SerializeField] private float rewardPopupDuration = 0.75f;
    [SerializeField] private float rewardPopupScale = 0.01f;
    [SerializeField] private Color rewardPopupBackgroundColor = new Color(0.12f, 0.02f, 0.22f, 0.88f);
    [SerializeField] private Color rewardPopupTextColor = new Color(1f, 0.86f, 0.16f, 1f);

    private bool isDestroying;

    public void DestroyCoin()
    {
        if (isDestroying)
        {
            return;
        }

        isDestroying = true;
        DisableCollider();
        ShowRewardPopup();

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

    private void ShowRewardPopup()
    {
        Vector3 popupPosition = transform.position + rewardPopupOffset;
        CoinPickupPopupUI popup = CoinPickupPopupPool.GetOrCreateInstance().Get(popupPosition);

        popup.Play(
            popupPosition,
            "+" + GameManager.CoinPickupCurrencyReward + " COINS",
            rewardPopupFloatOffset,
            rewardPopupDuration,
            rewardPopupScale,
            rewardPopupBackgroundColor,
            rewardPopupTextColor);
    }
}
