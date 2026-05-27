using System;
using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public static class LeaderboardManager
{
    private const string LeaderboardKey = "LocalLeaderboard";
    private const string PlayerNameKey = "LeaderboardPlayerName";
    private const string SubmittedCompletedScoreKey = "LeaderboardSubmittedCompletedScore";
    private const string LeaderboardPath = "leaderboards/global";
    private const int MaxSavedEntries = 20;
    private const int MaxPlayerNameLength = 15;

    [Serializable]
    private class LeaderboardEntryList
    {
        public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
    }

    public static string GetSavedPlayerName()
    {
        return PlayerPrefs.GetString(PlayerNameKey, string.Empty);
    }

    public static bool HasSubmittedCompletedGameScore(SaveData saveData)
    {
        return saveData != null
            && saveData.isGameCompleted
            && PlayerPrefs.GetInt(SubmittedCompletedScoreKey, int.MinValue) == saveData.totalScore;
    }

    public static void ClearSubmittedCompletedScore()
    {
        PlayerPrefs.DeleteKey(SubmittedCompletedScoreKey);
        PlayerPrefs.Save();
    }

    public static void SavePlayerName(string playerName)
    {
        string normalizedName = NormalizePlayerName(playerName);

        if (string.IsNullOrEmpty(normalizedName))
        {
            return;
        }

        PlayerPrefs.SetString(PlayerNameKey, normalizedName);
        PlayerPrefs.Save();
    }

    public static void SubmitCompletedGameScore(
        string playerName,
        SaveData saveData,
        Action<bool, string> onComplete
    )
    {
        if (saveData == null)
        {
            onComplete?.Invoke(false, "Missing save data.");
            return;
        }

        if (!saveData.isGameCompleted)
        {
            onComplete?.Invoke(false, "Complete all levels before submitting.");
            return;
        }

        if (HasSubmittedCompletedGameScore(saveData))
        {
            onComplete?.Invoke(false, "This completed score was already submitted.");
            return;
        }

        string normalizedName = NormalizePlayerName(playerName);

        if (string.IsNullOrEmpty(normalizedName))
        {
            onComplete?.Invoke(false, "Enter a player name.");
            return;
        }

        LeaderboardEntry entry = new LeaderboardEntry(
            normalizedName,
            saveData.totalScore,
            saveData.levelNumber,
            saveData.isGameCompleted,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        );

        FirebaseManager.Initialize((isReady, message) =>
        {
            if (!isReady)
            {
                AnalyticsManager.LogLeaderboardSubmit(false, saveData.totalScore);
                onComplete?.Invoke(false, "Firebase is not ready yet.");
                return;
            }

            IsPlayerNameTakenOnline(normalizedName, (success, isTaken, nameCheckMessage) =>
            {
                if (!success)
                {
                    AnalyticsManager.LogLeaderboardSubmit(false, saveData.totalScore);
                    onComplete?.Invoke(false, nameCheckMessage);
                    return;
                }

                if (isTaken)
                {
                    AnalyticsManager.LogLeaderboardSubmit(false, saveData.totalScore);
                    onComplete?.Invoke(false, "Player name is already taken.");
                    return;
                }

                SavePlayerName(normalizedName);
                SaveLocalEntry(entry);
                SubmitOnlineEntry(entry, onComplete);
            });
        });
    }

    public static bool IsValidPlayerName(string playerName)
    {
        return !string.IsNullOrEmpty(NormalizePlayerName(playerName));
    }

    public static void GetPlayerRankOnline(string playerName, int score, Action<int, string> onComplete)
    {
        string normalizedName = NormalizePlayerName(playerName);

        if (string.IsNullOrEmpty(normalizedName))
        {
            onComplete?.Invoke(0, "Missing player name.");
            return;
        }

        FirebaseManager.Initialize((isReady, message) =>
        {
            if (!isReady)
            {
                onComplete?.Invoke(0, message);
                return;
            }

            FirebaseDatabase.DefaultInstance
                .RootReference
                .Child(LeaderboardPath)
                .GetValueAsync()
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        onComplete?.Invoke(0, "Could not load global rank.");
                        return;
                    }

                    List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

                    foreach (DataSnapshot childSnapshot in task.Result.Children)
                    {
                        string json = childSnapshot.GetRawJsonValue();

                        if (string.IsNullOrEmpty(json))
                        {
                            continue;
                        }

                        LeaderboardEntry entry = JsonUtility.FromJson<LeaderboardEntry>(json);

                        if (entry != null)
                        {
                            entries.Add(entry);
                        }
                    }

                    SortLeaderboard(entries);

                    for (int i = 0; i < entries.Count; i++)
                    {
                        LeaderboardEntry entry = entries[i];

                        if (entry.score == score && entry.playerName == normalizedName)
                        {
                            onComplete?.Invoke(i + 1, "Global rank loaded.");
                            return;
                        }
                    }

                    onComplete?.Invoke(0, "Submitted score was not found in global rank yet.");
                });
        });
    }

    public static List<LeaderboardEntry> GetTopEntries(int limit)
    {
        LeaderboardEntryList leaderboard = LoadLocalLeaderboard();
        SortLeaderboard(leaderboard.entries);

        int count = Mathf.Clamp(limit, 0, leaderboard.entries.Count);
        return leaderboard.entries.GetRange(0, count);
    }

    public static void GetTopEntriesOnline(int limit, Action<List<LeaderboardEntry>, string> onComplete)
    {
        FirebaseManager.Initialize((isReady, message) =>
        {
            if (!isReady)
            {
                onComplete?.Invoke(GetTopEntries(limit), message);
                return;
            }

            FirebaseDatabase.DefaultInstance
                .RootReference
                .Child(LeaderboardPath)
                .OrderByChild("score")
                .LimitToLast(limit)
                .GetValueAsync()
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        onComplete?.Invoke(GetTopEntries(limit), "Could not load online leaderboard. Showing local scores.");
                        return;
                    }

                    List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

                    foreach (DataSnapshot childSnapshot in task.Result.Children)
                    {
                        string json = childSnapshot.GetRawJsonValue();

                        if (string.IsNullOrEmpty(json))
                        {
                            continue;
                        }

                        LeaderboardEntry entry = JsonUtility.FromJson<LeaderboardEntry>(json);

                        if (entry != null)
                        {
                            entries.Add(entry);
                        }
                    }

                    SortLeaderboard(entries);

                    if (entries.Count == 0)
                    {
                        onComplete?.Invoke(GetTopEntries(limit), "No online scores yet. Showing local scores.");
                        return;
                    }

                    onComplete?.Invoke(entries, "Online leaderboard loaded.");
                });
        });
    }

    private static string NormalizePlayerName(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            return string.Empty;
        }

        string normalizedName = playerName.Trim();

        if (normalizedName.Length > MaxPlayerNameLength)
        {
            normalizedName = normalizedName.Substring(0, MaxPlayerNameLength);
        }

        return normalizedName;
    }

    private static void IsPlayerNameTakenOnline(string playerName, Action<bool, bool, string> onComplete)
    {
        string normalizedName = NormalizePlayerName(playerName);

        if (IsPlayerNameTakenLocal(normalizedName))
        {
            onComplete?.Invoke(true, true, "Player name is already taken.");
            return;
        }

        FirebaseDatabase.DefaultInstance
            .RootReference
            .Child(LeaderboardPath)
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    onComplete?.Invoke(false, false, "Could not verify player name.");
                    return;
                }

                bool isTaken = false;

                foreach (DataSnapshot childSnapshot in task.Result.Children)
                {
                    string json = childSnapshot.GetRawJsonValue();

                    if (string.IsNullOrEmpty(json))
                    {
                        continue;
                    }

                    LeaderboardEntry entry = JsonUtility.FromJson<LeaderboardEntry>(json);

                    if (entry != null && string.Equals(entry.playerName, normalizedName, StringComparison.OrdinalIgnoreCase))
                    {
                        isTaken = true;
                        break;
                    }
                }

                onComplete?.Invoke(true, isTaken, isTaken ? "Player name is already taken." : "Player name is available.");
            });
    }

    private static bool IsPlayerNameTakenLocal(string playerName)
    {
        LeaderboardEntryList leaderboard = LoadLocalLeaderboard();

        foreach (LeaderboardEntry entry in leaderboard.entries)
        {
            if (string.Equals(entry.playerName, playerName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static void SaveLocalEntry(LeaderboardEntry entry)
    {
        LeaderboardEntryList leaderboard = LoadLocalLeaderboard();
        leaderboard.entries.Add(entry);

        SortLeaderboard(leaderboard.entries);

        if (leaderboard.entries.Count > MaxSavedEntries)
        {
            leaderboard.entries.RemoveRange(MaxSavedEntries, leaderboard.entries.Count - MaxSavedEntries);
        }

        PlayerPrefs.SetString(LeaderboardKey, JsonUtility.ToJson(leaderboard));
        PlayerPrefs.Save();
    }

    private static void SubmitOnlineEntry(LeaderboardEntry entry, Action<bool, string> onComplete)
    {
        string json = JsonUtility.ToJson(entry);

        FirebaseDatabase.DefaultInstance
            .RootReference
            .Child(LeaderboardPath)
            .Push()
            .SetRawJsonValueAsync(json)
            .ContinueWithOnMainThread(task =>
            {
                bool success = !task.IsFaulted && !task.IsCanceled;
                AnalyticsManager.LogLeaderboardSubmit(success, entry.score);

                string message = success
                    ? "Score submitted online."
                    : "Score saved locally, but online submit failed.";

                if (success)
                {
                    MarkCompletedScoreSubmitted(entry.score);
                }

                if (!success && task.Exception != null)
                {
                    Debug.LogError(task.Exception);
                }

                onComplete?.Invoke(success, message);
            });
    }

    private static LeaderboardEntryList LoadLocalLeaderboard()
    {
        string json = PlayerPrefs.GetString(LeaderboardKey, string.Empty);

        if (string.IsNullOrEmpty(json))
        {
            return new LeaderboardEntryList();
        }

        LeaderboardEntryList leaderboard = JsonUtility.FromJson<LeaderboardEntryList>(json);
        return leaderboard ?? new LeaderboardEntryList();
    }

    private static void SortLeaderboard(List<LeaderboardEntry> entries)
    {
        entries.Sort((left, right) => right.score.CompareTo(left.score));
    }

    private static void MarkCompletedScoreSubmitted(int score)
    {
        PlayerPrefs.SetInt(SubmittedCompletedScoreKey, score);
        PlayerPrefs.Save();
    }
}
