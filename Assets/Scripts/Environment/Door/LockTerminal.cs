using DG.Tweening;
using UnityEngine;

public class LockTerminal : MonoBehaviour
{
    [SerializeField] private KeyDoorSystem keyDoorSystem;
    [SerializeField] private Collider2D triggerCollider;
    [SerializeField] private Transform progressVisual;
    [SerializeField] private SpriteRenderer progressSprite;
    [SerializeField] private Transform chargingEffectVisual;
    [SerializeField] private SpriteRenderer chargingEffectSprite;
    [SerializeField] private float unlockHoldDuration = 2f;
    [SerializeField] private float activePulseScale = 0.08f;
    [SerializeField] private float progressStartScale = 0.35f;
    [SerializeField] private float progressEndScale = 1.15f;
    [SerializeField] private float chargingRotationSpeed = 180f;
    [SerializeField] private Color progressStartColor = new Color(1f, 1f, 1f, 0.15f);
    [SerializeField] private Color progressEndColor = new Color(1f, 0.9f, 0.15f, 0.95f);

    private bool playerInside;
    private float unlockTimer;
    private Vector3 progressOriginalScale;
    private Vector3 chargingEffectOriginalScale;
    private Color chargingEffectOriginalColor;
    private Tween pulseTween;

    private void Awake()
    {
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider2D>();
        }

        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }

        if (progressVisual == null)
        {
            progressVisual = transform;
        }

        if (progressSprite == null && progressVisual != null)
        {
            progressSprite = progressVisual.GetComponent<SpriteRenderer>();
        }

        if (chargingEffectVisual == null)
        {
            Transform parent = transform.parent;
            if (parent != null)
            {
                chargingEffectVisual = parent.Find("CircleEffect");
            }
        }

        if (chargingEffectSprite == null && chargingEffectVisual != null)
        {
            chargingEffectSprite = chargingEffectVisual.GetComponent<SpriteRenderer>();
        }

        progressOriginalScale = progressVisual.localScale;
        if (chargingEffectVisual != null)
        {
            chargingEffectOriginalScale = chargingEffectVisual.localScale;
        }

        if (chargingEffectSprite != null)
        {
            chargingEffectOriginalColor = chargingEffectSprite.color;
        }

        ResetUnlockProgress();
    }

    private void Update()
    {
        if (!playerInside || keyDoorSystem == null || keyDoorSystem.IsOpened)
        {
            return;
        }

        if (!keyDoorSystem.HasKey)
        {
            ResetUnlockProgress();
            return;
        }

        keyDoorSystem.SetUnlocking(true);
        unlockTimer += Time.deltaTime;
        RotateChargingEffect();
        UpdateProgressVisual();

        if (unlockTimer >= unlockHoldDuration)
        {
            playerInside = false;
            StopPulse();
            keyDoorSystem.CompleteUnlock();
        }
    }

    public void Initialize(KeyDoorSystem keyDoorSystem)
    {
        this.keyDoorSystem = keyDoorSystem;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent(out PlayerMovement _))
        {
            return;
        }

        playerInside = true;

        if (keyDoorSystem == null)
        {
            keyDoorSystem = GetComponentInParent<KeyDoorSystem>();
        }

        if (keyDoorSystem == null)
        {
            return;
        }

        if (!keyDoorSystem.HasKey)
        {
            keyDoorSystem.NotifyLockedAttempt();
            return;
        }

        StartPulse();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.TryGetComponent(out PlayerMovement _))
        {
            return;
        }

        playerInside = false;
        StopPulse();

        if (keyDoorSystem != null)
        {
            keyDoorSystem.SetUnlocking(false);
        }

        ResetUnlockProgress();
    }

    private void ResetUnlockProgress()
    {
        unlockTimer = 0f;

        if (progressVisual != null)
        {
            progressVisual.localScale = progressOriginalScale;
        }

        if (progressSprite != null)
        {
            progressSprite.color = WithAlpha(progressStartColor, 0f);
            progressSprite.enabled = false;
        }

        if (chargingEffectVisual != null)
        {
            chargingEffectVisual.localScale = chargingEffectOriginalScale;
        }

        if (chargingEffectSprite != null)
        {
            chargingEffectSprite.color = chargingEffectOriginalColor;
        }
    }

    private void UpdateProgressVisual()
    {
        if (progressVisual == null)
        {
            return;
        }

        float progress = unlockHoldDuration <= 0f ? 1f : Mathf.Clamp01(unlockTimer / unlockHoldDuration);

        if (progressSprite != null)
        {
            progressSprite.enabled = true;
            progressSprite.color = Color.Lerp(progressStartColor, progressEndColor, progress);
        }

        float scale = Mathf.Lerp(progressStartScale, progressEndScale, progress);
        progressVisual.localScale = progressOriginalScale * scale;

        if (progress >= 1f)
        {
            progressVisual.DOKill();
            progressVisual.DOPunchScale(Vector3.one * 0.18f, 0.18f, 6, 0.8f)
                .SetLink(progressVisual.gameObject);
        }
    }

    private void OnValidate()
    {
        unlockHoldDuration = Mathf.Max(0.01f, unlockHoldDuration);
        activePulseScale = Mathf.Max(0f, activePulseScale);
        progressStartScale = Mathf.Max(0.01f, progressStartScale);
        progressEndScale = Mathf.Max(progressStartScale, progressEndScale);
    }

    private void StartPulse()
    {
        Transform pulseTarget = chargingEffectVisual != null ? chargingEffectVisual : progressVisual;
        if (pulseTarget == null || pulseTween != null)
        {
            return;
        }

        Vector3 originalScale = chargingEffectVisual != null ? chargingEffectOriginalScale : progressOriginalScale;
        pulseTween = pulseTarget
            .DOScale(originalScale * (1f + activePulseScale), 0.28f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(pulseTarget.gameObject);
    }

    private void StopPulse()
    {
        pulseTween?.Kill();
        pulseTween = null;
    }

    private void RotateChargingEffect()
    {
        if (chargingEffectVisual == null || Mathf.Approximately(chargingRotationSpeed, 0f))
        {
            return;
        }

        chargingEffectVisual.Rotate(0f, 0f, -chargingRotationSpeed * Time.deltaTime, Space.Self);
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }
}
