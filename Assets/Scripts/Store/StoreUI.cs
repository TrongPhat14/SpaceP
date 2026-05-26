using System;
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

    [Header("Navigation Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button startButton;

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
            startButton.onClick.AddListener(() =>
            {
                SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
            });
        }
    }

    private void Start()
    {
        RefreshUI();
    }

    private void OnClickUpgrade(StoreItemUI itemUI)
    {
        if (itemUI == null || itemUI.itemData == null)
        {
            Debug.LogError("Missing StoreItemUI or ShopItemData.");
            return;
        }

        UpgradeManager.TryUpgrade(itemUI.itemData);

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (coinsText != null)
        {
            coinsText.text = PlayerCurrency.GetCoins().ToString();
        }

        foreach (StoreItemUI itemUI in storeItemUIArray)
        {
            RefreshItemUI(itemUI);
        }
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
            targetObject.SetActive(active);
        }
    }
}
