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
    [SerializeField] private int leaderboardLimit = 10;

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
        DOTweenUIAnimator.PunchScale(refreshButton != null ? refreshButton.transform : null);
        SetStatus("Loading leaderboard...");
        SetRowsVisible(false);

        LeaderboardManager.GetTopEntriesOnline(leaderboardLimit, (entries, message) =>
        {
            bool hasEntries = PopulateRows(entries);

            if (hasEntries)
            {
                SetStatus(message);
            }
        });
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
