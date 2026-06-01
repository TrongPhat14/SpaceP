using System;
using DG.Tweening;
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
        PlayerMovement.Instance.onLanded += lander_onLanded;

        nextButton.Select();
        DOTweenUIAnimator.HidePanelImmediate(gameObject);
    }

    private void lander_onLanded(object sender, PlayerMovement.OnLandedEventArgs e)
    {
        bool successLanding = e.landingType == PlayerMovement.LandingType.Success;

        if (successLanding)
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

        DOTweenUIAnimator.PunchScale(titleTextMesh.transform, successLanding ? 0.14f : 0.22f);

        if (!successLanding && TryGetComponent(out RectTransform rectTransform))
        {
            rectTransform.DOShakeAnchorPos(0.28f, 12f, 14, 90f, false, true);
        }
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
        DOTweenUIAnimator.ShowPanel(gameObject);
        nextButton.Select();
    }

    private void Hide()
    {
        DOTweenUIAnimator.HidePanel(gameObject);
    }
}
