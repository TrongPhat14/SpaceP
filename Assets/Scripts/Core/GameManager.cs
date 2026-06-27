using System;
using System.Collections.Generic;
using SpaceP.Scoring;
using Unity.Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public const int CoinPickupScoreReward = 500;
    public const int CoinPickupCurrencyReward = 50;

    public static GameManager Instance { get; private set; }

    private static int levelNumber = 1;
    private static int totalScore = 0;
    private static bool isGameCompleted = false;

    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnPaused;

    [SerializeField] private List<GameLevel> gameLevelList;
    [SerializeField] private CinemachineCamera cinemachineCamera;

    private int score;
    private int currentLevelCoins;
    private float time;
    private bool isRunning;
    private bool hasLevelCompleted;
    private bool hasLevelEnded;
    private GameLevel spawnedGameLevel;

    public static void ResetStaticData()
    {
        levelNumber = 1;
        totalScore = 0;
        isGameCompleted = false;
    }

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        LoadSavedProgress();
    }

    private void Start()
    {
        SubscribeToPlayerEvents();
        SubscribeToInputEvents();
        LoadCurrentLevel();
    }

    private void Update()
    {
        if (isRunning)
        {
            time += Time.deltaTime;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromPlayerEvents();
        UnsubscribeFromInputEvents();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void SubscribeToPlayerEvents()
    {
        if (PlayerMovement.Instance == null)
        {
            Debug.LogError("GameManager could not find PlayerMovement.Instance.", this);
            return;
        }

        PlayerMovement.Instance.onCoinPickUp += HandleCoinPickup;
        PlayerMovement.Instance.onLanded += HandleLandingResult;
        PlayerMovement.Instance.onStateChange += HandlePlayerStateChanged;
    }

    private void UnsubscribeFromPlayerEvents()
    {
        if (PlayerMovement.Instance == null)
        {
            return;
        }

        PlayerMovement.Instance.onCoinPickUp -= HandleCoinPickup;
        PlayerMovement.Instance.onLanded -= HandleLandingResult;
        PlayerMovement.Instance.onStateChange -= HandlePlayerStateChanged;
    }

    private void SubscribeToInputEvents()
    {
        if (GameInput.Instance == null)
        {
            Debug.LogError("GameManager could not find GameInput.Instance.", this);
            return;
        }

        GameInput.Instance.OnMenuButtonPressed += HandleMenuButtonPressed;
    }

    private void UnsubscribeFromInputEvents()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnMenuButtonPressed -= HandleMenuButtonPressed;
        }
    }

    private void LoadSavedProgress()
    {
        SaveData saveData = SaveManager.LoadProgress();

        levelNumber = saveData.levelNumber;
        totalScore = saveData.totalScore;
        isGameCompleted = saveData.isGameCompleted;

        Debug.Log($"Loaded Save: Level {levelNumber}, Total Score {totalScore}, Completed {isGameCompleted}");
    }

    private void LoadCurrentLevel()
    {
        if (isGameCompleted)
        {
            Debug.Log("Game already completed. Load GameOverScreen.");
            SceneLoader.LoadScene(SceneLoader.Scene.GameOverScreen);
            return;
        }

        GameLevel levelPrefab = FindLevelPrefab(levelNumber);

        if (levelPrefab == null)
        {
            Debug.LogError($"Can not find GameLevel with levelNumber: {levelNumber}", this);
            SceneLoader.LoadScene(SceneLoader.Scene.GameOverScreen);
            return;
        }

        spawnedGameLevel = Instantiate(levelPrefab, Vector3.zero, Quaternion.identity);

        if (!spawnedGameLevel.HasRequiredReferences())
        {
            Debug.LogWarning($"Level {spawnedGameLevel.GetLevelNumber()} is missing start references.", spawnedGameLevel);
        }

        PositionPlayerAtLevelStart();
        SetCameraForLevelIntro();
        AnalyticsManager.LogLevelStart(levelNumber, totalScore);
        MechanicTutorialUI.Instance?.TryShow(spawnedGameLevel.GetMechanicTutorial());
    }

    private GameLevel FindLevelPrefab(int targetLevelNumber)
    {
        if (gameLevelList == null)
        {
            return null;
        }

        foreach (GameLevel level in gameLevelList)
        {
            if (level != null && level.GetLevelNumber() == targetLevelNumber)
            {
                return level;
            }
        }

        return null;
    }

    private bool HasGameLevel(int checkLevelNumber)
    {
        return FindLevelPrefab(checkLevelNumber) != null;
    }

    private void PositionPlayerAtLevelStart()
    {
        if (PlayerMovement.Instance == null || spawnedGameLevel == null)
        {
            return;
        }

        PlayerMovement.Instance.transform.position = spawnedGameLevel.GetLanderStartPosition();
    }

    private void SetCameraForLevelIntro()
    {
        if (cinemachineCamera != null && spawnedGameLevel != null)
        {
            cinemachineCamera.Target.TrackingTarget = spawnedGameLevel.GetCameraStartTargetTransform();
        }

        if (CinemachineCameraZoom.Instance != null && spawnedGameLevel != null)
        {
            CinemachineCameraZoom.Instance.SetTargetOrthographicSize(spawnedGameLevel.GetZoomOutOrthographicSize());
        }
    }

    private void SetCameraForPlayerFollow()
    {
        if (cinemachineCamera != null && PlayerMovement.Instance != null)
        {
            cinemachineCamera.Target.TrackingTarget = PlayerMovement.Instance.transform;
        }

        CinemachineCameraZoom.Instance?.SetNormalOrthographicSize();
    }

    private void HandleMenuButtonPressed(object sender, EventArgs e)
    {
        if (!hasLevelEnded)
        {
            PauseOrUnPauseGame();
        }
    }

    private void HandlePlayerStateChanged(object sender, PlayerMovement.OnStateChangeEventArgs e)
    {
        isRunning = e.State == PlayerMovement.State.Normal;

        if (e.State == PlayerMovement.State.Normal)
        {
            SetCameraForPlayerFollow();
        }
    }

    private void HandleLandingResult(object sender, PlayerMovement.OnLandedEventArgs e)
    {
        hasLevelEnded = true;

        if (!e.Result.IsSuccess)
        {
            LogLevelFail(e.Result);
            return;
        }

        AddScore(e.Result.Score);
        AnalyticsManager.LogLevelComplete(levelNumber, score, totalScore + score, time);
        CompleteLevelAndSave();
    }

    private void HandleCoinPickup(object sender, EventArgs e)
    {
        AddScore(CoinPickupScoreReward);
        currentLevelCoins += CoinPickupCurrencyReward;
        Debug.Log($"Current Level Coins: {currentLevelCoins}");
    }

    private void LogLevelFail(LandingResult result)
    {
        AnalyticsManager.LogLevelFail(
            levelNumber,
            GetLandingFailReason(result.Type),
            time,
            result.ImpactSpeed,
            result.Uprightness * 100f);
    }

    public void CompleteLevelAndSave()
    {
        if (hasLevelCompleted)
        {
            return;
        }

        hasLevelCompleted = true;
        totalScore += score;

        int rewardCoins = GetCurrentLevelCompleteCoinReward();
        int totalCoinsEarnedThisLevel = currentLevelCoins + rewardCoins;
        PlayerCurrency.AddCoins(totalCoinsEarnedThisLevel);

        Debug.Log($"Pickup Coins This Level: {currentLevelCoins}");
        Debug.Log($"Complete Level Coin Reward: {rewardCoins}");
        Debug.Log($"Total Coins Earned This Level: {totalCoinsEarnedThisLevel}");

        SaveNextProgress();
    }

    private void SaveNextProgress()
    {
        int nextLevelNumber = levelNumber + 1;

        if (HasGameLevel(nextLevelNumber))
        {
            levelNumber = nextLevelNumber;
            isGameCompleted = false;
            SaveManager.SaveProgress(levelNumber, totalScore, isGameCompleted);
            Debug.Log($"Level Completed And Saved: Next Level {levelNumber}, Total Score {totalScore}");
            return;
        }

        isGameCompleted = true;
        SaveManager.SaveProgress(levelNumber, totalScore, isGameCompleted);
        AnalyticsManager.LogGameComplete(totalScore, levelNumber);
        Debug.Log($"Game Completed And Saved: Last Level {levelNumber}, Total Score {totalScore}");
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

    public int GetLevelNumber()
    {
        return levelNumber;
    }

    public int GetCurrentLevelCompleteCoinReward()
    {
        return spawnedGameLevel != null ? spawnedGameLevel.GetCompleteCoinReward() : 0;
    }

    public bool HasLevelEnded()
    {
        return hasLevelEnded;
    }

    public void NextLevel()
    {
        if (isGameCompleted)
        {
            SceneLoader.LoadScene(SceneLoader.Scene.GameOverScreen);
            return;
        }

        SceneLoader.LoadScene(FindLevelPrefab(levelNumber) == null
            ? SceneLoader.Scene.GameOverScreen
            : SceneLoader.Scene.GameScene);
    }

    public void RetryLevel()
    {
        AnalyticsManager.LogLevelRetry(levelNumber, time);
        SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
    }

    public void PauseOrUnPauseGame()
    {
        if (hasLevelEnded)
        {
            return;
        }

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
        if (hasLevelEnded)
        {
            return;
        }

        Time.timeScale = 0f;
        OnGamePaused?.Invoke(this, EventArgs.Empty);
    }

    public void UnPauseGame()
    {
        Time.timeScale = 1f;
        OnGameUnPaused?.Invoke(this, EventArgs.Empty);
    }

    private string GetLandingFailReason(LandingType landingType)
    {
        switch (landingType)
        {
            case LandingType.WrongLandingArea:
                return "wrong_landing_area";

            case LandingType.TooFast:
                return "too_fast";

            case LandingType.TooSteep:
                return "too_steep_angle";

            default:
                return "unknown";
        }
    }
}
