
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

        loadCurrentLevel();
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
        foreach(GameLevel level in gameLevelList)
        {
            if(level.GetLevelNumber() == levelNumber)
            {
               GameLevel spawnedGameLevel =  Instantiate(level, UnityEngine.Vector3.zero, UnityEngine.Quaternion.identity);
               PlayerMovement.instance.transform.position =  spawnedGameLevel.GetLanderStartPosition();
               cinemachineCamera.Target.TrackingTarget = spawnedGameLevel.GetCameraStartTargetTransform();
               CinemachineCameraZoom.Instance.SetTargetOrthographicSize(spawnedGameLevel.GetZoomOutOrthographicSize());
            }
        }
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

    public void NextLevel()
    {
        levelNumber++;
        SceneManager.LoadScene(0);
    }

    public void RetryLevel()
    {
        SceneManager.LoadScene(0);
    }

    public int GetLevelNumber()
    {
        return levelNumber;
    }
}
