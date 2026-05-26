using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private TextMeshProUGUI scoreTextMesh;

    [Header("Leaderboard - Optional")]
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private Button submitScoreButton;
    [SerializeField] private TextMeshProUGUI globalRankValueText;

    [Header("Submit State UI - Optional")]
    [SerializeField] private GameObject readyStatusObject;
    [SerializeField] private GameObject submittingStatusObject;
    [SerializeField] private GameObject submittedStatusObject;
    [SerializeField] private GameObject submitErrorStatusObject;

    [Header("Selected Frames - Optional")]
    [SerializeField] private GameObject leaderboardSelectedFrame;
    [SerializeField] private GameObject mainMenuSelectedFrame;

    private SaveData saveData;
    private bool scoreSubmitted;
    private bool isSubmitting;

    private void Awake()
    {
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(() =>
            {
                SceneLoader.LoadScene(SceneLoader.Scene.MainMenuScreen);
            });
        }

        if (submitScoreButton != null)
        {
            submitScoreButton.onClick.AddListener(OnSubmitScoreButtonClicked);
        }

        if (playerNameInputField != null)
        {
            playerNameInputField.onValueChanged.AddListener(_ => UpdateSubmitAvailability());
        }

        if (leaderboardButton != null)
        {
            leaderboardButton.onClick.AddListener(() =>
            {
                SceneLoader.LoadScene(SceneLoader.Scene.LeaderboardScreen);
            });
        }
    }

    private void Start()
    {
        saveData = SaveManager.LoadProgress();

        if (scoreTextMesh != null)
        {
            scoreTextMesh.text = saveData.totalScore.ToString();
        }

        SetupLeaderboardUI();
        SelectInitialControl();
    }

    private void Update()
    {
        UpdateSelectedFrame();
    }

    private void SetupLeaderboardUI()
    {
        bool canSubmitScore = saveData != null && saveData.isGameCompleted;
        bool alreadySubmitted = LeaderboardManager.HasSubmittedCompletedGameScore(saveData);

        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(canSubmitScore);
        }

        if (playerNameInputField != null)
        {
            playerNameInputField.gameObject.SetActive(canSubmitScore);
            playerNameInputField.text = string.Empty;
            playerNameInputField.interactable = canSubmitScore && !alreadySubmitted;
        }

        if (submitScoreButton != null)
        {
            submitScoreButton.gameObject.SetActive(canSubmitScore);
        }

        if (globalRankValueText != null)
        {
            globalRankValueText.text = alreadySubmitted ? "SUBMITTED" : "ENTER NAME";
        }

        if (canSubmitScore)
        {
            FirebaseManager.Initialize((isReady, message) =>
            {
                if (isReady)
                {
                    AnalyticsManager.LogGameComplete(saveData.totalScore, saveData.levelNumber);
                }
            });
        }

        UpdateSubmitAvailability();
    }

    private void OnSubmitScoreButtonClicked()
    {
        if (scoreSubmitted || isSubmitting)
        {
            return;
        }

        string playerName = playerNameInputField != null
            ? playerNameInputField.text
            : LeaderboardManager.GetSavedPlayerName();

        if (!HasPlayerName(playerName))
        {
            SetSubmitStatus(SubmitStatus.Error);

            if (globalRankValueText != null)
            {
                globalRankValueText.text = "ENTER NAME";
            }

            return;
        }

        isSubmitting = true;
        SetSubmitInteractable(false);
        SetSubmitStatus(SubmitStatus.Submitting);

        if (globalRankValueText != null)
        {
            globalRankValueText.text = "SYNCING";
        }

        LeaderboardManager.SubmitCompletedGameScore(playerName, saveData, (success, message) =>
        {
            isSubmitting = false;
            scoreSubmitted = success;

            if (!success)
            {
                SetSubmitStatus(SubmitStatus.Error);
                SetSubmitInteractable(HasPlayerName(playerName));
                SetPlayerNameInteractable(true);

                if (globalRankValueText != null)
                {
                    if (message == "Player name is already taken.")
                    {
                        globalRankValueText.text = "NAME USED";
                    }
                    else if (message == "Could not verify player name.")
                    {
                        globalRankValueText.text = "CHECK FAIL";
                    }
                    else
                    {
                        globalRankValueText.text = "PENDING";
                    }
                }

                return;
            }

            SetSubmitStatus(SubmitStatus.Submitted);
            SetPlayerNameInteractable(false);
            SelectLeaderboardButton();
            UpdateGlobalRank(playerName);
        });
    }

    private void UpdateSubmitAvailability()
    {
        bool canSubmitScore = saveData != null && saveData.isGameCompleted;
        bool alreadySubmitted = LeaderboardManager.HasSubmittedCompletedGameScore(saveData);
        bool hasPlayerName = playerNameInputField != null && HasPlayerName(playerNameInputField.text);

        if (isSubmitting)
        {
            SetSubmitInteractable(false);
            SetPlayerNameInteractable(false);
            SetSubmitStatus(SubmitStatus.Submitting);
            return;
        }

        SetSubmitInteractable(canSubmitScore && !alreadySubmitted && hasPlayerName && !scoreSubmitted);
        SetPlayerNameInteractable(canSubmitScore && !alreadySubmitted && !scoreSubmitted);

        if (alreadySubmitted || scoreSubmitted)
        {
            SetSubmitStatus(SubmitStatus.Submitted);

            if (globalRankValueText != null)
            {
                globalRankValueText.text = "SUBMITTED";
            }

            return;
        }

        if (!canSubmitScore || !hasPlayerName)
        {
            SetSubmitStatus(SubmitStatus.Hidden);

            if (globalRankValueText != null)
            {
                globalRankValueText.text = "ENTER NAME";
            }

            return;
        }

        SetSubmitStatus(SubmitStatus.Ready);

        if (globalRankValueText != null)
        {
            globalRankValueText.text = "PENDING";
        }
    }

    private void UpdateGlobalRank(string playerName)
    {
        if (globalRankValueText == null || saveData == null)
        {
            return;
        }

        globalRankValueText.text = "SYNCED";

        LeaderboardManager.GetPlayerRankOnline(playerName, saveData.totalScore, (rank, message) =>
        {
            globalRankValueText.text = rank > 0 ? $"#{rank}" : "SYNCED";
        });
    }

    private void SetSubmitInteractable(bool interactable)
    {
        if (submitScoreButton != null)
        {
            submitScoreButton.interactable = interactable;
        }
    }

    private void SetPlayerNameInteractable(bool interactable)
    {
        if (playerNameInputField != null)
        {
            playerNameInputField.interactable = interactable;
        }
    }

    private void SelectInitialControl()
    {
        if (playerNameInputField != null && playerNameInputField.interactable)
        {
            playerNameInputField.Select();
            return;
        }

        if (leaderboardButton != null)
        {
            leaderboardButton.Select();
            UpdateSelectedFrame();
            return;
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.Select();
            UpdateSelectedFrame();
        }
    }

    private void SelectLeaderboardButton()
    {
        if (leaderboardButton != null)
        {
            leaderboardButton.Select();
            UpdateSelectedFrame();
        }
    }

    private void UpdateSelectedFrame()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            return;
        }

        GameObject selectedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        SetActive(leaderboardSelectedFrame, leaderboardButton != null && selectedObject == leaderboardButton.gameObject);
        SetActive(mainMenuSelectedFrame, mainMenuButton != null && selectedObject == mainMenuButton.gameObject);
    }

    private bool HasPlayerName(string playerName)
    {
        return !string.IsNullOrWhiteSpace(playerName);
    }

    private void SetSubmitStatus(SubmitStatus submitStatus)
    {
        SetActive(readyStatusObject, submitStatus == SubmitStatus.Ready);
        SetActive(submittingStatusObject, submitStatus == SubmitStatus.Submitting);
        SetActive(submittedStatusObject, submitStatus == SubmitStatus.Submitted);
        SetActive(submitErrorStatusObject, submitStatus == SubmitStatus.Error);
    }

    private void SetActive(GameObject targetObject, bool active)
    {
        if (targetObject != null)
        {
            targetObject.SetActive(active);
        }
    }

    private enum SubmitStatus
    {
        Hidden,
        Ready,
        Submitting,
        Submitted,
        Error,
    }
}
