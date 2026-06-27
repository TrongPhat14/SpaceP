using UnityEngine;

public static class SceneLoader
{
    public enum Scene
    {
        MainMenuScreen,
        GameScene,
        GameOverScreen,
        StoreScreen,
        LeaderboardScreen,
    }

    public static void LoadScene(Scene scene)
    {
        LoadScene(scene.ToString());
    }

    public static void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("SceneLoader received an empty scene name.");
            return;
        }

        SceneTransition.LoadScene(sceneName);
    }
}
