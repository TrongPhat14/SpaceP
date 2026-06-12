using System;
using DG.Tweening;
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

    private Action nextButtonClickAction;
    private GameObject rewardPanel;
    private TextMeshProUGUI rewardValueText;

    private void Awake()
    {
        CreateRewardPanel();

        nextButton.onClick.AddListener(() =>
        {
            nextButtonClickAction?.Invoke();
        });
    }

    private void Start()
    {
        PlayerMovement.Instance.onLanded += lander_onLanded;

        nextButton.Select();
        DOTweenUIAnimator.HidePanelImmediate(gameObject);
    }

    private void lander_onLanded(object sender, PlayerMovement.OnLandedEventArgs e)
    {
        bool successLanding = e.landingType == PlayerMovement.LandingType.Success;

        if (successLanding)
        {
            titleTextMesh.text = "SUCCESSFUL LANDING!";
            nextButtonTextMexh.text = "NEXT LEVEL";
            nextButtonClickAction = GameManager.Instance.NextLevel;
            ShowRewardPanel(GameManager.Instance.GetCurrentLevelCompleteCoinReward());
        }
        else
        {
            titleTextMesh.text = GetCrashTitle(e.landingType);
            nextButtonTextMexh.text = "RESTART";
            nextButtonClickAction = GameManager.Instance.RetryLevel;
            rewardPanel.SetActive(false);
        }

        statsTextMesh.text =
            Mathf.Round(e.speed * 2f) + "\n" +
            Mathf.Round(e.dotVector * 100f) + "\n" +
            "x" + e.scoreMultiplier + "\n" +
            e.score;

        Show();

        DOTweenUIAnimator.PunchScale(titleTextMesh.transform, successLanding ? 0.14f : 0.22f);

        if (!successLanding && TryGetComponent(out RectTransform rectTransform))
        {
            rectTransform.DOShakeAnchorPos(0.28f, 12f, 14, 90f, false, true);
        }
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

    private void ShowRewardPanel(int rewardCoins)
    {
        if (rewardPanel == null || rewardValueText == null)
        {
            return;
        }

        rewardValueText.text = "+" + Mathf.Max(0, rewardCoins) + " COINS";
        rewardPanel.SetActive(true);

        CanvasGroup canvasGroup = rewardPanel.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        rewardPanel.transform.localScale = Vector3.one * 0.85f;
        rewardPanel.transform.DOKill();
        canvasGroup.DOKill();

        canvasGroup
            .DOFade(1f, 0.2f)
            .SetDelay(0.15f)
            .SetLink(rewardPanel);
        rewardPanel.transform
            .DOScale(Vector3.one, 0.32f)
            .SetDelay(0.15f)
            .SetEase(Ease.OutBack)
            .SetLink(rewardPanel);
    }

    private string GetCrashTitle(PlayerMovement.LandingType landingType)
    {
        switch (landingType)
        {
            case PlayerMovement.LandingType.WrongLandingArea:
                return "<color=#ff0000>TERRAIN HIT</color>";

            case PlayerMovement.LandingType.TooSpeedLanding:
                return "<color=#ff0000>TOO FAST</color>";

            case PlayerMovement.LandingType.TooSteepAngle:
                return "<color=#ff0000>BAD ANGLE</color>";

            default:
                return "<color=#ff0000>CRASH!</color>";
        }
    }

    private void Show()
    {
        DOTweenUIAnimator.ShowPanel(gameObject);
        nextButton.Select();
    }

    private void Hide()
    {
        DOTweenUIAnimator.HidePanel(gameObject);
    }

    private void OnDestroy()
    {
        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.onLanded -= lander_onLanded;
        }
    }
}
