using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }


    private int score;
    private float time;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        PlayerMovement.instance.onCoinPickUp += lander_onCoinPickUp;
        PlayerMovement.instance.onLanded += Lander_onLanded;
    }

    private void Update()
    {
        time += Time.deltaTime;
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
}
