using UnityEngine;

public static class UpgradeManager
{
    private static bool isLoaded;

    private static int fuelTankLevel;
    private static int enginePowerLevel;
    private static int rotationControlLevel;
    private static int landingStabilizerLevel;

    private static void EnsureLoaded()
    {
        if (isLoaded)
        {
            return;
        }

        SaveData saveData = SaveManager.LoadProgress();

        fuelTankLevel = saveData.fuelTankLevel;
        enginePowerLevel = saveData.enginePowerLevel;
        rotationControlLevel = saveData.rotationControlLevel;
        landingStabilizerLevel = saveData.landingStabilizerLevel;

        isLoaded = true;
    }

    public static int GetUpgradeLevel(UpgradeType upgradeType)
    {
        EnsureLoaded();

        switch (upgradeType)
        {
            case UpgradeType.FuelTank:
                return fuelTankLevel;

            case UpgradeType.EnginePower:
                return enginePowerLevel;

            case UpgradeType.RotationControl:
                return rotationControlLevel;

            case UpgradeType.LandingStabilizer:
                return landingStabilizerLevel;

            default:
                return 0;
        }
    }

    public static bool IsMaxLevel(ShopItemData itemData)
    {
        int currentLevel = GetUpgradeLevel(itemData.upgradeType);
        return currentLevel >= itemData.maxLevel;
    }

    public static int GetUpgradePrice(ShopItemData itemData)
    {
        int currentLevel = GetUpgradeLevel(itemData.upgradeType);

        if (currentLevel >= itemData.maxLevel)
        {
            return 0;
        }

        return itemData.GetPrice(currentLevel);
    }

    public static bool TryUpgrade(ShopItemData itemData)
    {
        EnsureLoaded();

        if (itemData == null)
        {
            Debug.LogError("ShopItemData is null.");
            return false;
        }

        int currentLevel = GetUpgradeLevel(itemData.upgradeType);

        if (currentLevel >= itemData.maxLevel)
        {
            Debug.Log($"{itemData.itemName} already max level.");
            return false;
        }

        int price = itemData.GetPrice(currentLevel);

        if (!PlayerCurrency.SpendCoins(price))
        {
            return false;
        }

        SetUpgradeLevel(itemData.upgradeType, currentLevel + 1);
        SaveCurrentUpgradeLevels();

        Debug.Log($"Upgrade Success: {itemData.itemName} Level {currentLevel + 1}");
        return true;
    }

    private static void SetUpgradeLevel(UpgradeType upgradeType, int newLevel)
    {
        switch (upgradeType)
        {
            case UpgradeType.FuelTank:
                fuelTankLevel = newLevel;
                break;

            case UpgradeType.EnginePower:
                enginePowerLevel = newLevel;
                break;

            case UpgradeType.RotationControl:
                rotationControlLevel = newLevel;
                break;

            case UpgradeType.LandingStabilizer:
                landingStabilizerLevel = newLevel;
                break;
        }
    }

    private static void SaveCurrentUpgradeLevels()
    {
        SaveManager.SaveUpgradeLevels(
            fuelTankLevel,
            enginePowerLevel,
            rotationControlLevel,
            landingStabilizerLevel
        );
    }

    public static void Reload()
    {
        isLoaded = false;
        EnsureLoaded();
    }

    // GAMEPLAY STATS:

    public static float GetFuelAmountMax()
    {
        EnsureLoaded();

        // STORE STAT:
        // Lv0 = 10, Lv1 = 12, Lv2 = 14, Lv3 = 16
        return GetFuelAmountMaxByLevel(fuelTankLevel);
    }

    public static float GetEngineForce()
    {
        EnsureLoaded();

        // STORE STAT:
        // Lv0 = 700, Lv1 = 750, Lv2 = 800, Lv3 = 850
        return GetEngineForceByLevel(enginePowerLevel);
    }

    public static float GetTurnSpeed()
    {
        EnsureLoaded();

        // STORE STAT:
        // Lv0 = 100, Lv1 = 115, Lv2 = 130, Lv3 = 145
        return GetTurnSpeedByLevel(rotationControlLevel);
    }

