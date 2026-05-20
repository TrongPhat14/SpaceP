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
        PlayerMovement.instance.onCoinPickUp += lander_onCoinPickUp;
        PlayerMovement.instance.onLanded += Lander_onLanded;
        PlayerMovement.instance.onStateChange += Lander_onStateChange;

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

    public static void SetProgress(int newLevelNumber, int newTotalScore)
    {
        SetProgress(newLevelNumber, newTotalScore, false);
    }

    public static void SetProgress(int newLevelNumber, int newTotalScore, bool newIsGameCompleted)
    {
        levelNumber = newLevelNumber;
        totalScore = newTotalScore;
        isGameCompleted = newIsGameCompleted;

        SaveManager.SaveProgress(levelNumber, totalScore, isGameCompleted);
    }

    private void GameInput_OnMenuButtonPressed(object sender, EventArgs e)
    {
        PauseOrUnPauseGame();
    }

    private void Lander_onStateChange(object sender, PlayerMovement.OnStateChangeEventArgs e)
    {
        isRunning = e.State == PlayerMovement.State.Normal;

        if (e.State == PlayerMovement.State.Normal)
        {
            cinemachineCamera.Target.TrackingTarget = PlayerMovement.instance.transform;
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

        // STORE CHANGED:
        // Gán vào field spawnedGameLevel thay vì chỉ dùng local variable.
        spawnedGameLevel = Instantiate(
            gamelevel,
            UnityEngine.Vector3.zero,
            UnityEngine.Quaternion.identity
        );

        PlayerMovement.instance.transform.position = spawnedGameLevel.GetLanderStartPosition();

        cinemachineCamera.Target.TrackingTarget = spawnedGameLevel.GetCameraStartTargetTransform();

        CinemachineCameraZoom.Instance.SetTargetOrthographicSize(
            spawnedGameLevel.GetZoomOutOrthographicSize()
        );
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
            return;
        }

        AddScore(e.score);

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

        // STORE ADDED:
        // Thưởng coin khi hoàn thành màn.
        // Reward lấy từ GameLevel hiện tại.
        int rewardCoins = 0;

        if (spawnedGameLevel != null)
        {
            rewardCoins = spawnedGameLevel.GetCompleteCoinReward();
        }

        // STORE FIX CHANGED:
        // Tổng coin thật sự nhận được sau khi qua màn thành công.
        // Bao gồm coin nhặt trong màn + coin thưởng hoàn thành level.
        int totalCoinsEarnedThisLevel = currentLevelCoins + rewardCoins;

        // STORE FIX CHANGED:
        // Chỉ lúc hoàn thành màn thành công mới cộng vào PlayerCurrency.
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

    public bool IsGameCompleted()
    {
        return isGameCompleted;
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
}