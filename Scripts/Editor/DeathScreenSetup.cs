using System.Linq;
using UnityEditor;
using UnityEngine;

// Automatically creates Assets/Resources/DeathScreenData.asset on every compile.
[InitializeOnLoad]
static class DeathScreenSetup
{
    static DeathScreenSetup()
    {
        EditorApplication.delayCall += CreateAsset;
    }

    static void CreateAsset()
    {
        const string assetPath = "Assets/Resources/DeathScreenData.asset";
        if (AssetDatabase.LoadAssetAtPath<DeathScreenData>(assetPath) != null) return;

        var sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/death-screen.png")
            .OfType<Sprite>()
            .OrderBy(s => s.name)
            .ToArray();
        if (sprites.Length == 0) return;

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        var data = ScriptableObject.CreateInstance<DeathScreenData>();
        data.frames = sprites;
        AssetDatabase.CreateAsset(data, assetPath);
        AssetDatabase.SaveAssets();
        Debug.Log($"[DeathScreenSetup] Created DeathScreenData ({sprites.Length} frames).");
    }
}
