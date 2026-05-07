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
            GameManager.instance.UnPauseGame();
            SettingUI.Instance.Show();
        });
        mainMenuButon.onClick.AddListener(() =>
        {
            SceneLoader.LoadScene(SceneLoader.Scene.MainMenuScreen); 
        });
    }

    private void Start()
    {

        GameManager.instance.OnGamePaused += GameManager_OnGamePaused;
        GameManager.instance.OnGameUnPaused += GameManager_OnGameUnPaused;

        musicVolumeTextMesh.text = "MUSIC " + MusicManager.Instance.GetMusicVolume();
        soundVolumeTextMesh.text = "SOUND " + SoundManager.Instance.GetSoundVolume();
        Hide();
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
        gameObject.SetActive(true);
        resumeButon.Select();

    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
