using System;
using DG.Tweening;
using SpaceP.Scoring;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LandedUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleTextMesh;
    [SerializeField] private TextMeshProUGUI statsTextMesh;
    [SerializeField] private TextMeshProUGUI nextButtonTextMexh;
    [SerializeField] private Button nextButton;
    [SerializeField] private Sprite rewardCoinSprite;
    [Header("Result Animation")]
    [SerializeField] private GameObject bannerObject;
    [SerializeField] private GameObject statsPanelObject;
    [SerializeField] private GameObject statsLabelsObject;
    [Header("Fail Upgrade Suggestion")]
    [SerializeField] private GameObject upgradeSuggestionPanel;
    [SerializeField] private Button storeButton;
    [SerializeField] private Button retryButton;

    private Action nextButtonClickAction;
    private GameObject rewardPanel;
    private TextMeshProUGUI rewardValueText;
    private Sequence resultSequence;

    private const float BannerDuration = 0.28f;
    private const float StatsDuration = 0.26f;
    private const float FinalPanelDuration = 0.28f;

    private void Awake()
    {
        CreateRewardPanel();

        nextButton.onClick.AddListener(() =>
        {
            nextButtonClickAction?.Invoke();
        });

        if (storeButton != null)
        {
            storeButton.onClick.AddListener(() =>
            {
                SceneLoader.LoadScene(SceneLoader.Scene.StoreScreen);
            });
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(() =>
            {
                GameManager.Instance.RetryLevel();
            });
        }

        if (upgradeSuggestionPanel != null)
        {
            upgradeSuggestionPanel.SetActive(false);
        }
    }

    private void Start()
    {
        PlayerMovement.Instance.onLanded += lander_onLanded;

        nextButton.Select();
        DOTweenUIAnimator.HidePanelImmediate(gameObject);
    }

    private void lander_onLanded(object sender, PlayerMovement.OnLandedEventArgs e)
    {
        bool successLanding = e.Result.IsSuccess;

        if (successLanding)
        {
            titleTextMesh.text = "SUCCESSFUL LANDING!";
            nextButtonTextMexh.text = "NEXT LEVEL";
            nextButtonClickAction = GameManager.Instance.NextLevel;
            nextButton.gameObject.SetActive(true);
            PrepareRewardPanel(GameManager.Instance.GetCurrentLevelCompleteCoinReward());
            if (upgradeSuggestionPanel != null)
            {
                upgradeSuggestionPanel.SetActive(false);
            }
        }
        else
        {
            titleTextMesh.text = GetCrashTitle(e.Result.Type);
            nextButtonTextMexh.text = "RESTART";
            nextButtonClickAction = GameManager.Instance.RetryLevel;
            nextButton.gameObject.SetActive(upgradeSuggestionPanel == null);
            rewardPanel.SetActive(false);
            if (upgradeSuggestionPanel != null)
            {
                upgradeSuggestionPanel.SetActive(true);
            }
        }

        statsTextMesh.text =
            e.Result.ImpactSpeed.ToString("0.0") + "\n" +
            Mathf.Round(e.Result.Uprightness * 100f) + "\n" +
            "x" + e.Result.ScoreMultiplier + "\n" +
            e.Result.Score;

        PlayResultSequence(successLanding);
    }

    private void CreateRewardPanel()
    {
        rewardPanel = new GameObject(
            "LevelRewardPanel",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Outline),
            typeof(CanvasGroup)
        );
        rewardPanel.layer = gameObject.layer;
        rewardPanel.transform.SetParent(transform, false);

        RectTransform panelRect = rewardPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0f, -190f);
        panelRect.sizeDelta = new Vector2(450f, 105f);

        Image panelImage = rewardPanel.GetComponent<Image>();
        panelImage.color = new Color(0.025f, 0.07f, 0.13f, 0.96f);
        panelImage.raycastTarget = false;

        Outline outline = rewardPanel.GetComponent<Outline>();
        outline.effectColor = new Color(0.05f, 0.85f, 1f, 0.9f);
        outline.effectDistance = new Vector2(3f, -3f);

        CreateCoinIcon(panelRect);
        CreateRewardLabel(panelRect);
        rewardValueText = CreateRewardValue(panelRect);

        rewardPanel.SetActive(false);
    }

    private void CreateCoinIcon(RectTransform parent)
    {
        GameObject iconObject = new GameObject(
            "CoinIcon",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        iconObject.layer = gameObject.layer;
        iconObject.transform.SetParent(parent, false);

        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(-175f, 0f);
        iconRect.sizeDelta = new Vector2(72f, 72f);

        Image iconImage = iconObject.GetComponent<Image>();
        iconImage.sprite = rewardCoinSprite;
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;
        iconImage.color = rewardCoinSprite != null
            ? Color.white
            : new Color(1f, 0.78f, 0.05f, 1f);
    }

    private void CreateRewardLabel(RectTransform parent)
    {
        TextMeshProUGUI label = CreateRewardText(
            "RewardLabel",
            parent,
            new Vector2(35f, 25f),
            new Vector2(330f, 38f),
            30f
        );
        label.text = "LEVEL REWARD";
        label.color = new Color(0.1f, 0.85f, 1f, 1f);
    }

    private TextMeshProUGUI CreateRewardValue(RectTransform parent)
    {
        TextMeshProUGUI valueText = CreateRewardText(
            "RewardValue",
            parent,
            new Vector2(35f, -20f),
            new Vector2(330f, 48f),
            38f
        );
        valueText.fontStyle = FontStyles.Bold;
        valueText.color = new Color(1f, 0.78f, 0.05f, 1f);
        return valueText;
    }

    private TextMeshProUGUI CreateRewardText(
        string objectName,
        RectTransform parent,
        Vector2 position,
        Vector2 size,
        float fontSize
    )
    {
        GameObject textObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        textObject.layer = gameObject.layer;
        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = position;
        textRect.sizeDelta = size;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = titleTextMesh.font;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = false;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private void PrepareRewardPanel(int rewardCoins)
    {
        if (rewardPanel == null || rewardValueText == null)
        {
            return;
        }

        rewardValueText.text = "+" + Mathf.Max(0, rewardCoins) + " COINS";
        rewardPanel.SetActive(true);
    }

    private void PlayResultSequence(bool successLanding)
    {
        resultSequence?.Kill();
        gameObject.SetActive(true);

        CanvasGroup rootCanvasGroup = DOTweenUIAnimator.EnsureCanvasGroup(gameObject);
        rootCanvasGroup.alpha = 1f;
        rootCanvasGroup.interactable = false;
        rootCanvasGroup.blocksRaycasts = false;

        GameObject banner = bannerObject != null ? bannerObject : titleTextMesh.gameObject;
        GameObject statsPanel = statsPanelObject != null ? statsPanelObject : statsTextMesh.gameObject;
        PreparePopVisual(banner, 0.82f);
        PrepareFadeVisual(titleTextMesh.gameObject);
        PrepareSlideVisual(statsPanel, 28f);
        PrepareSlideVisual(statsLabelsObject, 28f);
        PrepareSlideVisual(statsTextMesh.gameObject, 28f);

        GameObject finalPanel = successLanding ? rewardPanel : upgradeSuggestionPanel;
        PreparePopVisual(finalPanel, 0.88f);
        PreparePopVisual(nextButton.gameObject, 0.9f);

        resultSequence = DOTween.Sequence()
            .SetLink(gameObject)
            .SetUpdate(true);

        AppendPop(resultSequence, banner, BannerDuration);
        JoinFade(resultSequence, titleTextMesh.gameObject, BannerDuration * 0.8f);
        resultSequence.AppendCallback(() =>
            DOTweenUIAnimator.PunchScale(titleTextMesh.transform, successLanding ? 0.14f : 0.22f, true));
        resultSequence.AppendInterval(0.05f);
        AppendSlide(resultSequence, statsPanel, StatsDuration);
        JoinSlide(resultSequence, statsLabelsObject, StatsDuration);
        JoinSlide(resultSequence, statsTextMesh.gameObject, StatsDuration);
        resultSequence.AppendInterval(0.08f);
        AppendPop(resultSequence, finalPanel, FinalPanelDuration);

        if (nextButton.gameObject != finalPanel)
        {
            JoinPop(resultSequence, nextButton.gameObject, FinalPanelDuration);
        }

        RectTransform resultRectTransform = null;
        if (!successLanding)
        {
            TryGetComponent(out resultRectTransform);
        }

        resultSequence.OnComplete(() =>
        {
            rootCanvasGroup.interactable = true;
            rootCanvasGroup.blocksRaycasts = true;
            SelectDefaultButton();

            if (resultRectTransform != null)
            {
                resultRectTransform
                    .DOShakeAnchorPos(0.28f, 12f, 14, 90f, false, true)
                    .SetUpdate(true);
            }
        });
    }

    private static void PreparePopVisual(GameObject target, float startScale)
    {
        if (target == null || !target.activeInHierarchy)
        {
            return;
        }

        CanvasGroup canvasGroup = DOTweenUIAnimator.EnsureCanvasGroup(target);
        target.transform.DOKill();
        canvasGroup.DOKill();
        canvasGroup.alpha = 0f;
        target.transform.localScale = DOTweenUIAnimator.GetOriginalScale(target.transform) * startScale;
    }

    private static void PrepareFadeVisual(GameObject target)
    {
        if (target != null && target.activeInHierarchy)
        {
            DOTweenUIAnimator.EnsureCanvasGroup(target).alpha = 0f;
        }
    }

    private static void PrepareSlideVisual(GameObject target, float offsetY)
    {
        if (target == null || !target.activeInHierarchy)
        {
            return;
        }

        RectTransform rectTransform = target.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = DOTweenUIAnimator.EnsureCanvasGroup(target);
        rectTransform.DOKill();
        canvasGroup.DOKill();
        canvasGroup.alpha = 0f;
        Vector2 originalPosition = GetOriginalAnchoredPosition(rectTransform);
        rectTransform.anchoredPosition = originalPosition + Vector2.down * offsetY;
    }

    private static readonly System.Collections.Generic.Dictionary<int, Vector2> originalAnchoredPositions =
        new System.Collections.Generic.Dictionary<int, Vector2>();

    private static Vector2 GetOriginalAnchoredPosition(RectTransform rectTransform)
    {
        int instanceId = rectTransform.GetInstanceID();
        if (!originalAnchoredPositions.TryGetValue(instanceId, out Vector2 position))
        {
            position = rectTransform.anchoredPosition;
            originalAnchoredPositions[instanceId] = position;
        }
        return position;
    }

    private static void AppendPop(Sequence sequence, GameObject target, float duration)
    {
        if (target == null || !target.activeInHierarchy) return;
        sequence.Append(DOTweenUIAnimator.EnsureCanvasGroup(target).DOFade(1f, duration * 0.7f));
        sequence.Join(target.transform.DOScale(DOTweenUIAnimator.GetOriginalScale(target.transform), duration).SetEase(Ease.OutBack));
    }

    private static void JoinPop(Sequence sequence, GameObject target, float duration)
    {
        if (target == null || !target.activeInHierarchy) return;
        sequence.Join(DOTweenUIAnimator.EnsureCanvasGroup(target).DOFade(1f, duration * 0.7f));
        sequence.Join(target.transform.DOScale(DOTweenUIAnimator.GetOriginalScale(target.transform), duration).SetEase(Ease.OutBack));
    }

    private static void JoinFade(Sequence sequence, GameObject target, float duration)
    {
        if (target != null && target.activeInHierarchy)
        {
            sequence.Join(DOTweenUIAnimator.EnsureCanvasGroup(target).DOFade(1f, duration));
        }
    }

    private static void AppendSlide(Sequence sequence, GameObject target, float duration)
    {
        if (target == null || !target.activeInHierarchy) return;
        RectTransform rectTransform = target.GetComponent<RectTransform>();
        sequence.Append(DOTweenUIAnimator.EnsureCanvasGroup(target).DOFade(1f, duration));
        sequence.Join(rectTransform.DOAnchorPos(GetOriginalAnchoredPosition(rectTransform), duration).SetEase(Ease.OutCubic));
    }

    private static void JoinSlide(Sequence sequence, GameObject target, float duration)
    {
        if (target == null || !target.activeInHierarchy) return;
        RectTransform rectTransform = target.GetComponent<RectTransform>();
        sequence.Join(DOTweenUIAnimator.EnsureCanvasGroup(target).DOFade(1f, duration));
        sequence.Join(rectTransform.DOAnchorPos(GetOriginalAnchoredPosition(rectTransform), duration).SetEase(Ease.OutCubic));
    }

    private string GetCrashTitle(LandingType landingType)
    {
        switch (landingType)
        {
            case LandingType.WrongLandingArea:
                return "<color=#ff0000>TERRAIN HIT</color>";

            case LandingType.TooFast:
                return "<color=#ff0000>TOO FAST</color>";

            case LandingType.TooSteep:
                return "<color=#ff0000>BAD ANGLE</color>";

            default:
                return "<color=#ff0000>CRASH!</color>";
        }
    }

    private void Hide()
    {
        DOTweenUIAnimator.HidePanel(gameObject);
    }

    private void SelectDefaultButton()
    {
        if (upgradeSuggestionPanel != null && upgradeSuggestionPanel.activeSelf)
        {
            if (storeButton != null)
            {
                storeButton.Select();
            }
            return;
        }

        if (nextButton != null && nextButton.gameObject.activeInHierarchy)
        {
            nextButton.Select();
        }
    }

    private void OnDestroy()
    {
        resultSequence?.Kill();
        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.onLanded -= lander_onLanded;
        }
    }
}
