using System;
using UnityEngine;

public static class RewardedAdDailyLimit
{
    public const int MaxViewsPerDay = 3;

    private const string LastDateKey = "RewardedAds.LastDate";
    private const string ViewsWatchedKey = "RewardedAds.ViewsWatched";

    public static int GetRemainingViews()
    {
        RefreshForCurrentDate();
        return Mathf.Max(0, MaxViewsPerDay - GetWatchedViews());
    }

    public static bool HasRemainingViews()
    {
        return GetRemainingViews() > 0;
    }

    public static bool TryRecordCompletedView()
    {
        RefreshForCurrentDate();

        int watchedViews = GetWatchedViews();
        if (watchedViews >= MaxViewsPerDay)
        {
            return false;
        }

        PlayerPrefs.SetInt(ViewsWatchedKey, watchedViews + 1);
        PlayerPrefs.Save();
        return true;
    }

    private static void RefreshForCurrentDate()
    {
        int currentDate = GetCurrentDateValue();
        int savedDate = PlayerPrefs.GetInt(LastDateKey, 0);

        if (savedDate == 0)
        {
            PlayerPrefs.SetInt(LastDateKey, currentDate);
            PlayerPrefs.SetInt(ViewsWatchedKey, 0);
            PlayerPrefs.Save();
            return;
        }

        // Only a later date resets the limit. Moving the device clock backward
        // keeps the existing count until the saved date is reached again.
        if (currentDate > savedDate)
        {
            PlayerPrefs.SetInt(LastDateKey, currentDate);
            PlayerPrefs.SetInt(ViewsWatchedKey, 0);
            PlayerPrefs.Save();
        }
    }

    private static int GetWatchedViews()
    {
        return Mathf.Clamp(
            PlayerPrefs.GetInt(ViewsWatchedKey, 0),
            0,
            MaxViewsPerDay
        );
    }

    private static int GetCurrentDateValue()
    {
        DateTime today = DateTime.Now.Date;
        return today.Year * 10000 + today.Month * 100 + today.Day;
    }
}
