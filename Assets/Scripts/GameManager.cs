
using System;
using System.Collections.Generic;
using System.Numerics;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    private static int levelNumber = 1;
    private static int totalScore = 0;

    public static void ResetStaticData()
    {
        levelNumber = 1;
        totalScore = 0;
    }

    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnPaused;

    [SerializeField] private List<GameLevel> gameLevelList;
    [SerializeField] private CinemachineCamera  cinemachineCamera;

    private int score;
    private float time;
    private bool isRunning;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        PlayerMovement.instance.onCoinPickUp += lander_onCoinPickUp;
        PlayerMovement.instance.onLanded += Lander_onLanded;
        PlayerMovement.instance.onStateChange += Lander_onStateChange; ;

        GameInput.Instance.OnMenuButtonPressed += GameInput_OnMenuButtonPressed;
        loadCurrentLevel();
    }

    private void GameInput_OnMenuButtonPressed(object sender, EventArgs e)
    {
        PauseOrUnPauseGame();
    }

    private void Lander_onStateChange(object sender, PlayerMovement.OnStateChangeEventArgs e)
    {
        isRunning = e.ste == PlayerMovement.State.Normal;
        if(e.ste == PlayerMovement.State.Normal )
        {
            cinemachineCamera.Target.TrackingTarget = PlayerMovement.instance.transform;
            CinemachineCameraZoom.Instance.SetNormalOrthographicSize();

        }
    }


    private void Update()
    {
       if(isRunning) {
         time += Time.deltaTime;
       }
    }

    private void loadCurrentLevel()
    {
        GameLevel gamelevel = GetGameLevel();
        GameLevel spawnedGameLevel = Instantiate(gamelevel, UnityEngine.Vector3.zero, UnityEngine.Quaternion.identity);
        PlayerMovement.instance.transform.position = spawnedGameLevel.GetLanderStartPosition();
        cinemachineCamera.Target.TrackingTarget = spawnedGameLevel.GetCameraStartTargetTransform();
        CinemachineCameraZoom.Instance.SetTargetOrthographicSize(spawnedGameLevel.GetZoomOutOrthographicSize());
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

    private void Lander_onLanded(object sender, PlayerMovement.OnLandedEventArgs e)
    {
        AddScore(e.score);
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
    public void NextLevel()
    {
        levelNumber++;
        totalScore += score;

        if(GetGameLevel() == null)
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
