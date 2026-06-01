using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PausedUI : MonoBehaviour
{
    [SerializeField] private Button resumeButon;
    [SerializeField] private Button mainMenuButon;
    [SerializeField] private Button soundVolumeButon;
    [SerializeField] private TextMeshProUGUI soundVolumeTextMesh;
    [SerializeField] private Button musicVolumeButon;
    [SerializeField] private TextMeshProUGUI musicVolumeTextMesh;

    [Header("Selected Frames")]
    [SerializeField] private GameObject resumeSelectedFrame;
    [SerializeField] private GameObject mainMenuSelectedFrame;
    [SerializeField] private GameObject soundVolumeSelectedFrame;
    [SerializeField] private GameObject musicVolumeSelectedFrame;

    private void Awake()
    {
        soundVolumeButon.onClick.AddListener(() =>
        {
            SoundManager.Instance.ChangeSoundVolume();
            soundVolumeTextMesh.text = "SOUND " + SoundManager.Instance.GetSoundVolume();
        });
        musicVolumeButon.onClick.AddListener(() =>
        {
            MusicManager.Instance.ChangeMusicVolume();
            musicVolumeTextMesh.text = "MUSIC " + MusicManager.Instance.GetMusicVolume();
        });
        resumeButon.onClick.AddListener(() => {
            GameManager.Instance.UnPauseGame();
        });
        mainMenuButon.onClick.AddListener(() =>
        {
            SceneLoader.LoadScene(SceneLoader.Scene.MainMenuScreen); 
        });
    }

    private void Start()
    {

        GameManager.Instance.OnGamePaused += GameManager_OnGamePaused;
        GameManager.Instance.OnGameUnPaused += GameManager_OnGameUnPaused;

        musicVolumeTextMesh.text = "MUSIC " + MusicManager.Instance.GetMusicVolume();
        soundVolumeTextMesh.text = "SOUND " + SoundManager.Instance.GetSoundVolume();
        DOTweenUIAnimator.HidePanelImmediate(gameObject);
    }

    private void Update()
    {
        UpdateSelectedFrame();
    }

    private void GameManager_OnGameUnPaused(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void GameManager_OnGamePaused(object sender, System.EventArgs e)
    {
        Show();
    }

    private void Show()
    {
        DOTweenUIAnimator.ShowPanel(gameObject, true);
        resumeButon.Select();
        UpdateSelectedFrame();

    }
    private void Hide()
    {
        DOTweenUIAnimator.HidePanel(gameObject, true);
    }

    private void UpdateSelectedFrame()
    {
        if (!gameObject.activeSelf || UnityEngine.EventSystems.EventSystem.current == null)
        {
            return;
        }

        GameObject selectedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        SetSelectedFrame(resumeSelectedFrame, selectedObject == resumeButon.gameObject);
        SetSelectedFrame(mainMenuSelectedFrame, selectedObject == mainMenuButon.gameObject);
        SetSelectedFrame(soundVolumeSelectedFrame, selectedObject == soundVolumeButon.gameObject);
        SetSelectedFrame(musicVolumeSelectedFrame, selectedObject == musicVolumeButon.gameObject);
    }

    private void SetSelectedFrame(GameObject selectedFrame, bool active)
    {
        if (selectedFrame != null)
        {
            DOTweenUIAnimator.SetSelectedFrame(selectedFrame, active, true);
        }
    }
}
