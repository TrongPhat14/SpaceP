using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private static int levelNumber = 1;
    private static int totalScore = 0;

    private static bool isGameCompleted = false;

    public static void ResetStaticData()
    {
        levelNumber = 1;
        totalScore = 0;
        isGameCompleted = false;
    }

    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnPaused;

    [SerializeField] private List<GameLevel> gameLevelList;
    [SerializeField] private CinemachineCamera cinemachineCamera;

    private int score;
    private int currentLevelCoins;
    private float time;
    private bool isRunning;

    private bool hasLevelCompleted;

    private GameLevel spawnedGameLevel;


    private void Awake()
    {
        Instance = this;

        LoadSavedProgress();
    }

    private void Start()
    {
        PlayerMovement.Instance.onCoinPickUp += lander_onCoinPickUp;
        PlayerMovement.Instance.onLanded += Lander_onLanded;
        PlayerMovement.Instance.onStateChange += Lander_onStateChange;

        GameInput.Instance.OnMenuButtonPressed += GameInput_OnMenuButtonPressed;

        loadCurrentLevel();
    }

    private void LoadSavedProgress()
    {
        SaveData saveData = SaveManager.LoadProgress();

        levelNumber = saveData.levelNumber;
        totalScore = saveData.totalScore;
        isGameCompleted = saveData.isGameCompleted;

        Debug.Log($"Loaded Save: Level {levelNumber}, Total Score {totalScore}, Completed {isGameCompleted}");
    }
    // Test level game
