using UnityEngine;
using Firebase.Analytics;

public static class AnalyticsManager
{
    public static void LogGameComplete(int totalScore, int levelNumber)
    {
        if (FirebaseManager.IsReady)
        {
            FirebaseAnalytics.LogEvent(
                "game_complete",
                new Parameter("total_score", totalScore),
                new Parameter("level_number", levelNumber)
            );
        }

        Debug.Log($"Analytics game_complete total_score={totalScore} level_number={levelNumber}");
    }

    public static void LogLeaderboardSubmit(bool success, int totalScore)
    {
        if (FirebaseManager.IsReady)
        {
            FirebaseAnalytics.LogEvent(
                "leaderboard_submit",
                new Parameter("success", success ? 1 : 0),
                new Parameter("total_score", totalScore)
            );
        }

        Debug.Log($"Analytics leaderboard_submit success={success} total_score={totalScore}");
    }
}
