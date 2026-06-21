using DG.Tweening;
using UnityEngine;

public class TouchUI : MonoBehaviour
{
    [SerializeField] private float hideDuration = 0.2f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void Start()
    {
        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.onLanded += PlayerMovement_OnLanded;
        }
    }

    private void PlayerMovement_OnLanded(object sender, PlayerMovement.OnLandedEventArgs e)
    {
        HideControls();
    }

    private void HideControls()
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.DOKill();

        canvasGroup
            .DOFade(0f, hideDuration)
            .SetLink(gameObject);
    }

    private void OnDestroy()
    {
        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.onLanded -= PlayerMovement_OnLanded;
        }

        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
        }
    }
}
