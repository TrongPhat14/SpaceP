using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    private enum TutorialStep
    {
        Thrust,
        RotateLeft,
        RotateRight,
        Fuel,
        Landing,
        Completed
    }

    private const float FuelStepDuration = 2f;
    private const float LandingStepDuration = 2.5f;

    [Header("Tutorial UI")]
    [SerializeField] private RectTransform highlightFrame;
    [SerializeField] private RectTransform tutorialPopup;
    [SerializeField] private CanvasGroup popupCanvasGroup;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private Button skipButton;
    [SerializeField] private StatsUI statsUI;

    [Header("Highlight Targets")]
    [SerializeField] private RectTransform upButtonVisual;
    [SerializeField] private RectTransform leftButtonVisual;
    [SerializeField] private RectTransform rightButtonVisual;
    [SerializeField] private RectTransform fuelBar;

    [Header("Layout")]
    [SerializeField] private Vector2 highlightPadding = new Vector2(28f, 28f);
    [SerializeField] private float highlightBorderThickness = 8f;
    [SerializeField] private Color highlightBorderColor =
        new Color(0.05f, 0.9f, 1f, 1f);
    [SerializeField] private float popupSpacing = 35f;
    [SerializeField] private float screenPadding = 20f;

    private RectTransform rootRect;
    private TutorialStep currentStep;
    private Coroutine timedStepCoroutine;
    private Coroutine unlockCoroutine;
    private Tween highlightTween;
    private bool isActive;
    private bool inputSubscribed;
    private bool landingSubscribed;
    private bool controlsLocked;

    private IEnumerator Start()
    {
        rootRect = (RectTransform)transform;
        ConfigureHighlightFrame();
        ConfigureSkipButtonAppearance();
        SetPresentationVisible(false);

        yield return null;

        if (GameManager.Instance == null ||
            GameManager.Instance.GetLevelNumber() != 1 ||
            PlayerPrefs.GetInt(SaveKeys.ControlsTutorialCompleted, 0) == 1)
        {
            gameObject.SetActive(false);
            yield break;
        }

        if (!HasRequiredReferences() ||
            GameInput.Instance == null ||
            PlayerMovement.Instance == null)
        {
            Debug.LogWarning(
                "TutorialUI is missing scene references. Assign its UI and highlight targets in the Inspector.",
                this
            );
            gameObject.SetActive(false);
            yield break;
        }

        skipButton.onClick.AddListener(SkipTutorial);
        SetPlayerControlLocked(true);
        SubscribeEvents();
        isActive = true;
        SetPresentationVisible(true);
        SetStep(TutorialStep.Thrust);
    }

    private void ConfigureHighlightFrame()
    {
        Image fillImage = highlightFrame != null
            ? highlightFrame.GetComponent<Image>()
            : null;
        if (fillImage != null)
        {
            fillImage.color = Color.clear;
            fillImage.raycastTarget = false;
        }

        Outline outline = highlightFrame != null
            ? highlightFrame.GetComponent<Outline>()
            : null;
        if (outline != null)
        {
            outline.enabled = false;
        }

        if (highlightFrame == null ||
            highlightFrame.Find("TopBorder") != null)
        {
            return;
        }

        CreateHighlightBorder(
            "TopBorder",
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0f, -highlightBorderThickness * 0.5f),
            new Vector2(0f, highlightBorderThickness)
        );
        CreateHighlightBorder(
            "BottomBorder",
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, highlightBorderThickness * 0.5f),
            new Vector2(0f, highlightBorderThickness)
        );
        CreateHighlightBorder(
            "LeftBorder",
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            new Vector2(highlightBorderThickness * 0.5f, 0f),
            new Vector2(highlightBorderThickness, 0f)
        );
        CreateHighlightBorder(
            "RightBorder",
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(-highlightBorderThickness * 0.5f, 0f),
            new Vector2(highlightBorderThickness, 0f)
        );
    }

    private void ConfigureSkipButtonAppearance()
    {
        if (skipButton == null)
        {
            return;
        }

        Image backgroundImage = skipButton.GetComponent<Image>();
        if (backgroundImage != null)
        {
            backgroundImage.color = Color.clear;
            backgroundImage.raycastTarget = true;
        }

        skipButton.transition = Selectable.Transition.None;
    }

    private void CreateHighlightBorder(
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        Vector2 sizeDelta
    )
    {
        GameObject borderObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        borderObject.layer = highlightFrame.gameObject.layer;
        borderObject.transform.SetParent(highlightFrame, false);

        RectTransform borderRect =
            borderObject.GetComponent<RectTransform>();
        borderRect.anchorMin = anchorMin;
        borderRect.anchorMax = anchorMax;
        borderRect.anchoredPosition = anchoredPosition;
        borderRect.sizeDelta = sizeDelta;

        Image borderImage = borderObject.GetComponent<Image>();
        borderImage.color = highlightBorderColor;
        borderImage.raycastTarget = false;
    }

    private bool HasRequiredReferences()
    {
        if (statsUI == null && fuelBar != null)
        {
            statsUI = fuelBar.GetComponentInParent<StatsUI>();
        }

        return highlightFrame != null &&
            tutorialPopup != null &&
            popupCanvasGroup != null &&
            titleText != null &&
            instructionText != null &&
            skipButton != null &&
            upButtonVisual != null &&
            leftButtonVisual != null &&
            rightButtonVisual != null &&
            fuelBar != null;
    }

    private void SetPresentationVisible(bool visible)
    {
        if (highlightFrame != null)
        {
            highlightFrame.gameObject.SetActive(visible);
        }

        if (tutorialPopup != null)
        {
            tutorialPopup.gameObject.SetActive(visible);
        }
    }

    private void SubscribeEvents()
    {
        GameInput.Instance.OnUpButtonPressed += GameInput_OnUpButtonPressed;
        GameInput.Instance.OnLeftButtonPressed += GameInput_OnLeftButtonPressed;
        GameInput.Instance.OnRightButtonPressed += GameInput_OnRightButtonPressed;
        inputSubscribed = true;

        PlayerMovement.Instance.onLanded += PlayerMovement_OnLanded;
        landingSubscribed = true;
    }

    private void GameInput_OnUpButtonPressed(object sender, EventArgs e)
    {
        if (CanAdvance(TutorialStep.Thrust))
        {
            SetStep(TutorialStep.RotateLeft);
        }
    }

    private void GameInput_OnLeftButtonPressed(object sender, EventArgs e)
    {
        if (CanAdvance(TutorialStep.RotateLeft))
        {
            SetStep(TutorialStep.RotateRight);
        }
    }

    private void GameInput_OnRightButtonPressed(object sender, EventArgs e)
    {
        if (CanAdvance(TutorialStep.RotateRight))
        {
            SetStep(TutorialStep.Fuel);
        }
    }

    private bool CanAdvance(TutorialStep expectedStep)
    {
        return isActive &&
            Time.timeScale > 0f &&
            currentStep == expectedStep;
    }

    private void PlayerMovement_OnLanded(
        object sender,
        PlayerMovement.OnLandedEventArgs e
    )
    {
        if (isActive && currentStep != TutorialStep.Completed)
        {
            AbortTutorial();
        }
    }

    private void SetStep(TutorialStep step)
    {
        if (!isActive)
        {
            return;
        }

        currentStep = step;
        StopTimedStep();
        statsUI?.SetTutorialPreviewVisible(step == TutorialStep.Fuel);

        switch (step)
        {
            case TutorialStep.Thrust:
                ShowControlStep(
                    upButtonVisual,
                    "THRUST",
                    "HOLD TO THRUST\nMovement consumes fuel."
                );
                break;

            case TutorialStep.RotateLeft:
                ShowControlStep(
                    leftButtonVisual,
                    "ROTATE LEFT",
                    "HOLD TO ROTATE LEFT"
                );
                break;

            case TutorialStep.RotateRight:
                ShowControlStep(
                    rightButtonVisual,
                    "ROTATE RIGHT",
                    "HOLD TO ROTATE RIGHT"
                );
                break;

            case TutorialStep.Fuel:
                ShowCenteredStep(
                    fuelBar,
                    "FUEL",
                    "THRUST AND ROTATION USE FUEL\nCollect fuel items to refill."
                );
                timedStepCoroutine = StartCoroutine(
                    AdvanceAfterDelay(FuelStepDuration, TutorialStep.Landing)
                );
                break;

            case TutorialStep.Landing:
                HideHighlight();
                ShowPopup(
                    "LAND SLOWLY",
                    "Keep the ship upright for a safe landing.",
                    Vector2.zero
                );
                timedStepCoroutine = StartCoroutine(
                    CompleteAfterDelay(LandingStepDuration)
                );
                break;
        }
    }

    private void ShowControlStep(
        RectTransform target,
        string title,
        string instruction
    )
    {
        GetTargetRect(target, out Vector2 center, out Vector2 size);
        ShowHighlight(center, size);

        float popupOffset =
            size.y * 0.5f +
            tutorialPopup.rect.height * 0.5f +
            popupSpacing;
        ShowPopup(
            title,
            instruction,
            ClampPopupPosition(center + Vector2.up * popupOffset)
        );
    }

    private void ShowCenteredStep(
        RectTransform target,
        string title,
        string instruction
    )
    {
        GetTargetRect(target, out Vector2 center, out Vector2 size);
        ShowHighlight(center, size);
        ShowPopup(title, instruction, new Vector2(0f, 20f));
    }

    private void ShowPopup(
        string title,
        string instruction,
        Vector2 position
    )
    {
        titleText.text = title;
        instructionText.text = instruction;
        tutorialPopup.anchoredPosition = position;
        tutorialPopup.gameObject.SetActive(true);

        tutorialPopup.DOKill();
        popupCanvasGroup.DOKill();
        tutorialPopup.localScale = Vector3.one * 0.92f;
        popupCanvasGroup.alpha = 0f;

        popupCanvasGroup
            .DOFade(1f, 0.18f)
            .SetUpdate(true)
            .SetLink(gameObject);
        tutorialPopup
            .DOScale(Vector3.one, 0.22f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .SetLink(gameObject);
    }

    private void ShowHighlight(Vector2 center, Vector2 targetSize)
    {
        highlightFrame.gameObject.SetActive(true);
        highlightFrame.anchoredPosition = center;
        highlightFrame.sizeDelta = targetSize + highlightPadding;
        highlightFrame.localScale = Vector3.one;

        highlightTween?.Kill();
        highlightTween = highlightFrame
            .DOScale(1.08f, 0.55f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true)
            .SetLink(gameObject);
    }

    private void HideHighlight()
    {
        highlightTween?.Kill();
        highlightTween = null;

        if (highlightFrame != null)
        {
            highlightFrame.gameObject.SetActive(false);
        }
    }

    private void GetTargetRect(
        RectTransform target,
        out Vector2 center,
        out Vector2 size
    )
    {
        Vector3[] worldCorners = new Vector3[4];
        target.GetWorldCorners(worldCorners);

        Vector3 bottomLeft = rootRect.InverseTransformPoint(worldCorners[0]);
        Vector3 topRight = rootRect.InverseTransformPoint(worldCorners[2]);

        center = (bottomLeft + topRight) * 0.5f;
        size = new Vector2(
            Mathf.Abs(topRight.x - bottomLeft.x),
            Mathf.Abs(topRight.y - bottomLeft.y)
        );
    }

    private Vector2 ClampPopupPosition(Vector2 position)
    {
        float halfWidth = tutorialPopup.rect.width * 0.5f + screenPadding;
        float halfHeight = tutorialPopup.rect.height * 0.5f + screenPadding;
        Rect rootBounds = rootRect.rect;

        position.x = Mathf.Clamp(
            position.x,
            rootBounds.xMin + halfWidth,
            rootBounds.xMax - halfWidth
        );
        position.y = Mathf.Clamp(
            position.y,
            rootBounds.yMin + halfHeight,
            rootBounds.yMax - halfHeight
        );
        return position;
    }

    private IEnumerator AdvanceAfterDelay(
        float delay,
        TutorialStep nextStep
    )
    {
        yield return new WaitForSeconds(delay);
        timedStepCoroutine = null;

        if (isActive)
        {
            SetStep(nextStep);
        }
    }

    private IEnumerator CompleteAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        timedStepCoroutine = null;

        if (isActive)
        {
            CompleteTutorial();
        }
    }

    private void SkipTutorial()
    {
        CompleteTutorial();
    }

    private void CompleteTutorial()
    {
        if (!isActive)
        {
            return;
        }

        currentStep = TutorialStep.Completed;
        PlayerPrefs.SetInt(SaveKeys.ControlsTutorialCompleted, 1);
        PlayerPrefs.Save();
        CloseTutorial();
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Tutorial/Reset Controls Tutorial")]
    private static void ResetTutorialForTesting()
    {
        PlayerPrefs.DeleteKey(SaveKeys.ControlsTutorialCompleted);
        PlayerPrefs.Save();
        Debug.Log("Controls tutorial progress reset.");
    }
#endif

    private void AbortTutorial()
    {
        if (isActive)
        {
            CloseTutorial();
        }
    }

    private void CloseTutorial()
    {
        isActive = false;
        statsUI?.SetTutorialPreviewVisible(false);
        StopTimedStep();
        HideHighlight();
        UnsubscribeEvents();

        tutorialPopup.DOKill();
        popupCanvasGroup.DOKill();
        popupCanvasGroup
            .DOFade(0f, 0.15f)
            .SetUpdate(true)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                if (this != null)
                {
                    SetPresentationVisible(false);
                }
            });

        StopUnlockCoroutine();
        unlockCoroutine = StartCoroutine(
            UnlockAfterMovementInputReleased()
        );
    }

    private IEnumerator UnlockAfterMovementInputReleased()
    {
        yield return null;

        while (GameInput.Instance != null &&
            (GameInput.Instance.IsUpActionPressed() ||
             GameInput.Instance.IsLeftActionPressed() ||
             GameInput.Instance.IsRightActionPressed() ||
             GameInput.Instance.GetMovementInputVector2() != Vector2.zero))
        {
            yield return null;
        }

        unlockCoroutine = null;
        SetPlayerControlLocked(false);
        gameObject.SetActive(false);
    }

    private void SetPlayerControlLocked(bool isLocked)
    {
        if (controlsLocked == isLocked)
        {
            return;
        }

        controlsLocked = isLocked;
        PlayerMovement.Instance?.SetTutorialControlLocked(isLocked);
    }

    private void StopUnlockCoroutine()
    {
        if (unlockCoroutine == null)
        {
            return;
        }

        StopCoroutine(unlockCoroutine);
        unlockCoroutine = null;
    }

    private void StopTimedStep()
    {
        if (timedStepCoroutine == null)
        {
            return;
        }

        StopCoroutine(timedStepCoroutine);
        timedStepCoroutine = null;
    }

    private void UnsubscribeEvents()
    {
        if (inputSubscribed && GameInput.Instance != null)
        {
            GameInput.Instance.OnUpButtonPressed -= GameInput_OnUpButtonPressed;
            GameInput.Instance.OnLeftButtonPressed -= GameInput_OnLeftButtonPressed;
            GameInput.Instance.OnRightButtonPressed -= GameInput_OnRightButtonPressed;
            inputSubscribed = false;
        }

        if (landingSubscribed && PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.onLanded -= PlayerMovement_OnLanded;
            landingSubscribed = false;
        }
    }

    private void OnDestroy()
    {
        StopTimedStep();
        StopUnlockCoroutine();
        highlightTween?.Kill();
        tutorialPopup?.DOKill();
        popupCanvasGroup?.DOKill();
        skipButton?.onClick.RemoveListener(SkipTutorial);
        UnsubscribeEvents();
        SetPlayerControlLocked(false);
    }

    private void OnDisable()
    {
        StopUnlockCoroutine();
        SetPlayerControlLocked(false);
    }
}
