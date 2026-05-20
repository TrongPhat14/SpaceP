using System;

[Serializable]
public class SaveData
{
    public int levelNumber;
    public int totalScore;
    public bool isGameCompleted;

    // Coin upgrade skill
    public int coins;

    // Skill
    public int fuelTankLevel;
    public int enginePowerLevel;
    public int rotationControlLevel;
    public int landingStabilizerLevel;

    public SaveData(
        int levelNumber,
        int totalScore,
        bool isGameCompleted,
        int coins,
        int fuelTankLevel,
        int enginePowerLevel,
        int rotationControlLevel,
        int landingStabilizerLevel
    )
    {
        this.levelNumber = levelNumber;
        this.totalScore = totalScore;
        this.isGameCompleted = isGameCompleted;
        this.coins = coins;
        this.fuelTankLevel = fuelTankLevel;
        this.enginePowerLevel = enginePowerLevel;
        this.rotationControlLevel = rotationControlLevel;
        this.landingStabilizerLevel = landingStabilizerLevel;
    }
}