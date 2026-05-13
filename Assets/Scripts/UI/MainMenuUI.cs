using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private SaveUI saveUI;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    [Header("Selected Frames")]
    [SerializeField] private GameObject playSelectedFrame;
    [SerializeField] private GameObject quitSelectedFrame;

    private void Awake()
    {
        Time.timeScale = 1f;

        playButton.onClick.AddListener(() =>
        {
            OnClickPlay();
        });

        quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }

    private void Start()
    {
        Show();

        playButton.Select();
    }

    private void Update()
    {
        UpdateSelectedFrame();
    }

    private void OnClickPlay()
    {
        if (SaveManager.HasSave())
        {
            Hide();

            saveUI.Show();
        }
        else
        {
            StartNewGame();
        }
    }

    private void StartNewGame()
    {
        GameManager.ResetStaticData();

        SaveManager.ResetProgress();

        SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
    }

    public void Show()
    {
        mainMenuUI.SetActive(true);

        playButton.Select();
    }

    public void Hide()
    {
        mainMenuUI.SetActive(false);
    }

    private void UpdateSelectedFrame()
    {
        GameObject selectedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        playSelectedFrame.SetActive(selectedObject == playButton.gameObject);
        quitSelectedFrame.SetActive(selectedObject == quitButton.gameObject);
    }
}