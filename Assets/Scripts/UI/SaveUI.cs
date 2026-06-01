using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject saveUI;
    [SerializeField] private MainMenuUI mainMenuUI;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI continueButtonText;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button backButton;

    [Header("Selected Frames")]
    [SerializeField] private GameObject continueSelectedFrame;
    [SerializeField] private GameObject newGameSelectedFrame;
    [SerializeField] private GameObject backSelectedFrame;

    private bool saveIsCompleted;
    private bool completedScoreSubmitted;

    private void Awake()
    {
        continueButton.onClick.AddListener(() =>
        {
            OnClickContinue();
        });

        newGameButton.onClick.AddListener(() =>
        {
            OnClickNewGame();
        });

        backButton.onClick.AddListener(() =>
        {
            OnClickBack();
        });
    }

    private void Start()
    {
        DOTweenUIAnimator.HidePanelImmediate(saveUI);
    }

    private void Update()
    {
        UpdateSelectedFrame();
    }

    public void Show()
    {
        DOTweenUIAnimator.ShowPanel(saveUI);

        UpdateSaveText();

        continueButton.Select();
    }

    public void Hide()
    {
        DOTweenUIAnimator.HidePanel(saveUI);
    }

    private void UpdateSaveText()
    {
        SaveData saveData = SaveManager.LoadProgress();
        saveIsCompleted = saveData.isGameCompleted;
        completedScoreSubmitted = LeaderboardManager.HasSubmittedCompletedGameScore(saveData);

        if (saveIsCompleted)
        {
            levelText.text = "COMPLETED";
        }
        else
        {
            levelText.text = saveData.levelNumber.ToString();
        }

        totalScoreText.text = saveData.totalScore.ToString();

        continueButton.interactable = true;

        if (continueButtonText != null)
        {
            if (!saveIsCompleted)
            {
                continueButtonText.text = "CONTINUE";
            }
            else if (completedScoreSubmitted)
            {
                continueButtonText.text = "LEADERBOARD";
            }
            else
            {
                continueButtonText.text = "SUBMIT SCORE";
            }
        }
    }
    private void OnClickContinue()
    {
        if (saveIsCompleted)
        {
            SceneLoader.LoadScene(
                completedScoreSubmitted
                    ? SceneLoader.Scene.LeaderboardScreen
                    : SceneLoader.Scene.GameOverScreen
            );
            return;
        }

        SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
    }

    private void OnClickNewGame()
    {
        GameManager.ResetStaticData();

        SaveManager.ResetProgress();
        LeaderboardManager.ClearSubmittedCompletedScore();

        SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
    }

    private void OnClickBack()
    {
        Hide();

        mainMenuUI.Show();
    }

    private void UpdateSelectedFrame()
    {
        if (saveUI == null || !saveUI.activeSelf || UnityEngine.EventSystems.EventSystem.current == null)
        {
            return;
        }

        GameObject selectedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        SetSelectedFrame(continueSelectedFrame, continueButton != null && selectedObject == continueButton.gameObject);
        SetSelectedFrame(newGameSelectedFrame, newGameButton != null && selectedObject == newGameButton.gameObject);
        SetSelectedFrame(backSelectedFrame, backButton != null && selectedObject == backButton.gameObject);
    }

    private void SetSelectedFrame(GameObject selectedFrame, bool active)
    {
        if (selectedFrame != null)
        {
            DOTweenUIAnimator.SetSelectedFrame(selectedFrame, active);
        }
    }
}
