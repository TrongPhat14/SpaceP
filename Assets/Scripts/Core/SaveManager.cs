using UnityEngine;

public static class SaveManager
{
    private const int DefaultLevelNumber = 1;
    private const int DefaultTotalScore = 0;

    private const int DefaultCoins = 0;
    private const int DefaultUpgradeLevel = 0;

    private const int FalseValue = 0;
    private const int TrueValue = 1;

    public static void SaveProgress(int levelNumber, int totalScore, bool isGameCompleted)
    {
        PlayerPrefs.SetInt(SaveKeys.LevelNumber, levelNumber);
        PlayerPrefs.SetInt(SaveKeys.TotalScore, totalScore);
        PlayerPrefs.SetInt(SaveKeys.IsGameCompleted, isGameCompleted ? TrueValue : FalseValue);

        PlayerPrefs.Save();

        Debug.Log($"Save Progress: Level {levelNumber}, Total Score {totalScore}, Completed {isGameCompleted}");
    }

    // Keep your coins separate, use them when you earn more coins or buy upgrades.
    public static void SaveCoins(int coins)
    {
        PlayerPrefs.SetInt(SaveKeys.Coins, coins);
        PlayerPrefs.Save();

        Debug.Log($"Save Coins: {coins}");
    }

    // Save the level of all 4 upgrades.
    public static void SaveUpgradeLevels(
        int fuelTankLevel,
        int enginePowerLevel,
        int rotationControlLevel,
        int landingStabilizerLevel
    )
    {
        PlayerPrefs.SetInt(SaveKeys.FuelTankLevel, fuelTankLevel);
        PlayerPrefs.SetInt(SaveKeys.EnginePowerLevel, enginePowerLevel);
        PlayerPrefs.SetInt(SaveKeys.RotationControlLevel, rotationControlLevel);
        PlayerPrefs.SetInt(SaveKeys.LandingStabilizerLevel, landingStabilizerLevel);

        PlayerPrefs.Save();

        Debug.Log(
            $"Save Upgrades: Fuel {fuelTankLevel}, Engine {enginePowerLevel}, Rotation {rotationControlLevel}, Landing {landingStabilizerLevel}"
        );
    }

    public static SaveData LoadProgress()
    {
        int levelNumber = PlayerPrefs.GetInt(SaveKeys.LevelNumber, DefaultLevelNumber);
        int totalScore = PlayerPrefs.GetInt(SaveKeys.TotalScore, DefaultTotalScore);
        bool isGameCompleted = PlayerPrefs.GetInt(SaveKeys.IsGameCompleted, FalseValue) == TrueValue;
        int coins = PlayerPrefs.GetInt(SaveKeys.Coins, DefaultCoins);
        int fuelTankLevel = PlayerPrefs.GetInt(SaveKeys.FuelTankLevel, DefaultUpgradeLevel);
        int enginePowerLevel = PlayerPrefs.GetInt(SaveKeys.EnginePowerLevel, DefaultUpgradeLevel);
        int rotationControlLevel = PlayerPrefs.GetInt(SaveKeys.RotationControlLevel, DefaultUpgradeLevel);
        int landingStabilizerLevel = PlayerPrefs.GetInt(SaveKeys.LandingStabilizerLevel, DefaultUpgradeLevel);

        return new SaveData(
            levelNumber,
            totalScore,
            isGameCompleted,
            coins,
            fuelTankLevel,
            enginePowerLevel,
            rotationControlLevel,
            landingStabilizerLevel
        );
    }

    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(SaveKeys.LevelNumber)
            && PlayerPrefs.HasKey(SaveKeys.TotalScore);
    }

    public static void ResetProgress()
    {
        SaveProgress(DefaultLevelNumber, DefaultTotalScore, false);
    }

    public static void ResetAllProgressAndShop()
    {
        SaveProgress(DefaultLevelNumber, DefaultTotalScore, false);
        SaveCoins(DefaultCoins);

        SaveUpgradeLevels(
            DefaultUpgradeLevel,
            DefaultUpgradeLevel,
            DefaultUpgradeLevel,
            DefaultUpgradeLevel
        );

        PlayerCurrency.Reload();
        UpgradeManager.Reload();
    }

    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SaveKeys.LevelNumber);
        PlayerPrefs.DeleteKey(SaveKeys.TotalScore);
        PlayerPrefs.DeleteKey(SaveKeys.IsGameCompleted);
        PlayerPrefs.DeleteKey(SaveKeys.Coins);
        PlayerPrefs.DeleteKey(SaveKeys.FuelTankLevel);
        PlayerPrefs.DeleteKey(SaveKeys.EnginePowerLevel);
        PlayerPrefs.DeleteKey(SaveKeys.RotationControlLevel);
        PlayerPrefs.DeleteKey(SaveKeys.LandingStabilizerLevel);

        PlayerPrefs.Save();

        Debug.Log("Save data deleted.");
    }
}
