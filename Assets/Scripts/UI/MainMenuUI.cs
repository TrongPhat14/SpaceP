using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private SaveUI saveUI;

    [Header("Buttons")]
    [SerializeField] private Button playButton;

    [SerializeField] private Button storeButton;

    [SerializeField] private Button leaderboardButton;

    [SerializeField] private Button quitButton;

    [Header("Selected Frames")]
    [SerializeField] private GameObject playSelectedFrame;

    [SerializeField] private GameObject storeSelectedFrame;

    [SerializeField] private GameObject leaderBoardSelectedFrame;

    [SerializeField] private GameObject quitSelectedFrame;

    private void Awake()
    {
        Time.timeScale = 1f;

        playButton.onClick.AddListener(() =>
        {
            OnClickPlay();
        });

        storeButton.onClick.AddListener(() =>
        {
            SceneLoader.LoadScene(SceneLoader.Scene.StoreScreen);
        });

        if (leaderboardButton != null)
        {
            leaderboardButton.onClick.AddListener(() =>
            {
                SceneLoader.LoadScene(SceneLoader.Scene.LeaderboardScreen);
            });
        }

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
        LeaderboardManager.ClearSubmittedCompletedScore();

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

        storeSelectedFrame.SetActive(selectedObject == storeButton.gameObject);

        if (leaderboardButton != null && leaderBoardSelectedFrame != null)
        {
            leaderBoardSelectedFrame.SetActive(selectedObject == leaderboardButton.gameObject);
        }

        quitSelectedFrame.SetActive(selectedObject == quitButton.gameObject);
    }
}
