using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor.SceneManagement;

/// Tools > Setup Game Scene (Issues #3 #4 #5)
/// Run with the Game scene open. Creates SpellSelectionUI, GameBootstrap, wires
/// all references, and saves the scene automatically.
public static class GameSceneSetup
{
    [MenuItem("Tools/Setup Game Scene (Issues #3 #4 #5)")]
    public static void SetupGameScene()
    {
        try
        {
            Debug.Log("[GameSceneSetup] Step 1: GameBootstrap...");
            var bootstrap = Object.FindFirstObjectByType<GameBootstrap>();
            if (bootstrap == null)
                bootstrap = new GameObject("GameBootstrap").AddComponent<GameBootstrap>();

            bootstrap.elfMeleePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/ElfMelee.prefab");
            bootstrap.elfMagicPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/ElfMagic.prefab");
            bootstrap.santaPrefab    = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Santa.prefab");
            bootstrap.spellDatabase  = AssetDatabase.LoadAssetAtPath<SpellDatabase>("Assets/ScriptableObjects/SpellDatabase.asset");
            EditorUtility.SetDirty(bootstrap);

            Debug.Log("[GameSceneSetup] Step 2: WaveManager...");
            var waveManager = Object.FindFirstObjectByType<WaveManager>();
            if (waveManager != null)
            {
                waveManager.elfMeleePrefab = bootstrap.elfMeleePrefab;
                waveManager.elfMagicPrefab = bootstrap.elfMagicPrefab;
                waveManager.santaPrefab    = bootstrap.santaPrefab;
                EditorUtility.SetDirty(waveManager);
            }
            else Debug.LogWarning("[GameSceneSetup] WaveManager not found in scene.");

            Debug.Log("[GameSceneSetup] Step 3: SpellCaster...");
            var caster = Object.FindFirstObjectByType<SpellCaster>();
            if (caster != null && bootstrap.spellDatabase != null)
            {
                caster.spellDatabase = bootstrap.spellDatabase;
                EditorUtility.SetDirty(caster);
            }

            Debug.Log("[GameSceneSetup] Step 4: EventSystem...");
            EnsureEventSystem();

            Debug.Log("[GameSceneSetup] Step 5: Canvas...");
            var canvas = GetOrCreateCanvas();

            Debug.Log("[GameSceneSetup] Step 6: SpellSelectionUI...");
            var existingUI = Object.FindFirstObjectByType<SpellSelectionUI>();
            if (existingUI == null)
                BuildSpellSelectionPanel(canvas);

            var selUI = Object.FindFirstObjectByType<SpellSelectionUI>();
            if (waveManager != null && selUI != null)
            {
                waveManager.spellSelectionUI = selUI;
                EditorUtility.SetDirty(waveManager);
            }

            Debug.Log("[GameSceneSetup] Step 7: Saving...");
            EditorSceneManager.MarkAllScenesDirty();
            AssetDatabase.SaveAssets();

            Debug.Log("[GameSceneSetup] Done — press Ctrl+S to save the scene.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameSceneSetup] FAILED at: {e.Message}\n{e.StackTrace}");
        }
    }

    // ── UI builders ───────────────────────────────────────────────────────

    static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
    }

    static Canvas GetOrCreateCanvas()
    {
        // Prefer an existing Screen-Space overlay canvas
        foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            if (c.renderMode == RenderMode.ScreenSpaceOverlay) return c;

        var cvGO = new GameObject("UICanvas");
        var canvas = cvGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = cvGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        cvGO.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    static void BuildSpellSelectionPanel(Canvas canvas)
    {
        // Dark semi-transparent overlay
        var panelGO = new GameObject("SpellSelectionPanel");
        panelGO.transform.SetParent(canvas.transform, false);
        var panelRT  = panelGO.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;
        panelGO.AddComponent<Image>().color = new Color(0f, 0f, 0.05f, 0.85f);

        // Wave label
        var waveLabel = MakeLabel(panelGO.transform, "WaveLabel",
            new Vector2(0f, 200f), new Vector2(800f, 70f), 32f, "Choose your starting spell!");

        // Four element buttons
        var waterBtn = MakeButton(panelGO.transform, "Water",       new Vector2(-405f, 0f), Color.cyan);
        var fireBtn  = MakeButton(panelGO.transform, "Fire",        new Vector2(-135f, 0f), new Color(1f, 0.4f, 0f));
        var elecBtn  = MakeButton(panelGO.transform, "Electricity", new Vector2( 135f, 0f), Color.yellow);
        var lightBtn = MakeButton(panelGO.transform, "Light",       new Vector2( 405f, 0f), Color.white);

        // Level sub-labels
        var wLvl  = MakeLabel(panelGO.transform, "WaterLvl",  new Vector2(-405f, -90f), new Vector2(150f, 36f), 22f, "NEW");
        var fLvl  = MakeLabel(panelGO.transform, "FireLvl",   new Vector2(-135f, -90f), new Vector2(150f, 36f), 22f, "NEW");
        var eLvl  = MakeLabel(panelGO.transform, "ElecLvl",   new Vector2( 135f, -90f), new Vector2(150f, 36f), 22f, "NEW");
        var liLvl = MakeLabel(panelGO.transform, "LightLvl",  new Vector2( 405f, -90f), new Vector2(150f, 36f), 22f, "NEW");

        // SpellSelectionUI component on its own GO
        var uiGO = new GameObject("SpellSelectionUI");
        uiGO.transform.SetParent(canvas.transform, false);
        var ui = uiGO.AddComponent<SpellSelectionUI>();
        ui.panel                = panelGO;
        ui.waterButton          = waterBtn.GetComponent<Button>();
        ui.fireButton           = fireBtn.GetComponent<Button>();
        ui.electricityButton    = elecBtn.GetComponent<Button>();
        ui.lightButton          = lightBtn.GetComponent<Button>();
        ui.waterLevelText       = wLvl;
        ui.fireLevelText        = fLvl;
        ui.electricityLevelText = eLvl;
        ui.lightLevelText       = liLvl;
        ui.waveLabel            = waveLabel;

        panelGO.SetActive(false);
        EditorUtility.SetDirty(uiGO);
    }

    static GameObject MakeButton(Transform parent, string label, Vector2 pos, Color color)
    {
        var go = new GameObject($"{label}Btn");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta     = new Vector2(220f, 120f);
        rt.anchoredPosition = pos;

        var img = go.AddComponent<Image>();
        img.color = color * 0.55f;

        var btn = go.AddComponent<Button>();
        var cb = btn.colors;
        cb.normalColor      = color * 0.55f;
        cb.highlightedColor = color * 0.85f;
        cb.pressedColor     = color * 0.35f;
        btn.colors = cb;

        var txtGO = new GameObject("Label");
        txtGO.transform.SetParent(go.transform, false);
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize  = 22f;
        tmp.color     = Color.white;
        var trt = txtGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        return go;
    }

    static TextMeshProUGUI MakeLabel(Transform parent, string name,
        Vector2 pos, Vector2 size, float fontSize, string text)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta        = size;
        rt.anchoredPosition = pos;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize  = fontSize;
        tmp.color     = Color.white;
        return tmp;
    }
}


