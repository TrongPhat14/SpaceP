using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private static int levelNumber = 1;
    private static int totalScore = 0;

    // COMPLETED ADDED:
    // Lưu trạng thái hoàn thành game trong runtime.
    private static bool isGameCompleted = false;

    public static void ResetStaticData()
    {
        levelNumber = 1;
        totalScore = 0;

        // COMPLETED ADDED:
        // Khi reset game thì không còn trạng thái completed.
        isGameCompleted = false;
    }

    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnPaused;

    [SerializeField] private List<GameLevel> gameLevelList;
    [SerializeField] private CinemachineCamera cinemachineCamera;

    private int score;
    private float time;
    private bool isRunning;

    // SAVE ADDED:
    // Dùng để tránh việc hạ cánh thành công bị xử lý/save nhiều lần.
    private bool hasLevelCompleted;

    private void Awake()
    {
        Instance = this;

        // SAVE:
        // Khi vào GameScene, đọc tiến trình đã lưu.
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

        // COMPLETED ADDED:
        // Đọc trạng thái completed từ save.
        isGameCompleted = saveData.isGameCompleted;

        Debug.Log($"Loaded Save: Level {levelNumber}, Total Score {totalScore}, Completed {isGameCompleted}");
    }

    // COMPLETED ADDED:
    // Giữ hàm cũ để code cũ không bị lỗi.
    public static void SetProgress(int newLevelNumber, int newTotalScore)
    {
        SetProgress(newLevelNumber, newTotalScore, false);
    }

    // COMPLETED ADDED:
    // Hàm mới có thêm trạng thái completed.
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
        // COMPLETED ADDED:
        // Nếu vì lý do nào đó Continue vẫn vào GameScene khi đã completed,
        // thì chuyển sang GameOverScreen thay vì cố load level.
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

        GameLevel spawnedGameLevel = Instantiate(
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

    // COMPLETED ADDED:
    // Kiểm tra một level number có tồn tại trong gameLevelList không.
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

        // COMPLETED ADDED:
        // Không tăng levelNumber mù quáng nữa.
        // Trước tiên kiểm tra level tiếp theo có tồn tại không.
        int nextLevelNumber = levelNumber + 1;

        if (HasGameLevel(nextLevelNumber))
        {
            // COMPLETED ADDED:
            // Còn màn tiếp theo thì save màn tiếp theo.
            levelNumber = nextLevelNumber;
            isGameCompleted = false;

            SaveManager.SaveProgress(levelNumber, totalScore, isGameCompleted);

            Debug.Log($"Level Completed And Saved: Next Level {levelNumber}, Total Score {totalScore}");
        }
        else
        {
            // COMPLETED ADDED:
            // Không có màn tiếp theo.
            // Đây là hoàn thành game, không save level 4.
            isGameCompleted = true;

            SaveManager.SaveProgress(levelNumber, totalScore, isGameCompleted);

            Debug.Log($"Game Completed And Saved: Last Level {levelNumber}, Total Score {totalScore}");
        }
    }

    private void lander_onCoinPickUp(object sender, EventArgs e)
    {
        AddScore(500);
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

    // COMPLETED ADDED:
    // Cho UI khác đọc trạng thái completed nếu cần.
    public bool IsGameCompleted()
    {
        return isGameCompleted;
    }

    public void NextLevel()
    {
        // COMPLETED ADDED:
        // Nếu đã hoàn thành game thì NextLevel đi tới GameOverScreen.
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