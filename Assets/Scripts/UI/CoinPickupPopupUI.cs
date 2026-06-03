using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoinPickupPopupUI : MonoBehaviour
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Image backgroundImage;
    private Outline outline;
    private TextMeshProUGUI rewardText;
    private CoinPickupPopupPool pool;
    private Sequence sequence;

    private void Awake()
    {
        EnsureComponents();
    }

    public void SetPool(CoinPickupPopupPool pool)
    {
        this.pool = pool;
    }

    public void Play(
        Vector3 worldPosition,
        string text,
        Vector3 floatOffset,
        float duration,
        float scale,
        Color backgroundColor,
        Color textColor)
    {
        EnsureComponents();

        sequence?.Kill();
        transform.position = worldPosition;
        transform.localScale = Vector3.one * scale * 0.82f;
        canvasGroup.alpha = 0f;
        backgroundImage.color = backgroundColor;
        rewardText.color = textColor;
        rewardText.text = text;

        gameObject.SetActive(true);

        sequence = DOTween.Sequence().SetLink(gameObject);
        sequence.Append(canvasGroup.DOFade(1f, 0.12f));
        sequence.Join(transform.DOScale(Vector3.one * scale, 0.18f).SetEase(Ease.OutBack));
        sequence.Join(transform.DOMove(worldPosition + floatOffset, duration).SetEase(Ease.OutQuad));
        sequence.AppendInterval(0.16f);
        sequence.Append(canvasGroup.DOFade(0f, 0.18f));
        sequence.OnComplete(Release);
    }

    public void ResetPopup()
    {
        sequence?.Kill();
        canvasGroup.alpha = 0f;
        transform.localScale = Vector3.one;
    }

    private void Release()
    {
        if (pool != null)
        {
            pool.Release(this);
            return;
        }

        gameObject.SetActive(false);
    }

    private void EnsureComponents()
    {
        if (rectTransform != null)
        {
            return;
        }

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        backgroundImage = GetComponent<Image>();
        outline = GetComponent<Outline>();

        rectTransform.sizeDelta = new Vector2(190f, 54f);
        canvasGroup.alpha = 0f;
        backgroundImage.raycastTarget = false;
        outline.effectColor = new Color(1f, 0.75f, 0.08f, 0.75f);
        outline.effectDistance = new Vector2(3f, -3f);

        rewardText = GetComponentInChildren<TextMeshProUGUI>();

        if (rewardText == null)
        {
            GameObject textObject = new GameObject("CoinRewardText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.layer = gameObject.layer;
            textObject.transform.SetParent(transform, false);

            RectTransform textRectTransform = textObject.GetComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.offsetMin = new Vector2(12f, 0f);
            textRectTransform.offsetMax = new Vector2(-12f, 0f);

            rewardText = textObject.GetComponent<TextMeshProUGUI>();
        }

        rewardText.raycastTarget = false;
        rewardText.alignment = TextAlignmentOptions.Center;
        rewardText.fontSize = 28f;
        rewardText.fontStyle = FontStyles.Bold;
    }
}
