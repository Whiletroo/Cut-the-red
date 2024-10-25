using UnityEngine.SceneManagement;

public static class Loader
{
    public enum Scene
    {
        MainMenuScene,
        LoadingScene,
        GameScene
    }

    public static Scene TargetScene;

    public static void Load(Scene targetScene)
    {
        Loader.TargetScene = targetScene;

        SceneManager.LoadScene(Scene.LoadingScene.ToString());

    }

    public static void LoaderCallback()
    {

        SceneManager.LoadScene(TargetScene.ToString());
    }
}