/*    public static void SetProgress(int newLevelNumber, int newTotalScore)
    {
        SetProgress(newLevelNumber, newTotalScore, false);
    }

    public static void SetProgress(int newLevelNumber, int newTotalScore, bool newIsGameCompleted)
    {
        levelNumber = newLevelNumber;
        totalScore = newTotalScore;
        isGameCompleted = newIsGameCompleted;

        SaveManager.SaveProgress(levelNumber, totalScore, isGameCompleted);
    }*/

    private void GameInput_OnMenuButtonPressed(object sender, EventArgs e)
    {
        PauseOrUnPauseGame();
    }

    private void Lander_onStateChange(object sender, PlayerMovement.OnStateChangeEventArgs e)
    {
        isRunning = e.State == PlayerMovement.State.Normal;

        if (e.State == PlayerMovement.State.Normal)
        {
            cinemachineCamera.Target.TrackingTarget = PlayerMovement.Instance.transform;
            CinemachineCameraZoom.Instance.SetNormalOrthographicSize();
        }
    }

    private void Update()
    {
        if (isRunning)
        {
            time += Time.deltaTime;
        }
    }

    private void loadCurrentLevel()
    {
        if (isGameCompleted)
        {
            Debug.Log("Game already completed. Load GameOverScreen.");
            SceneLoader.LoadScene(SceneLoader.Scene.GameOverScreen);
            return;
        }

        GameLevel gamelevel = GetGameLevel();

        if (gamelevel == null)
        {
            Debug.LogError($"Can not find GameLevel with levelNumber: {levelNumber}");
            SceneLoader.LoadScene(SceneLoader.Scene.GameOverScreen);
            return;
        }

        spawnedGameLevel = Instantiate(
            gamelevel,
            UnityEngine.Vector3.zero,
            UnityEngine.Quaternion.identity
        );

        PlayerMovement.Instance.transform.position = spawnedGameLevel.GetLanderStartPosition();

        cinemachineCamera.Target.TrackingTarget = spawnedGameLevel.GetCameraStartTargetTransform();

        CinemachineCameraZoom.Instance.SetTargetOrthographicSize(
            spawnedGameLevel.GetZoomOutOrthographicSize()
        );

        AnalyticsManager.LogLevelStart(levelNumber, totalScore);
    }

    private GameLevel GetGameLevel()
    {
        foreach (GameLevel level in gameLevelList)
        {
            if (level.GetLevelNumber() == levelNumber)
            {
                return level;
            }
        }

        return null;
    }

    private bool HasGameLevel(int checkLevelNumber)
    {
        foreach (GameLevel level in gameLevelList)
        {
            if (level.GetLevelNumber() == checkLevelNumber)
            {
                return true;
            }
        }

        return false;
    }

    private void Lander_onLanded(object sender, PlayerMovement.OnLandedEventArgs e)
    {
        if (e.landingType != PlayerMovement.LandingType.Success)
        {
            AnalyticsManager.LogLevelFail(
                levelNumber,
                GetLandingFailReason(e.landingType),
                time,
                e.speed,
                e.dotVector * 100f
            );
            return;
        }

        AddScore(e.score);

        AnalyticsManager.LogLevelComplete(levelNumber, score, totalScore + score, time);

        CompleteLevelAndSave();
    }

    public void CompleteLevelAndSave()
    {
        if (hasLevelCompleted)
        {
            return;
        }

        hasLevelCompleted = true;

        totalScore += score;

        int rewardCoins = 0;

        if (spawnedGameLevel != null)
        {
            rewardCoins = spawnedGameLevel.GetCompleteCoinReward();
        }

        int totalCoinsEarnedThisLevel = currentLevelCoins + rewardCoins;

        PlayerCurrency.AddCoins(totalCoinsEarnedThisLevel);

        Debug.Log($"Pickup Coins This Level: {currentLevelCoins}");
        Debug.Log($"Complete Level Coin Reward: {rewardCoins}");
        Debug.Log($"Total Coins Earned This Level: {totalCoinsEarnedThisLevel}");

        int nextLevelNumber = levelNumber + 1;

        if (HasGameLevel(nextLevelNumber))
        {
            levelNumber = nextLevelNumber;
            isGameCompleted = false;

            SaveManager.SaveProgress(levelNumber, totalScore, isGameCompleted);

            Debug.Log($"Level Completed And Saved: Next Level {levelNumber}, Total Score {totalScore}");
        }
        else
        {
            isGameCompleted = true;

            SaveManager.SaveProgress(levelNumber, totalScore, isGameCompleted);
            AnalyticsManager.LogGameComplete(totalScore, levelNumber);

            Debug.Log($"Game Completed And Saved: Last Level {levelNumber}, Total Score {totalScore}");
        }
    }

    private void lander_onCoinPickUp(object sender, EventArgs e)
    {
        AddScore(500);

        int pickupCoinReward = 50;
        currentLevelCoins += pickupCoinReward;

        Debug.Log($"Current Level Coins: {currentLevelCoins}");
    }

    public void AddScore(int addScore)
    {
        score += addScore;
        Debug.Log(score);
    }

    public int GetScore()
    {
        return score;
    }

    public float GetTime()
    {
        return time;
    }

    public int GetTotalScore()
    {
        return totalScore;
    }

    public void NextLevel()
    {
        if (isGameCompleted)
        {
            SceneLoader.LoadScene(SceneLoader.Scene.GameOverScreen);
            return;
        }

        if (GetGameLevel() == null)
        {
            SceneLoader.LoadScene(SceneLoader.Scene.GameOverScreen);
        }
        else
        {
            SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
        }
    }

    public void RetryLevel()
    {
        AnalyticsManager.LogLevelRetry(levelNumber, time);
        SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
    }

    public int GetLevelNumber()
    {
        return levelNumber;
    }

    public void PauseOrUnPauseGame()
    {
        if (Time.timeScale == 1f)
        {
            PauseGame();
        }
        else
        {
            UnPauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        OnGamePaused?.Invoke(this, EventArgs.Empty);
    }

    public void UnPauseGame()
    {
        Time.timeScale = 1f;
        OnGameUnPaused?.Invoke(this, EventArgs.Empty);
    }

    private string GetLandingFailReason(PlayerMovement.LandingType landingType)
    {
        switch (landingType)
        {
            case PlayerMovement.LandingType.WrongLandingArea:
                return "wrong_landing_area";

            case PlayerMovement.LandingType.TooSpeedLanding:
                return "too_fast";

            case PlayerMovement.LandingType.TooSteepAngle:
                return "too_steep_angle";

            default:
                return "unknown";
        }
    }
}
