using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StartupIntroVideoPlayer : MonoBehaviour
{
    private const string OpeningVideoResourcePath = "Videos/Opening";
    private const int CanvasSortOrder = 5000;

    private static bool hasPlayedThisSession;

    private Canvas canvas;
    private RawImage videoImage;
    private VideoPlayer videoPlayer;
    private AudioSource audioSource;
    private RenderTexture renderTexture;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void PlayAfterFirstSceneLoad()
    {
        if (hasPlayedThisSession)
        {
            return;
        }

        VideoClip openingVideo = Resources.Load<VideoClip>(OpeningVideoResourcePath);
        if (openingVideo == null)
        {
            ReleaseLog.Warning(
                "Opening video not found at Resources/" +
                OpeningVideoResourcePath +
                "."
            );
            hasPlayedThisSession = true;
            return;
        }

        hasPlayedThisSession = true;

        GameObject playerObject = new GameObject(nameof(StartupIntroVideoPlayer));
        StartupIntroVideoPlayer player =
            playerObject.AddComponent<StartupIntroVideoPlayer>();
        player.Initialize(openingVideo);
    }

    private void Initialize(VideoClip openingVideo)
    {
        DontDestroyOnLoad(gameObject);
        BuildOverlay();
        PlayVideo(openingVideo);
    }

    private void BuildOverlay()
    {
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = CanvasSortOrder;

        CanvasScaler canvasScaler = gameObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(transform, false);

        Image backgroundImage = backgroundObject.AddComponent<Image>();
        backgroundImage.color = Color.black;
        StretchToParent(backgroundImage.rectTransform);

        GameObject videoObject = new GameObject("Video");
        videoObject.transform.SetParent(transform, false);

        videoImage = videoObject.AddComponent<RawImage>();
        videoImage.color = Color.white;
        StretchToParent(videoImage.rectTransform);
    }

    private void PlayVideo(VideoClip openingVideo)
    {
        int textureWidth = Mathf.Max(Screen.width, 1280);
        int textureHeight = Mathf.Max(Screen.height, 720);

        renderTexture = new RenderTexture(textureWidth, textureHeight, 0)
        {
            name = "OpeningVideoRenderTexture"
        };
        renderTexture.Create();

        videoImage.texture = renderTexture;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = openingVideo;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);
        videoPlayer.isLooping = false;
        videoPlayer.skipOnDrop = true;
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.Prepare();
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        source.Play();
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        Close();
    }

    private void OnVideoError(VideoPlayer source, string message)
    {
        ReleaseLog.Warning("Opening video failed: " + message);
        Close();
    }

    private void Close()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.errorReceived -= OnVideoError;
            videoPlayer.Stop();
        }

        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
            renderTexture = null;
        }

        Destroy(gameObject);
    }

    private static void StretchToParent(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