    public static float GetSoftLandingVelocityMagnitude()
    {
        EnsureLoaded();

        // STORE STAT:
        // Lv0 = 4.0, Lv1 = 4.3, Lv2 = 4.6, Lv3 = 4.9
        return GetSoftLandingVelocityMagnitudeByLevel(landingStabilizerLevel);
    }

    public static float GetMinLandingDotVector()
    {
        EnsureLoaded();

        // STORE STAT:
        // Lv0 = 0.90, Lv1 = 0.88, Lv2 = 0.86, Lv3 = 0.84
        return GetMinLandingDotVectorByLevel(landingStabilizerLevel);
    }

    // STORE ADDED:
    // Các hàm dưới dùng cho StoreUI để lấy chỉ số hiện tại và chỉ số cấp tiếp theo.

    public static float GetFuelAmountMaxByLevel(int level)
    {
        return 10f + level * 2f;
    }

    public static float GetEngineForceByLevel(int level)
    {
        return 700f + level * 50f;
    }

    public static float GetTurnSpeedByLevel(int level)
    {
        return 100f + level * 15f;
    }

    public static float GetSoftLandingVelocityMagnitudeByLevel(int level)
    {
        return 4f + level * 0.3f;
    }

    public static float GetMinLandingDotVectorByLevel(int level)
    {
        return 0.90f - level * 0.02f;
    }

    // STORE ADDED:
    // Lấy tên chỉ số để hiện trên Store UI.
    public static string GetStatName(UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case UpgradeType.FuelTank:
                return "FUEL";

            case UpgradeType.EnginePower:
                return "POWER";

            case UpgradeType.RotationControl:
                return "TURN";

            case UpgradeType.LandingStabilizer:
                return "SAFE SPD";

            default:
                return "STAT";
        }
    }

    // STORE ADDED:
    // Lấy chỉ số chính hiện tại theo level.
    public static string GetCurrentStatText(UpgradeType upgradeType)
    {
        int currentLevel = GetUpgradeLevel(upgradeType);
        return GetStatTextByLevel(upgradeType, currentLevel);
    }

    // STORE ADDED:
    // Lấy chỉ số cấp tiếp theo.
    public static string GetNextStatText(ShopItemData itemData)
    {
        int currentLevel = GetUpgradeLevel(itemData.upgradeType);

        if (currentLevel >= itemData.maxLevel)
        {
            return "MAX";
        }

        int nextLevel = currentLevel + 1;
        return GetStatTextByLevel(itemData.upgradeType, nextLevel);
    }

    private static string GetStatTextByLevel(UpgradeType upgradeType, int level)
    {
        switch (upgradeType)
        {
            case UpgradeType.FuelTank:
                return GetFuelAmountMaxByLevel(level).ToString("0");

            case UpgradeType.EnginePower:
                return GetEngineForceByLevel(level).ToString("0");

            case UpgradeType.RotationControl:
                return GetTurnSpeedByLevel(level).ToString("0");

            case UpgradeType.LandingStabilizer:
                return GetSoftLandingVelocityMagnitudeByLevel(level).ToString("0.0");

            default:
                return "0";
        }
    }

    // STORE ADDED:
    // Landing Stabilizer có thêm chỉ số phụ là angle.
    public static string GetLandingAngleCurrentText()
    {
        int currentLevel = GetUpgradeLevel(UpgradeType.LandingStabilizer);
        return GetMinLandingDotVectorByLevel(currentLevel).ToString("0.00");
    }

    public static string GetLandingAngleNextText(ShopItemData itemData)
    {
        int currentLevel = GetUpgradeLevel(UpgradeType.LandingStabilizer);

        if (currentLevel >= itemData.maxLevel)
        {
            return "MAX";
        }

        int nextLevel = currentLevel + 1;
        return GetMinLandingDotVectorByLevel(nextLevel).ToString("0.00");
    }
}