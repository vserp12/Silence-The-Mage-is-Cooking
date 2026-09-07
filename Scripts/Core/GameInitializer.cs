using UnityEngine;
using UnityEngine.SceneManagement;

// Resets Time.timeScale to 1 every time any scene loads so a paused death screen
// can never leave the next scene frozen.
public static class GameInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RegisterSceneReset()
    {
        SceneManager.sceneLoaded += (_, __) => Time.timeScale = 1f;
    }
}
