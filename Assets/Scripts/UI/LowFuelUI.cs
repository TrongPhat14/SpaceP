using DG.Tweening;
using UnityEngine;

public class LowFuelUI : MonoBehaviour
{
    private const float LowFuelThreshold = 0.3f;
    private const float CriticalFuelThreshold = 0.15f;

    private CanvasGroup canvasGroup;
    private Tween lowFuelTween;
    private WarningLevel currentWarningLevel = WarningLevel.Hidden;

    private void Awake()
    {
        canvasGroup = DOTweenUIAnimator.EnsureCanvasGroup(gameObject);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void Update()
    {
        float fuel = PlayerMovement.Instance.GetFuelAmountNormalized();
        WarningLevel nextWarningLevel = GetWarningLevel(fuel);

        if (nextWarningLevel == currentWarningLevel)
        {
            return;
        }

        currentWarningLevel = nextWarningLevel;

        switch (currentWarningLevel)
        {
            case WarningLevel.Low:
                ShowWarning(0.22f, 0.45f);
                break;

            case WarningLevel.Critical:
                ShowWarning(0.5f, 1f);
                break;

            default:
                HideWarning();
                break;
        }
    }

    private WarningLevel GetWarningLevel(float fuel)
    {
        if (fuel < CriticalFuelThreshold)
        {
            return WarningLevel.Critical;
        }

        if (fuel < LowFuelThreshold)
        {
            return WarningLevel.Low;
        }

        return WarningLevel.Hidden;
    }

    private void ShowWarning(float minAlpha, float maxAlpha)
    {
        lowFuelTween?.Kill();
        canvasGroup.alpha = minAlpha;
        lowFuelTween = canvasGroup
            .DOFade(maxAlpha, 0.35f)
            .SetLink(gameObject)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void HideWarning()
    {
        lowFuelTween?.Kill();
        lowFuelTween = canvasGroup
            .DOFade(0f, 0.18f)
            .SetLink(gameObject)
            .SetEase(Ease.OutQuad);
    }

    private void OnDisable()
    {
        lowFuelTween?.Kill();
        lowFuelTween = null;
        currentWarningLevel = WarningLevel.Hidden;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    private enum WarningLevel
    {
        Hidden,
        Low,
        Critical,
    }
}
