using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    private const float FadeDuration = 0.06f;
    private const int SortingOrder = 9999;

    private static SceneTransition instance;
    private static bool isTransitioning;

    private CanvasGroup canvasGroup;

    public static void LoadScene(string sceneName)
    {
        EnsureInstance();

        if (instance == null)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        instance.LoadSceneWithFade(sceneName);
    }

    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        GameObject transitionObject = new GameObject("Scene Transition");
        instance = transitionObject.AddComponent<SceneTransition>();
        DontDestroyOnLoad(transitionObject);
        instance.BuildOverlay();
    }

    private void BuildOverlay()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = SortingOrder;

        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        GameObject imageObject = new GameObject("Fade Image");
        imageObject.transform.SetParent(transform, false);

        Image fadeImage = imageObject.AddComponent<Image>();
        fadeImage.color = Color.black;

        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private void LoadSceneWithFade(string sceneName)
    {
        if (isTransitioning)
        {
            return;
        }

        isTransitioning = true;
        canvasGroup.DOKill();
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        canvasGroup
            .DOFade(1f, FadeDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                SceneManager.LoadScene(sceneName);
                FadeIn();
            });
    }

    private void FadeIn()
    {
        canvasGroup
            .DOFade(0f, FadeDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
                isTransitioning = false;
            });
    }
}
