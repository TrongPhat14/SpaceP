using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KeyDoorSystem : MonoBehaviour
{
    private enum DoorSystemState
    {
        NeedKey,
        KeyCollected,
        Unlocking,
        Opened
    }

    [SerializeField] private KeyPickup keyPickup;
    [SerializeField] private LockTerminal lockTerminal;
    [SerializeField] private DoorLock doorLock;
    [SerializeField] private string requiredKeyId = "default";
    [SerializeField] private Transform lockEffectRoot;
    [SerializeField] private float lockEffectHideDuration = 0.25f;
    [SerializeField] private SpriteRenderer unlockVisualRenderer;
    [SerializeField] private float unlockVisualScale = 1.2f;
    [SerializeField] private float unlockVisualDuration = 0.45f;
    [SerializeField] private int unlockVisualSortingOrder = 30;

    private DoorSystemState state = DoorSystemState.NeedKey;
    private Vector3 lockEffectOriginalScale;
    private Vector3 unlockVisualOriginalScale = Vector3.one;
    private Color unlockVisualOriginalColor = Color.white;

    private static readonly HashSet<string> CollectedKeyIds = new HashSet<string>();
    private static int activeSceneHandle;

    public string RequiredKeyId => NormalizeKeyId(requiredKeyId);
    public bool HasKey => state == DoorSystemState.Opened || CollectedKeyIds.Contains(RequiredKeyId);
    public bool IsOpened => state == DoorSystemState.Opened;
    public bool CanUnlock => HasKey && state != DoorSystemState.Opened;

    private void Awake()
    {
        EnsureSceneKeyCache();

        if (keyPickup == null)
        {
            keyPickup = GetComponentInChildren<KeyPickup>();
        }

        if (lockTerminal == null)
        {
            lockTerminal = GetComponentInChildren<LockTerminal>();
        }

        if (doorLock == null)
        {
            doorLock = GetComponentInChildren<DoorLock>();
        }

        if (lockEffectRoot == null && lockTerminal != null)
        {
            lockEffectRoot = lockTerminal.transform;
        }

        if (lockEffectRoot != null)
        {
            lockEffectRoot.gameObject.SetActive(true);
            lockEffectOriginalScale = lockEffectRoot.localScale;
        }

        if (unlockVisualRenderer == null)
        {
            Transform unlockVisualTransform = transform.Find("UnLockVisual");
            if (unlockVisualTransform != null)
            {
                unlockVisualRenderer = unlockVisualTransform.GetComponent<SpriteRenderer>();
            }
        }

        if (unlockVisualRenderer != null)
        {
            MoveUnlockVisualOutsideLockEffect();
            unlockVisualOriginalScale = unlockVisualRenderer.transform.localScale;
            unlockVisualOriginalColor = unlockVisualRenderer.color;
            unlockVisualRenderer.sortingOrder = unlockVisualSortingOrder;
            unlockVisualRenderer.gameObject.SetActive(false);
        }

        if (keyPickup != null)
        {
            keyPickup.Initialize(this);
        }

        if (lockTerminal != null)
        {
            lockTerminal.Initialize(this);
        }
    }

    public static bool CollectKey(string keyId)
    {
        EnsureSceneKeyCache();
        CollectedKeyIds.Add(NormalizeKeyId(keyId));
        return true;
    }

    public void PlayKeyCollectedFeedback()
    {
        if (lockEffectRoot != null)
        {
            lockEffectRoot.DOKill();
            lockEffectRoot.localScale = lockEffectOriginalScale;
            lockEffectRoot.DOPunchScale(Vector3.one * 0.12f, 0.22f, 6, 0.8f)
                .SetLink(lockEffectRoot.gameObject);
        }
    }

    public void NotifyLockedAttempt()
    {
        if (lockEffectRoot == null || state != DoorSystemState.NeedKey)
        {
            return;
        }

        lockEffectRoot.DOKill();
        lockEffectRoot.localScale = lockEffectOriginalScale;
        lockEffectRoot.DOShakePosition(0.18f, 0.08f, 10, 90f, false, true)
            .SetLink(lockEffectRoot.gameObject);
    }

    public void CompleteUnlock()
    {
        if (!HasKey || state == DoorSystemState.Opened)
        {
            return;
        }

        state = DoorSystemState.Opened;
        HideLockEffect(() => PlayUnlockVisualAnimation(OpenDoor));
    }

    public void SetUnlocking(bool isUnlocking)
    {
        if (state == DoorSystemState.Opened)
        {
            return;
        }

        if (isUnlocking && HasKey)
        {
            state = DoorSystemState.Unlocking;
        }
        else if (!isUnlocking && state == DoorSystemState.Unlocking)
        {
            state = HasKey ? DoorSystemState.KeyCollected : DoorSystemState.NeedKey;
        }
    }

    private void HideLockEffect(Action onComplete)
    {
        if (lockEffectRoot == null)
        {
            onComplete?.Invoke();
            return;
        }

        lockEffectRoot.DOKill();
        lockEffectRoot
            .DOScale(Vector3.zero, Mathf.Max(0.01f, lockEffectHideDuration))
            .SetEase(Ease.InBack)
            .SetLink(lockEffectRoot.gameObject)
            .OnComplete(() =>
            {
                lockEffectRoot.gameObject.SetActive(false);
                onComplete?.Invoke();
            });
    }

    private void PlayUnlockVisualAnimation(Action onComplete)
    {
        if (unlockVisualRenderer == null)
        {
            ReleaseLog.Warning("KeyDoorSystem cannot show unlock visual because Unlock Visual Renderer is missing.", this);
            onComplete?.Invoke();
            return;
        }

        SpriteRenderer visualRenderer = unlockVisualRenderer;

        visualRenderer.DOKill();
        visualRenderer.transform.DOKill();
        visualRenderer.transform.rotation = Quaternion.identity;
        visualRenderer.transform.localScale = Vector3.zero;
        visualRenderer.color = WithAlpha(unlockVisualOriginalColor, 0f);
        visualRenderer.sortingOrder = unlockVisualSortingOrder;
        visualRenderer.gameObject.SetActive(true);

        Sequence sequence = DOTween.Sequence()
            .SetLink(visualRenderer.gameObject);

        sequence.Join(visualRenderer.transform.DOScale(unlockVisualOriginalScale * unlockVisualScale, 0.18f).SetEase(Ease.OutBack));
        sequence.Join(visualRenderer.DOFade(unlockVisualOriginalColor.a, 0.12f));
        sequence.Append(visualRenderer.transform.DOPunchRotation(new Vector3(0f, 0f, -12f), 0.18f, 6, 0.8f));
        sequence.AppendInterval(Mathf.Max(0f, unlockVisualDuration - 0.32f));
        sequence.Append(visualRenderer.DOFade(0f, 0.12f));
        sequence.Join(visualRenderer.transform.DOScale(unlockVisualOriginalScale * (unlockVisualScale * 0.85f), 0.12f).SetEase(Ease.InQuad));
        sequence.OnComplete(() =>
        {
            visualRenderer.gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    private void MoveUnlockVisualOutsideLockEffect()
    {
        if (unlockVisualRenderer == null || lockEffectRoot == null)
        {
            return;
        }

        Transform visualTransform = unlockVisualRenderer.transform;
        if (visualTransform == lockEffectRoot || !visualTransform.IsChildOf(lockEffectRoot))
        {
            return;
        }

        visualTransform.SetParent(transform, true);
    }

    private void OpenDoor()
    {
        if (doorLock != null)
        {
            doorLock.Open();
        }
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    private static void EnsureSceneKeyCache()
    {
        int currentSceneHandle = SceneManager.GetActiveScene().handle;
        if (activeSceneHandle == currentSceneHandle)
        {
            return;
        }

        activeSceneHandle = currentSceneHandle;
        CollectedKeyIds.Clear();
    }

    private static string NormalizeKeyId(string keyId)
    {
        return string.IsNullOrWhiteSpace(keyId) ? "default" : keyId.Trim().ToLowerInvariant();
    }
}
