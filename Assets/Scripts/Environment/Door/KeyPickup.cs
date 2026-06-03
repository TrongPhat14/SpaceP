using DG.Tweening;
using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [SerializeField] private string keyId = "default";
    [SerializeField] private KeyDoorSystem keyDoorSystem;
    [SerializeField] private Collider2D pickupCollider;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float pickupDuration = 0.28f;

    private bool isCollected;

    private void Awake()
    {
        if (pickupCollider == null)
        {
            pickupCollider = GetComponent<Collider2D>();
        }

        if (visualRoot == null)
        {
            visualRoot = transform;
        }
    }

    public void Initialize(KeyDoorSystem keyDoorSystem)
    {
        this.keyDoorSystem = keyDoorSystem;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCollected || !collision.TryGetComponent(out PlayerMovement _))
        {
            return;
        }

        KeyDoorSystem system = keyDoorSystem != null ? keyDoorSystem : GetComponentInParent<KeyDoorSystem>();
        string collectedKeyId = string.IsNullOrWhiteSpace(keyId) && system != null ? system.RequiredKeyId : keyId;

        if (!KeyDoorSystem.CollectKey(collectedKeyId))
        {
            return;
        }

        if (system != null)
        {
            system.PlayKeyCollectedFeedback();
        }

        isCollected = true;

        if (pickupCollider != null)
        {
            pickupCollider.enabled = false;
        }

        visualRoot.DOKill();
        Sequence sequence = DOTween.Sequence().SetLink(gameObject);
        sequence.Join(visualRoot.DOScale(Vector3.zero, pickupDuration).SetEase(Ease.InBack));
        sequence.Join(visualRoot.DORotate(new Vector3(0f, 0f, 360f), pickupDuration, RotateMode.FastBeyond360));
        sequence.OnComplete(() => gameObject.SetActive(false));
    }
}
