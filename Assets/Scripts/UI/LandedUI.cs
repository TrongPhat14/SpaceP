using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LandedUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleTextMesh;
    [SerializeField] private TextMeshProUGUI statsTextMesh;
    [SerializeField] private TextMeshProUGUI nextButtonTextMexh;
    [SerializeField] private Button nextButton;

    private Action nextButtonClickAction;

    private void Awake()
    {
        nextButton.onClick.AddListener(() =>
        {
            nextButtonClickAction?.Invoke();
        });
    }

    private void Start()
    {
        PlayerMovement.instance.onLanded += lander_onLanded;

        nextButton.Select();
        Hide();
    }

    private void lander_onLanded(object sender, PlayerMovement.OnLandedEventArgs e)
    {
        if (e.landingType == PlayerMovement.LandingType.Success)
        {
            titleTextMesh.text = "SUCCESSFUL LANDING!";
            nextButtonTextMexh.text = "NEXT LEVEL";
            nextButtonClickAction = GameManager.Instance.NextLevel;
        }
        else
        {
            titleTextMesh.text = GetCrashTitle(e.landingType);
            nextButtonTextMexh.text = "RESTART";
            nextButtonClickAction = GameManager.Instance.RetryLevel;
        }

        statsTextMesh.text =
            Mathf.Round(e.speed * 2f) + "\n" +
            Mathf.Round(e.dotVector * 100f) + "\n" +
            "x" + e.scoreMultiplier + "\n" +
            e.score;

        Show();
    }
    private string GetCrashTitle(PlayerMovement.LandingType landingType)
    {
        switch (landingType)
        {
            case PlayerMovement.LandingType.WrongLandingArea:
                return "<color=#ff0000>TERRAIN HIT</color>";

            case PlayerMovement.LandingType.TooSpeedLanding:
                return "<color=#ff0000>TOO FAST</color>";

            case PlayerMovement.LandingType.TooSteepAngle:
                return "<color=#ff0000>BAD ANGLE</color>";

            default:
                return "<color=#ff0000>CRASH!</color>";
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
        nextButton.Select();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}