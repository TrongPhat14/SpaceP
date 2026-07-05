using UnityEngine;
using Firebase.Analytics;

public static class AnalyticsManager
{
    public static void LogGameComplete(int totalScore, int levelNumber)
    {
        LogWhenReady(() =>
        {
            FirebaseAnalytics.LogEvent(
                "game_complete",
                new Parameter("total_score", totalScore),
                new Parameter("level_number", levelNumber),
                new Parameter("level_name", GetLevelName(levelNumber))
            );
        });

        ReleaseLog.Log($"Analytics game_complete total_score={totalScore} level_number={levelNumber} level_name={GetLevelName(levelNumber)}");
    }

    public static void LogLeaderboardSubmit(bool success, int totalScore)
    {
        LogWhenReady(() =>
        {
            FirebaseAnalytics.LogEvent(
                "leaderboard_submit",
                new Parameter("success", success ? 1 : 0),
                new Parameter("total_score", totalScore)
            );
        });

        ReleaseLog.Log($"Analytics leaderboard_submit success={success} total_score={totalScore}");
    }

    public static void LogLevelStart(int levelNumber, int totalScore)
    {
        LogWhenReady(() =>
        {
            FirebaseAnalytics.LogEvent(
                "level_start",
                new Parameter("level_number", levelNumber),
                new Parameter("level_name", GetLevelName(levelNumber)),
                new Parameter("total_score", totalScore)
            );
        });

        ReleaseLog.Log($"Analytics level_start level_number={levelNumber} level_name={GetLevelName(levelNumber)} total_score={totalScore}");
    }

    public static void LogLevelComplete(int levelNumber, int levelScore, int totalScore, float timeSeconds)
    {
        int roundedTimeSeconds = Mathf.RoundToInt(timeSeconds);

        LogWhenReady(() =>
        {
            FirebaseAnalytics.LogEvent(
                "level_complete",
                new Parameter("level_number", levelNumber),
                new Parameter("level_name", GetLevelName(levelNumber)),
                new Parameter("level_score", levelScore),
                new Parameter("total_score", totalScore),
                new Parameter("time_seconds", roundedTimeSeconds)
            );
        });

        ReleaseLog.Log($"Analytics level_complete level_number={levelNumber} level_name={GetLevelName(levelNumber)} level_score={levelScore} total_score={totalScore} time_seconds={roundedTimeSeconds}");
    }

    public static void LogLevelFail(int levelNumber, string failReason, float timeSeconds, float speed, float landingAnglePercent)
    {
        int roundedTimeSeconds = Mathf.RoundToInt(timeSeconds);
        int roundedSpeed = Mathf.RoundToInt(speed * 100f);
        int roundedLandingAnglePercent = Mathf.RoundToInt(landingAnglePercent);

        LogWhenReady(() =>
        {
            FirebaseAnalytics.LogEvent(
                "level_fail",
                new Parameter("level_number", levelNumber),
                new Parameter("level_name", GetLevelName(levelNumber)),
                new Parameter("fail_reason", failReason),
                new Parameter("time_seconds", roundedTimeSeconds),
                new Parameter("speed_x100", roundedSpeed),
                new Parameter("landing_angle_percent", roundedLandingAnglePercent)
            );
        });

        ReleaseLog.Log($"Analytics level_fail level_number={levelNumber} level_name={GetLevelName(levelNumber)} fail_reason={failReason} time_seconds={roundedTimeSeconds}");
    }

    public static void LogLevelRetry(int levelNumber, float timeSeconds)
    {
        int roundedTimeSeconds = Mathf.RoundToInt(timeSeconds);

        LogWhenReady(() =>
        {
            FirebaseAnalytics.LogEvent(
                "level_retry",
                new Parameter("level_number", levelNumber),
                new Parameter("level_name", GetLevelName(levelNumber)),
                new Parameter("time_seconds", roundedTimeSeconds)
            );
        });

        ReleaseLog.Log($"Analytics level_retry level_number={levelNumber} level_name={GetLevelName(levelNumber)} time_seconds={roundedTimeSeconds}");
    }

    private static string GetLevelName(int levelNumber)
    {
        return $"level_{levelNumber}";
    }

    private static void LogWhenReady(System.Action logAction)
    {
        FirebaseManager.Initialize((isReady, message) =>
        {
            if (isReady)
            {
                logAction?.Invoke();
            }
        });
    }
}
