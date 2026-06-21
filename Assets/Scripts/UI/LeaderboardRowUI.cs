using TMPro;
using DG.Tweening;
using UnityEngine;

public class LeaderboardRowUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject highlightObject;

    private RectTransform rectTransform;
    private Vector2 originalAnchoredPosition;
    private Vector3 originalScale;

    private void Awake()
    {
        rectTransform = transform as RectTransform;

        if (rectTransform != null)
        {
            originalAnchoredPosition = rectTransform.anchoredPosition;
        }

        originalScale = transform.localScale;
    }

    public void SetEntry(int rank, LeaderboardEntry entry, string currentPlayerName)
    {
        gameObject.SetActive(true);

        if (rankText != null)
        {
            rankText.text = rank.ToString();
        }

        if (playerNameText != null)
        {
            playerNameText.text = entry.playerName;
        }

        if (scoreText != null)
        {
            scoreText.text = entry.score.ToString();
        }

        if (highlightObject != null)
        {
            highlightObject.SetActive(entry.playerName == currentPlayerName);
        }

        PlayShowTween(rank);
    }

    public void Hide()
    {
        transform.DOKill();

        if (rectTransform != null)
        {
            rectTransform.DOKill();
            rectTransform.anchoredPosition = originalAnchoredPosition;
        }

        gameObject.SetActive(false);
    }

    private void PlayShowTween(int rank)
    {
        CanvasGroup canvasGroup = DOTweenUIAnimator.EnsureCanvasGroup(gameObject);

        transform.DOKill();
        canvasGroup.DOKill();
        canvasGroup.alpha = 0f;
        transform.localScale = originalScale * 0.96f;

        Sequence sequence = DOTween.Sequence()
            .SetLink(gameObject)
            .SetDelay(Mathf.Clamp(rank - 1, 0, 10) * 0.04f);
        sequence.Join(canvasGroup.DOFade(1f, 0.18f));
        sequence.Join(transform.DOScale(originalScale, 0.18f).SetEase(Ease.OutBack));

        if (rectTransform != null)
        {
            rectTransform.DOKill();
            rectTransform.anchoredPosition = originalAnchoredPosition + new Vector2(24f, 0f);
            sequence.Join(rectTransform.DOAnchorPos(originalAnchoredPosition, 0.18f).SetEase(Ease.OutQuad));
        }
    }
}
