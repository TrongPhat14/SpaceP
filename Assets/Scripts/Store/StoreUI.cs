using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreUI : MonoBehaviour
{
    [Serializable]
    public class StoreItemUI
    {
        [Header("Data")]
        public ShopItemData itemData;

        [Header("Static Text - Optional")]
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descriptionText;

        [Header("Level")]
        public TextMeshProUGUI levelText;

        [Header("Main Stat - Current")]
        public TextMeshProUGUI currentMainLabelText;
        public TextMeshProUGUI currentMainValueText;

        [Header("Main Stat - Next")]
        public GameObject nextMainGroup;
        public TextMeshProUGUI nextMainLabelText;
        public TextMeshProUGUI nextMainValueText;

        [Header("Main Stat - Arrow")]
        public GameObject nextArrowObject;

        [Header("Second Stat - Only For Landing Stabilizer")]
        public GameObject secondStatGroup;

        public TextMeshProUGUI currentSecondLabelText;
        public TextMeshProUGUI currentSecondValueText;

        public GameObject nextSecondGroup;
        public TextMeshProUGUI nextSecondLabelText;
        public TextMeshProUGUI nextSecondValueText;

        [Header("Button")]
        public Button upgradeButton;
        public TextMeshProUGUI upgradeButtonText;
        public TextMeshProUGUI priceText;
    }

    [Header("Items")]
    [SerializeField] private StoreItemUI[] storeItemUIArray;

    [Header("Top UI")]
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI coinsChangeText;

    [Header("Navigation Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button startButton;

    private bool hasRenderedUI;
    private int displayedCoins;
    private Tween coinsCountTween;
    private Tween coinsChangeTween;
    private bool hasCoinsChangeOriginalPosition;
    private Vector2 coinsChangeOriginalPosition;

    private void Awake()
    {
        foreach (StoreItemUI itemUI in storeItemUIArray)
        {
            StoreItemUI localItemUI = itemUI;

            if (localItemUI.upgradeButton != null)
            {
                localItemUI.upgradeButton.onClick.AddListener(() =>
                {
                    OnClickUpgrade(localItemUI);
                });
            }
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(() =>
            {
                SceneLoader.LoadScene(SceneLoader.Scene.MainMenuScreen);
            });
        }

        if (startButton != null)
        {
            startButton.onClick.AddListener(OnClickStart);
        }
    }

    private void Start()
    {
        displayedCoins = PlayerCurrency.GetCoins();
        EnsureCoinsChangeText();
        RefreshUI();
    }

    private void OnClickUpgrade(StoreItemUI itemUI)
    {
        if (itemUI == null || itemUI.itemData == null)
        {
            Debug.LogError("Missing StoreItemUI or ShopItemData.");
            return;
        }

        int coinsBeforeUpgrade = PlayerCurrency.GetCoins();
        bool upgraded = UpgradeManager.TryUpgrade(itemUI.itemData);
        int coinsAfterUpgrade = PlayerCurrency.GetCoins();

        RefreshUI(!upgraded);
        PlayUpgradeFeedback(itemUI, upgraded);

        if (upgraded)
        {
            PlayCoinsSpendFeedback(coinsBeforeUpgrade, coinsAfterUpgrade);
        }
    }

    private void OnClickStart()
    {
        SaveData saveData = SaveManager.LoadProgress();

        if (saveData != null && saveData.isGameCompleted)
        {
            GameManager.ResetStaticData();
            SaveManager.ResetProgress();
            LeaderboardManager.ClearSubmittedCompletedScore();
        }

        SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
    }

    private void RefreshUI(bool updateCoinsText = true)
    {
        if (coinsText != null && updateCoinsText)
        {
            displayedCoins = PlayerCurrency.GetCoins();
            coinsText.text = displayedCoins.ToString();
        }

        foreach (StoreItemUI itemUI in storeItemUIArray)
        {
            RefreshItemUI(itemUI);
        }

        hasRenderedUI = true;
    }

    private void RefreshItemUI(StoreItemUI itemUI)
    {
        if (itemUI == null || itemUI.itemData == null)
        {
            return;
        }

        ShopItemData itemData = itemUI.itemData;

        int currentLevel = UpgradeManager.GetUpgradeLevel(itemData.upgradeType);
        bool isMaxLevel = UpgradeManager.IsMaxLevel(itemData);

        RefreshStaticText(itemUI, itemData);
        RefreshLevel(itemUI, currentLevel, itemData.maxLevel, isMaxLevel);
        RefreshMainStat(itemUI, itemData, isMaxLevel);
        RefreshSecondStat(itemUI, itemData, isMaxLevel);
        RefreshUpgradeButton(itemUI, itemData, currentLevel, isMaxLevel);
    }

    private void RefreshStaticText(StoreItemUI itemUI, ShopItemData itemData)
    {
        if (itemUI.nameText != null)
        {
            itemUI.nameText.text = itemData.itemName;
        }

        if (itemUI.descriptionText != null)
        {
            itemUI.descriptionText.text = itemData.description;
        }
    }

    private void RefreshLevel(StoreItemUI itemUI, int currentLevel, int maxLevel, bool isMaxLevel)
    {
        if (itemUI.levelText == null)
        {
            return;
        }

        itemUI.levelText.text = isMaxLevel
            ? "MAX"
            : "LV " + currentLevel + " / " + maxLevel;
    }

    private void RefreshMainStat(StoreItemUI itemUI, ShopItemData itemData, bool isMaxLevel)
    {
        UpgradeType upgradeType = itemData.upgradeType;

        if (itemUI.currentMainLabelText != null)
        {
            itemUI.currentMainLabelText.text = isMaxLevel
                ? GetMaxMainLabel(upgradeType)
                : GetCurrentMainLabel(upgradeType);
        }

        if (itemUI.currentMainValueText != null)
        {
            itemUI.currentMainValueText.text = UpgradeManager.GetCurrentStatText(upgradeType);
        }

        if (itemUI.nextMainLabelText != null)
        {
            itemUI.nextMainLabelText.text = GetNextMainLabel(upgradeType);
        }

        if (itemUI.nextMainValueText != null)
        {
            itemUI.nextMainValueText.text = UpgradeManager.GetNextStatText(itemData);
        }

        SetActive(itemUI.nextMainGroup, !isMaxLevel);
        SetActive(itemUI.nextArrowObject, !isMaxLevel);
    }

    private void RefreshSecondStat(StoreItemUI itemUI, ShopItemData itemData, bool isMaxLevel)
    {
        bool isLandingStabilizer = itemData.upgradeType == UpgradeType.LandingStabilizer;

        SetActive(itemUI.secondStatGroup, isLandingStabilizer);

        if (!isLandingStabilizer)
        {
            return;
        }

        if (itemUI.currentSecondLabelText != null)
        {
            itemUI.currentSecondLabelText.text = isMaxLevel
                ? "MAX ANGLE"
                : "CURRENT ANGLE";
        }

        if (itemUI.currentSecondValueText != null)
        {
            itemUI.currentSecondValueText.text = UpgradeManager.GetLandingAngleCurrentText();
        }

        if (itemUI.nextSecondLabelText != null)
        {
            itemUI.nextSecondLabelText.text = "NEXT ANGLE";
        }

        if (itemUI.nextSecondValueText != null)
        {
            itemUI.nextSecondValueText.text = UpgradeManager.GetLandingAngleNextText(itemData);
        }

        SetActive(itemUI.nextSecondGroup, !isMaxLevel);
    }

    private void RefreshUpgradeButton(
        StoreItemUI itemUI,
        ShopItemData itemData,
        int currentLevel,
        bool isMaxLevel
    )
    {
        if (itemUI.upgradeButtonText != null)
        {
            itemUI.upgradeButtonText.text = isMaxLevel ? "MAXED" : "UPGRADE";
        }

        if (itemUI.priceText != null)
        {
            itemUI.priceText.text = isMaxLevel
                ? "MAX"
                : itemData.GetPrice(currentLevel).ToString();
        }

        if (itemUI.upgradeButton != null)
        {
            itemUI.upgradeButton.interactable = !isMaxLevel;
        }
    }

    private string GetCurrentMainLabel(UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case UpgradeType.FuelTank:
                return "CURRENT";

            case UpgradeType.EnginePower:
                return "CURRENT";

            case UpgradeType.RotationControl:
                return "CURRENT";

            case UpgradeType.LandingStabilizer:
                return "CURRENT SPD";

            default:
                return "CURRENT";
        }
    }

    private string GetNextMainLabel(UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case UpgradeType.FuelTank:
                return "NEXT";

            case UpgradeType.EnginePower:
                return "NEXT";

            case UpgradeType.RotationControl:
                return "NEXT";

            case UpgradeType.LandingStabilizer:
                return "NEXT SPD";

            default:
                return "NEXT";
        }
    }

    private string GetMaxMainLabel(UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case UpgradeType.FuelTank:
                return "MAX FUEL";

            case UpgradeType.EnginePower:
                return "MAX POWER";

            case UpgradeType.RotationControl:
                return "MAX TURN";

            case UpgradeType.LandingStabilizer:
                return "MAX SPD";

            default:
                return "MAX";
        }
    }

    private void SetActive(GameObject targetObject, bool active)
    {
        if (targetObject != null)
        {
            if (!hasRenderedUI)
            {
                CanvasGroup initialCanvasGroup = DOTweenUIAnimator.EnsureCanvasGroup(targetObject);
                initialCanvasGroup.alpha = active ? 1f : 0f;
                targetObject.transform.localScale = DOTweenUIAnimator.GetOriginalScale(targetObject.transform);
                targetObject.SetActive(active);
                return;
            }

            if (!active && !targetObject.activeSelf)
            {
                CanvasGroup hiddenCanvasGroup = DOTweenUIAnimator.EnsureCanvasGroup(targetObject);
                hiddenCanvasGroup.alpha = 0f;
                return;
            }

            CanvasGroup canvasGroup = DOTweenUIAnimator.EnsureCanvasGroup(targetObject);
            targetObject.SetActive(true);
            targetObject.transform.DOKill();
            canvasGroup.DOKill();

            if (active)
            {
                Vector3 originalScale = DOTweenUIAnimator.GetOriginalScale(targetObject.transform);
                targetObject.transform.localScale = originalScale * 0.96f;
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, 0.14f).SetLink(targetObject);
                targetObject.transform.DOScale(originalScale, 0.14f).SetLink(targetObject).SetEase(Ease.OutBack);
                return;
            }

            Vector3 hideOriginalScale = DOTweenUIAnimator.GetOriginalScale(targetObject.transform);
            canvasGroup
                .DOFade(0f, 0.12f)
                .SetLink(targetObject)
                .OnComplete(() =>
                {
                    if (targetObject != null)
                    {
                        targetObject.SetActive(false);
                        targetObject.transform.localScale = hideOriginalScale;
                    }
                });
        }
    }

    private void PlayUpgradeFeedback(StoreItemUI itemUI, bool coinsChanged)
    {
        DOTweenUIAnimator.PunchScale(itemUI.upgradeButton != null ? itemUI.upgradeButton.transform : null, 0.12f);
        DOTweenUIAnimator.PunchScale(itemUI.levelText != null ? itemUI.levelText.transform : null, 0.1f);

        if (coinsChanged)
        {
            DOTweenUIAnimator.PunchScale(coinsText != null ? coinsText.transform : null, 0.1f);
        }
    }

    private void PlayCoinsSpendFeedback(int coinsBeforeUpgrade, int coinsAfterUpgrade)
    {
        if (coinsText == null)
        {
            return;
        }

        int spentCoins = Mathf.Max(0, coinsBeforeUpgrade - coinsAfterUpgrade);

        coinsCountTween?.Kill();
        displayedCoins = coinsBeforeUpgrade;
        coinsText.text = displayedCoins.ToString();

        coinsCountTween = DOTween
            .To(
                () => displayedCoins,
                value =>
                {
                    displayedCoins = value;
                    coinsText.text = displayedCoins.ToString();
                },
                coinsAfterUpgrade,
                0.55f
            )
            .SetLink(coinsText.gameObject)
            .SetEase(Ease.OutCubic);

        ShowCoinsChangeText(spentCoins);
    }

    private void ShowCoinsChangeText(int spentCoins)
    {
        EnsureCoinsChangeText();

        if (coinsChangeText == null || spentCoins <= 0)
        {
            return;
        }

        CanvasGroup canvasGroup = DOTweenUIAnimator.EnsureCanvasGroup(coinsChangeText.gameObject);
        RectTransform rectTransform = coinsChangeText.transform as RectTransform;

        if (rectTransform != null && !hasCoinsChangeOriginalPosition)
        {
            coinsChangeOriginalPosition = rectTransform.anchoredPosition;
            hasCoinsChangeOriginalPosition = true;
        }

        coinsChangeTween?.Kill();
        coinsChangeText.gameObject.SetActive(true);
        coinsChangeText.text = "-" + spentCoins;
        coinsChangeText.color = new Color(1f, 0.25f, 0.18f, 1f);
        canvasGroup.alpha = 1f;

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = coinsChangeOriginalPosition;
        }

        Sequence sequence = DOTween.Sequence().SetLink(coinsChangeText.gameObject);
        sequence.Join(canvasGroup.DOFade(0f, 0.7f).SetDelay(0.25f));

        if (rectTransform != null)
        {
            sequence.Join(rectTransform.DOAnchorPos(coinsChangeOriginalPosition + new Vector2(0f, -16f), 0.7f).SetEase(Ease.OutQuad));
        }

        sequence.OnComplete(() =>
        {
            if (coinsChangeText != null)
            {
                coinsChangeText.gameObject.SetActive(false);
            }

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = coinsChangeOriginalPosition;
            }
        });

        coinsChangeTween = sequence;
    }

    private void EnsureCoinsChangeText()
    {
        if (coinsChangeText != null || coinsText == null)
        {
            return;
        }

        coinsChangeText = Instantiate(coinsText, coinsText.transform.parent);
        coinsChangeText.name = "CoinsChangeText";
        coinsChangeText.text = string.Empty;
        coinsChangeText.fontSize = coinsText.fontSize * 0.8f;
        coinsChangeText.alignment = TextAlignmentOptions.Center;

        RectTransform coinsRectTransform = coinsText.transform as RectTransform;
        RectTransform changeRectTransform = coinsChangeText.transform as RectTransform;

        if (coinsRectTransform != null && changeRectTransform != null)
        {
            changeRectTransform.anchorMin = coinsRectTransform.anchorMin;
            changeRectTransform.anchorMax = coinsRectTransform.anchorMax;
            changeRectTransform.pivot = coinsRectTransform.pivot;
            changeRectTransform.sizeDelta = coinsRectTransform.sizeDelta;
            changeRectTransform.anchoredPosition = coinsRectTransform.anchoredPosition + new Vector2(0f, -22f);
            coinsChangeOriginalPosition = changeRectTransform.anchoredPosition;
            hasCoinsChangeOriginalPosition = true;
        }

        CanvasGroup canvasGroup = DOTweenUIAnimator.EnsureCanvasGroup(coinsChangeText.gameObject);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        coinsChangeText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        coinsCountTween?.Kill();
        coinsChangeTween?.Kill();
    }
}
