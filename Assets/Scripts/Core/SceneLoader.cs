using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public enum Scene
    {
        MainMenuScreen,
        GameScene,
        GameOverScreen,
        StoreScreen,
    }

    public static void LoadScene(Scene scene)
    {
        SceneManager.LoadScene(scene.ToString());
    }
}
