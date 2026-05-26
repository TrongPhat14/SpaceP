using System;
using System.Collections.Generic;
using Firebase;
using Firebase.Extensions;
using UnityEngine;

public static class FirebaseManager
{
    public static bool IsReady { get; private set; }
    public static bool IsInitializing { get; private set; }

    private static readonly List<Action<bool, string>> PendingCallbacks = new List<Action<bool, string>>();

    public static void Initialize(Action<bool, string> onComplete = null)
    {
        if (IsReady)
        {
            onComplete?.Invoke(true, "Firebase is ready.");
            return;
        }

        if (IsInitializing)
        {
            if (onComplete != null)
            {
                PendingCallbacks.Add(onComplete);
            }

            return;
        }

        IsInitializing = true;

        if (onComplete != null)
        {
            PendingCallbacks.Add(onComplete);
        }

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            IsInitializing = false;

            if (task.IsFaulted || task.IsCanceled)
            {
                IsReady = false;
                string errorMessage = "Firebase dependency check failed.";
                Debug.LogError(errorMessage);
                CompletePendingCallbacks(false, errorMessage);
                return;
            }

            DependencyStatus dependencyStatus = task.Result;
            IsReady = dependencyStatus == DependencyStatus.Available;

            string message = IsReady
                ? "Firebase is ready."
                : $"Firebase dependencies are not available: {dependencyStatus}";

            if (IsReady)
            {
                Debug.Log(message);
            }
            else
            {
                Debug.LogError(message);
            }

            CompletePendingCallbacks(IsReady, message);
        });
    }

    private static void CompletePendingCallbacks(bool isReady, string message)
    {
        foreach (Action<bool, string> callback in PendingCallbacks)
        {
            callback?.Invoke(isReady, message);
        }

        PendingCallbacks.Clear();
    }
}
