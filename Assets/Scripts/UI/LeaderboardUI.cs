using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    [Header("Navigation")]
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button backButton;

    [Header("Selected Frames")]
    [SerializeField] private GameObject refreshSelectedFrame;
    [SerializeField] private GameObject backSelectedFrame;

    [Header("Content")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private List<LeaderboardRowUI> rowUIList = new List<LeaderboardRowUI>();
    [SerializeField] private LeaderboardRowUI myRankRow;
    [SerializeField] private int leaderboardLimit = 10;

    private int loadRequestVersion;

    private void Awake()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(() =>
            {
                SceneLoader.LoadScene(SceneLoader.Scene.MainMenuScreen);
            });
        }

        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(LoadLeaderboard);
        }
    }

    private void Start()
    {
        LoadLeaderboard();

        if (refreshButton != null)
        {
            refreshButton.Select();
            UpdateSelectedFrame();
        }
    }

    private void Update()
    {
        UpdateSelectedFrame();
    }

    public void LoadLeaderboard()
    {
        int requestVersion = ++loadRequestVersion;
        DOTweenUIAnimator.PunchScale(refreshButton != null ? refreshButton.transform : null);
        SetStatus("Loading leaderboard...");
        SetRowsVisible(false);
        HideMyRankRow();

        LeaderboardManager.GetTopEntriesOnline(leaderboardLimit, (entries, message) =>
        {
            if (requestVersion != loadRequestVersion)
            {
                return;
            }

            bool hasEntries = PopulateRows(entries);

            if (hasEntries)
            {
                SetStatus(message);
            }

            LoadMyRank(entries, requestVersion);
        });
    }

    private void LoadMyRank(List<LeaderboardEntry> topEntries, int requestVersion)
    {
        if (myRankRow == null)
        {
            return;
        }

        SaveData saveData = SaveManager.LoadProgress();
        string playerName = LeaderboardManager.GetSavedPlayerName();

        if (!LeaderboardManager.HasSubmittedCompletedGameScore(saveData) ||
            string.IsNullOrWhiteSpace(playerName))
        {
            HideMyRankRow();
            return;
        }

        if (ContainsPlayer(topEntries, playerName, saveData.totalScore))
        {
            HideMyRankRow();
            return;
        }

        SetStatus("Loading your rank...");
        LeaderboardManager.GetPlayerRankOnline(playerName, saveData.totalScore, (rank, message) =>
        {
            if (requestVersion != loadRequestVersion)
            {
                return;
            }

            if (rank <= 0)
            {
                HideMyRankRow();
                SetStatus(message);
                return;
            }

            LeaderboardEntry playerEntry = new LeaderboardEntry(
                playerName,
                saveData.totalScore,
                saveData.levelNumber,
                saveData.isGameCompleted,
                0
            );

            myRankRow.SetEntry(rank, playerEntry, playerName);
            SetStatus(string.Empty);
        });
    }

    private static bool ContainsPlayer(
        List<LeaderboardEntry> entries,
        string playerName,
        int score
    )
    {
        if (entries == null)
        {
            return false;
        }

        foreach (LeaderboardEntry entry in entries)
        {
            if (entry != null && entry.playerName == playerName && entry.score == score)
            {
                return true;
            }
        }

        return false;
    }

    private void HideMyRankRow()
    {
        if (myRankRow != null)
        {
            myRankRow.Hide();
        }
    }

    private bool PopulateRows(List<LeaderboardEntry> entries)
    {
        SetRowsVisible(false);

        if (entries == null || entries.Count == 0)
        {
            SetStatus("No scores yet.");
            return false;
        }

        string currentPlayerName = LeaderboardManager.GetSavedPlayerName();
        int count = Mathf.Min(entries.Count, rowUIList.Count);

        for (int i = 0; i < count; i++)
        {
            rowUIList[i].SetEntry(i + 1, entries[i], currentPlayerName);
        }

        return count > 0;
    }

    private void SetRowsVisible(bool visible)
    {
        foreach (LeaderboardRowUI rowUI in rowUIList)
        {
            if (rowUI != null)
            {
                if (visible)
                {
                    rowUI.gameObject.SetActive(true);
                }
                else
                {
                    rowUI.Hide();
                }
            }
        }
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
            DOTweenUIAnimator.PunchScale(statusText.transform, 0.06f);
        }
    }

    private void UpdateSelectedFrame()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            return;
        }

        GameObject selectedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        if (refreshButton != null && refreshSelectedFrame != null)
        {
            DOTweenUIAnimator.SetSelectedFrame(refreshSelectedFrame, selectedObject == refreshButton.gameObject);
        }

        if (backButton != null && backSelectedFrame != null)
        {
            DOTweenUIAnimator.SetSelectedFrame(backSelectedFrame, selectedObject == backButton.gameObject);
        }
    }
}
