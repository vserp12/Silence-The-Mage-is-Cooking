using UnityEditor;

// Keeps MainMenu and Game in the build profile automatically on every compile.
[InitializeOnLoad]
static class BuildProfileSetup
{
    static BuildProfileSetup()
    {
        EditorApplication.delayCall += AddScenes;
    }

    static void AddScenes()
    {
        var scenes = new[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Game.unity"
        };

        var existing = EditorBuildSettings.scenes;
        var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(existing);
        bool dirty = false;

        foreach (var path in scenes)
        {
            if (list.Exists(s => s.path == path)) continue;
            list.Add(new EditorBuildSettingsScene(path, true));
            dirty = true;
        }

        if (dirty)
        {
            EditorBuildSettings.scenes = list.ToArray();
            UnityEngine.Debug.Log("[BuildProfileSetup] Added MainMenu + Game to build profile.");
        }
    }
}
