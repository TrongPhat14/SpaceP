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
        SceneTransition.LoadScene(scene.ToString());
    }
}
