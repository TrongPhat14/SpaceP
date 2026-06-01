using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public static class DOTweenUIAnimator
{
    private const float PanelShowDuration = 0.22f;
    private const float PanelHideDuration = 0.14f;
    private const float FrameFadeDuration = 0.12f;
    private const float SelectedScale = 1f;
    private const float DeselectedScale = 0.95f;

    private static readonly Dictionary<int, bool> selectedFrameStates = new Dictionary<int, bool>();
    private static readonly Dictionary<int, Vector3> originalScales = new Dictionary<int, Vector3>();
    private static int currentSelectedFrameId;

    public static CanvasGroup EnsureCanvasGroup(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return null;
        }

        CanvasGroup canvasGroup = targetObject.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = targetObject.AddComponent<CanvasGroup>();
        }

        return canvasGroup;
    }

    public static Vector3 GetOriginalScale(Transform targetTransform)
    {
        int instanceId = targetTransform.GetInstanceID();

        if (!originalScales.TryGetValue(instanceId, out Vector3 originalScale))
        {
            originalScale = targetTransform.localScale;
            originalScales[instanceId] = originalScale;
        }

        return originalScale;
    }

    public static void ShowPanel(GameObject targetObject, bool useUnscaledTime = false)
    {
        CanvasGroup canvasGroup = EnsureCanvasGroup(targetObject);

        if (canvasGroup == null)
        {
            return;
        }

        Transform targetTransform = targetObject.transform;
        Vector3 originalScale = GetOriginalScale(targetTransform);
        targetObject.SetActive(true);
        DOTween.Kill(targetObject);
        targetTransform.DOKill();
        canvasGroup.DOKill();

        targetTransform.localScale = originalScale * 0.9f;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        DOTween.Sequence()
            .SetTarget(targetObject)
            .SetLink(targetObject)
            .SetUpdate(useUnscaledTime)
            .Join(canvasGroup.DOFade(1f, PanelShowDuration))
            .Join(targetTransform.DOScale(originalScale, PanelShowDuration).SetEase(Ease.OutBack));
    }

    public static void HidePanel(GameObject targetObject, bool useUnscaledTime = false)
    {
        CanvasGroup canvasGroup = EnsureCanvasGroup(targetObject);

        if (canvasGroup == null)
        {
            return;
        }

        Transform targetTransform = targetObject.transform;
        Vector3 originalScale = GetOriginalScale(targetTransform);
        DOTween.Kill(targetObject);
        targetTransform.DOKill();
        canvasGroup.DOKill();

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        DOTween.Sequence()
            .SetTarget(targetObject)
            .SetLink(targetObject)
            .SetUpdate(useUnscaledTime)
            .Join(canvasGroup.DOFade(0f, PanelHideDuration).SetEase(Ease.InQuad))
            .Join(targetTransform.DOScale(originalScale * 0.95f, PanelHideDuration).SetEase(Ease.InQuad))
            .OnComplete(() =>
            {
                if (targetObject != null)
                {
                    targetObject.SetActive(false);
                    targetTransform.localScale = originalScale;
                }
            });
    }

    public static void HidePanelImmediate(GameObject targetObject)
    {
        CanvasGroup canvasGroup = EnsureCanvasGroup(targetObject);

        if (canvasGroup == null)
        {
            return;
        }

        targetObject.transform.DOKill();
        DOTween.Kill(targetObject);
        canvasGroup.DOKill();
        targetObject.transform.localScale = GetOriginalScale(targetObject.transform);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        targetObject.SetActive(false);
    }

    public static void SetSelectedFrame(GameObject selectedFrame, bool selected, bool useUnscaledTime = false)
    {
        if (selectedFrame == null)
        {
            return;
        }

        int instanceId = selectedFrame.GetInstanceID();

        if (selectedFrameStates.TryGetValue(instanceId, out bool previousSelected) && previousSelected == selected)
        {
            return;
        }

        selectedFrameStates[instanceId] = selected;

        CanvasGroup canvasGroup = EnsureCanvasGroup(selectedFrame);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        Transform targetTransform = selectedFrame.transform;
        Vector3 originalScale = GetOriginalScale(targetTransform);
        DOTween.Kill(selectedFrame);
        targetTransform.DOKill();
        canvasGroup.DOKill();

        if (selected)
        {
            PlaySelectionChangedSound(instanceId);
            selectedFrame.SetActive(true);
            targetTransform.localScale = originalScale * DeselectedScale;
            canvasGroup.alpha = 0f;

            DOTween.Sequence()
                .SetTarget(selectedFrame)
                .SetLink(selectedFrame)
                .SetUpdate(useUnscaledTime)
                .Join(canvasGroup.DOFade(1f, FrameFadeDuration))
                .Join(targetTransform.DOScale(originalScale * SelectedScale, FrameFadeDuration).SetEase(Ease.OutBack));
            return;
        }

        DOTween.Sequence()
            .SetTarget(selectedFrame)
            .SetLink(selectedFrame)
            .SetUpdate(useUnscaledTime)
            .Join(canvasGroup.DOFade(0f, FrameFadeDuration))
            .Join(targetTransform.DOScale(originalScale * DeselectedScale, FrameFadeDuration))
            .OnComplete(() =>
            {
                if (selectedFrame != null)
                {
                    selectedFrame.SetActive(false);
                    targetTransform.localScale = originalScale;
                }
            });
    }

    private static void PlaySelectionChangedSound(int selectedFrameId)
    {
        if (currentSelectedFrameId == 0)
        {
            currentSelectedFrameId = selectedFrameId;
            return;
        }

        if (currentSelectedFrameId == selectedFrameId)
        {
            return;
        }

        currentSelectedFrameId = selectedFrameId;
        UISoundPlayer.PlayNavigation();
    }

    public static void PunchScale(Transform targetTransform, float strength = 0.12f, bool useUnscaledTime = false)
    {
        if (targetTransform == null)
        {
            return;
        }

        targetTransform.DOKill();
        Vector3 originalScale = GetOriginalScale(targetTransform);
        targetTransform.localScale = originalScale;
        targetTransform
            .DOPunchScale(originalScale * strength, 0.22f, 8, 0.8f)
            .SetLink(targetTransform.gameObject)
            .SetUpdate(useUnscaledTime)
            .OnComplete(() =>
            {
                if (targetTransform != null)
                {
                    targetTransform.localScale = originalScale;
                }
            });
    }
}
