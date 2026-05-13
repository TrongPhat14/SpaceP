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

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button backButton;

    [Header("Selected Frames")]
    [SerializeField] private GameObject continueSelectedFrame;
    [SerializeField] private GameObject newGameSelectedFrame;
    [SerializeField] private GameObject backSelectedFrame;

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
        Hide();
    }

    private void Update()
    {
        UpdateSelectedFrame();
    }

    public void Show()
    {
        saveUI.SetActive(true);

        UpdateSaveText();

        continueButton.Select();
    }

    public void Hide()
    {
        saveUI.SetActive(false);
    }

    private void UpdateSaveText()
    {
        SaveData saveData = SaveManager.LoadProgress();

        if (saveData.isGameCompleted)
        {
            levelText.text = "COMPLETED";
        }
        else
        {
            levelText.text = saveData.levelNumber.ToString();
        }

        totalScoreText.text = saveData.totalScore.ToString();
    }
    private void OnClickContinue()
    {
        SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
    }

    private void OnClickNewGame()
    {
        GameManager.ResetStaticData();

        SaveManager.ResetProgress();

        SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
    }

    private void OnClickBack()
    {
        Hide();

        mainMenuUI.Show();
    }

    private void UpdateSelectedFrame()
    {
        if (!saveUI.activeSelf)
        {
            return;
        }

        GameObject selectedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        continueSelectedFrame.SetActive(selectedObject == continueButton.gameObject);
        newGameSelectedFrame.SetActive(selectedObject == newGameButton.gameObject);
        backSelectedFrame.SetActive(selectedObject == backButton.gameObject);
    }
}