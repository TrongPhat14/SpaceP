using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Image fuelImage;
    [Header("Level Intro")]
    [SerializeField] private TextMeshProUGUI levelIntroText;
    [SerializeField] private float introVisibleDuration = 1.5f;
    [SerializeField] private float introFadeDuration = 0.35f;
    [SerializeField] private float statsFadeDuration = 0.25f;
    [SerializeField, Range(0f, 1f)] private float landedAlpha = 0.45f;
    [SerializeField] private float landedFadeDuration = 0.25f;

    private float displayedFuelAmount = -1f;
    private CanvasGroup canvasGroup;
    private CanvasGroup introCanvasGroup;
    private Sequence introSequence;
    private bool statsVisible;
    private bool tutorialPreviewVisible;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (levelIntroText != null)
        {
            introCanvasGroup = DOTweenUIAnimator.EnsureCanvasGroup(levelIntroText.gameObject);
        }
    }

    private void OnEnable()
    {
        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
            canvasGroup.alpha = 0f;
        }
    }

    private void Start()
    {
        HideStatsImmediate();
        PlayLevelIntro();

        if (PlayerMovement.Instance == null)
        {
            return;
        }

        PlayerMovement.Instance.onFuelPickUp += Player_OnFuelPickUp;
        PlayerMovement.Instance.onCoinPickUp += Player_OnCoinPickUp;
        PlayerMovement.Instance.onLanded += Player_OnLanded;
        PlayerMovement.Instance.onStateChange += Player_OnStateChange;
    }

    private void PlayLevelIntro()
    {
        if (levelIntroText == null || introCanvasGroup == null || GameManager.Instance == null)
        {
            return;
        }

        introSequence?.Kill();
        levelIntroText.text = $"LEVEL {GameManager.Instance.GetLevelNumber()}";
        levelIntroText.gameObject.SetActive(true);
        levelIntroText.transform.DOKill();
        introCanvasGroup.DOKill();

        Vector3 originalScale = DOTweenUIAnimator.GetOriginalScale(levelIntroText.transform);
        levelIntroText.transform.localScale = originalScale * 0.88f;
        introCanvasGroup.alpha = 0f;
        introCanvasGroup.interactable = false;
        introCanvasGroup.blocksRaycasts = false;

        introSequence = DOTween.Sequence()
            .SetLink(levelIntroText.gameObject)
            .SetUpdate(true)
            .Append(introCanvasGroup.DOFade(1f, introFadeDuration))
            .Join(levelIntroText.transform
                .DOScale(originalScale, introFadeDuration + 0.1f)
                .SetEase(Ease.OutBack))
            .AppendInterval(introVisibleDuration)
            .Append(introCanvasGroup.DOFade(0f, introFadeDuration))
            .Join(levelIntroText.transform
                .DOScale(originalScale * 1.05f, introFadeDuration)
                .SetEase(Ease.InQuad))
            .OnComplete(() =>
            {
                if (levelIntroText != null)
                {
                    levelIntroText.transform.localScale = originalScale;
                    levelIntroText.gameObject.SetActive(false);
                }
            });
    }

    private void Player_OnStateChange(object sender, PlayerMovement.OnStateChangeEventArgs e)
    {
        if (e.State == PlayerMovement.State.Normal)
        {
            ShowStats();
        }
    }

    private void HideStatsImmediate()
    {
        statsVisible = false;
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.DOKill();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void ShowStats()
    {
        if (statsVisible || canvasGroup == null)
        {
            return;
        }

        statsVisible = true;
        canvasGroup.DOKill();
        canvasGroup
            .DOFade(1f, statsFadeDuration)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    public void SetTutorialPreviewVisible(bool visible)
    {
        tutorialPreviewVisible = visible;

        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.DOKill();

        if (visible)
        {
            canvasGroup
                .DOFade(1f, statsFadeDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .SetLink(gameObject);
            return;
        }

        if (!statsVisible)
        {
            canvasGroup
                .DOFade(0f, statsFadeDuration)
                .SetEase(Ease.InQuad)
                .SetUpdate(true)
                .SetLink(gameObject);
        }
    }

    private void Update()
    {
        UpdateStatsTextMesh();
    }

    private void UpdateStatsTextMesh()
    {
        float fuelAmount = PlayerMovement.Instance.GetFuelAmountNormalized();

        if (displayedFuelAmount < 0f)
        {
            displayedFuelAmount = fuelAmount;
            fuelImage.fillAmount = fuelAmount;
        }
        else if (Mathf.Abs(displayedFuelAmount - fuelAmount) > 0.005f)
        {
            displayedFuelAmount = fuelAmount;
            fuelImage.DOKill();
            fuelImage
                .DOFillAmount(fuelAmount, 0.12f)
                .SetLink(fuelImage.gameObject)
                .SetEase(Ease.OutQuad);
        }

        statsText.text =
            GameManager.Instance.GetLevelNumber() + "\n" +
            GameManager.Instance.GetScore() + "\n" +
            Mathf.Round(GameManager.Instance.GetTime())
            ;
    }

    private void Player_OnFuelPickUp(object sender, EventArgs e)
    {
        DOTweenUIAnimator.PunchScale(fuelImage != null ? fuelImage.transform : null, 0.12f);
    }

    private void Player_OnCoinPickUp(object sender, EventArgs e)
    {
        DOTweenUIAnimator.PunchScale(statsText != null ? statsText.transform : null, 0.08f);
    }

    private void Player_OnLanded(object sender, PlayerMovement.OnLandedEventArgs e)
    {
        if (canvasGroup == null || tutorialPreviewVisible)
        {
            return;
        }

        canvasGroup.DOKill();
        canvasGroup
            .DOFade(landedAlpha, landedFadeDuration)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    private void OnDestroy()
    {
        introSequence?.Kill();

        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
        }

        if (PlayerMovement.Instance == null)
        {
            return;
        }

        PlayerMovement.Instance.onFuelPickUp -= Player_OnFuelPickUp;
        PlayerMovement.Instance.onCoinPickUp -= Player_OnCoinPickUp;
        PlayerMovement.Instance.onLanded -= Player_OnLanded;
        PlayerMovement.Instance.onStateChange -= Player_OnStateChange;
    }
}
