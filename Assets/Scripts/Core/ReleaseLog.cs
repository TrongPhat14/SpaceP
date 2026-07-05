using System.Diagnostics;
using UnityEngine;

public static class ReleaseLog
{
    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void Log(object message)
    {
        UnityEngine.Debug.Log(message);
    }

    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void Log(object message, UnityEngine.Object context)
    {
        UnityEngine.Debug.Log(message, context);
    }

    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void Warning(object message)
    {
        UnityEngine.Debug.LogWarning(message);
    }

    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void Warning(object message, UnityEngine.Object context)
    {
        UnityEngine.Debug.LogWarning(message, context);
    }
}
