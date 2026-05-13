using UnityEngine;

public static class SaveManager
{
    private const int DefaultLevelNumber = 1;
    private const int DefaultTotalScore = 0;

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

    public static SaveData LoadProgress()
    {
        int levelNumber = PlayerPrefs.GetInt(SaveKeys.LevelNumber, DefaultLevelNumber);
        int totalScore = PlayerPrefs.GetInt(SaveKeys.TotalScore, DefaultTotalScore);

        // COMPLETED ADDED:
        // Nếu save cũ chưa có key này thì mặc định là false.
        bool isGameCompleted = PlayerPrefs.GetInt(SaveKeys.IsGameCompleted, FalseValue) == TrueValue;

        return new SaveData(levelNumber, totalScore, isGameCompleted);
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

    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SaveKeys.LevelNumber);
        PlayerPrefs.DeleteKey(SaveKeys.TotalScore);
        PlayerPrefs.DeleteKey(SaveKeys.IsGameCompleted);

        PlayerPrefs.Save();

        Debug.Log("Save data deleted.");
    }
}