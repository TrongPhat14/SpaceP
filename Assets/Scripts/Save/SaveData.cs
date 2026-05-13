using System;

[Serializable]
public class SaveData
{
    public int levelNumber;
    public int totalScore;
    public bool isGameCompleted;

    public SaveData(int levelNumber, int totalScore, bool isGameCompleted)
    {
        this.levelNumber = levelNumber;
        this.totalScore = totalScore;
        this.isGameCompleted = isGameCompleted;
    }
}