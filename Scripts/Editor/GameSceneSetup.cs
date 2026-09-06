using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// Tools > Setup Game Scene (Issues #3, #4, #5)
/// Adds SpellSelectionUI canvas to the active Game scene and wires all references.
/// Run this while the Game scene is open.
public static class GameSceneSetup
{
    [MenuItem("Tools/Setup Game Scene (Issues #3 #4 #5)")]
    public static void SetupGameScene()
    {
        // ── SpellSelectionUI panel ────────────────────────────────────────
        var existingUI = Object.FindFirstObjectByType<SpellSelectionUI>();
        if (existingUI == null)
            BuildSpellSelectionPanel();

        // ── Wire WaveManager → SpellSelectionUI ───────────────────────────
        var waveManager = Object.FindFirstObjectByType<WaveManager>();
        var selUI       = Object.FindFirstObjectByType<SpellSelectionUI>();
        if (waveManager != null && selUI != null)
        {
            waveManager.spellSelectionUI = selUI;
            EditorUtility.SetDirty(waveManager);
        }

        // ── Wire SpellDatabase into SpellCaster ───────────────────────────
        var db = AssetDatabase.LoadAssetAtPath<SpellDatabase>("Assets/ScriptableObjects/SpellDatabase.asset");
        var caster = Object.FindFirstObjectByType<SpellCaster>();
        if (caster != null && db != null)
        {
            caster.spellDatabase = db;
            EditorUtility.SetDirty(caster);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[GameSceneSetup] Done.");
    }

    static void BuildSpellSelectionPanel()
    {
        // Find or create a Canvas
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var cvGO = new GameObject("Canvas");
            canvas = cvGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cvGO.AddComponent<CanvasScaler>();
            cvGO.AddComponent<GraphicRaycaster>();
        }

        // Root panel (dark overlay)
        var panelGO = new GameObject("SpellSelectionPanel");
        panelGO.transform.SetParent(canvas.transform, false);

        var panelRT = panelGO.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.75f);

        // Wave label
        var waveLabel = MakeLabel(panelGO.transform, "WaveLabel",
            new Vector2(0f, 120f), new Vector2(600f, 60f), 28, "Choose your starting spell!");

        // Buttons + level labels
        var waterBtn  = MakeElementButton(panelGO.transform, "Water",       new Vector2(-270f, 0f), Color.cyan);
        var fireBtn   = MakeElementButton(panelGO.transform, "Fire",        new Vector2(-90f,  0f), new Color(1f, 0.4f, 0f));
        var elecBtn   = MakeElementButton(panelGO.transform, "Electricity", new Vector2(90f,   0f), Color.yellow);
        var lightBtn  = MakeElementButton(panelGO.transform, "Light",       new Vector2(270f,  0f), Color.white);

        var waterLvl  = MakeLabel(panelGO.transform, "WaterLevel",       new Vector2(-270f, -70f), new Vector2(120f, 28f), 18, "NEW");
        var fireLvl   = MakeLabel(panelGO.transform, "FireLevel",        new Vector2(-90f,  -70f), new Vector2(120f, 28f), 18, "NEW");
        var elecLvl   = MakeLabel(panelGO.transform, "ElecLevel",        new Vector2(90f,   -70f), new Vector2(120f, 28f), 18, "NEW");
        var lightLvl  = MakeLabel(panelGO.transform, "LightLevel",       new Vector2(270f,  -70f), new Vector2(120f, 28f), 18, "NEW");

        // SpellSelectionUI component
        var uiGO = new GameObject("SpellSelectionUI");
        uiGO.transform.SetParent(canvas.transform, false);
        var ui = uiGO.AddComponent<SpellSelectionUI>();

        ui.panel               = panelGO;
        ui.waterButton         = waterBtn.GetComponent<Button>();
        ui.fireButton          = fireBtn.GetComponent<Button>();
        ui.electricityButton   = elecBtn.GetComponent<Button>();
        ui.lightButton         = lightBtn.GetComponent<Button>();
        ui.waterLevelText      = waterLvl;
        ui.fireLevelText       = fireLvl;
        ui.electricityLevelText= elecLvl;
        ui.lightLevelText      = lightLvl;
        ui.waveLabel           = waveLabel;

        panelGO.SetActive(false);
        EditorUtility.SetDirty(uiGO);
    }

    static GameObject MakeElementButton(Transform parent, string label,
        Vector2 anchoredPos, Color color)
    {
        var go = new GameObject($"{label}Button");
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta     = new Vector2(120f, 80f);
        rt.anchoredPosition = anchoredPos;

        var img = go.AddComponent<Image>();
        img.color = color * 0.7f;

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = color;
        btn.colors = colors;

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize  = 18;
        tmp.color     = Color.white;
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        return go;
    }

    static TextMeshProUGUI MakeLabel(Transform parent, string goName,
        Vector2 anchoredPos, Vector2 size, float fontSize, string defaultText)
    {
        var go = new GameObject(goName);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta        = size;
        rt.anchoredPosition = anchoredPos;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = defaultText;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize  = fontSize;
        tmp.color     = Color.white;
        return tmp;
    }
}
