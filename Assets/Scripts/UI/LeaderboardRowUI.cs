using TMPro;
using UnityEngine;

public class LeaderboardRowUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject highlightObject;

    public void SetEntry(int rank, LeaderboardEntry entry, string currentPlayerName)
    {
        gameObject.SetActive(true);

        if (rankText != null)
        {
            rankText.text = rank.ToString();
        }

        if (playerNameText != null)
        {
            playerNameText.text = entry.playerName;
        }

        if (scoreText != null)
        {
            scoreText.text = entry.score.ToString();
        }

        if (highlightObject != null)
        {
            highlightObject.SetActive(entry.playerName == currentPlayerName);
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
