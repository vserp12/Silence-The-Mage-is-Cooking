using UnityEngine;

// Fullscreen OnGUI spell-selection overlay.
// Requires no Canvas, no EventSystem, no TextMeshPro — zero scene setup needed.
// WaveManager adds this component to itself at runtime when SpellSelectionUI is absent.
public class RuntimeSpellSelection : MonoBehaviour
{
    private bool visible;
    private int wavesCompleted;
    private System.Action<ElementType> onPick;

    public void Show(int waves, System.Action<ElementType> callback)
    {
        visible       = true;
        wavesCompleted = waves;
        onPick        = callback;
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        visible        = false;
        Time.timeScale = 1f;
    }

    void OnGUI()
    {
        if (!visible) return;

        float sw = Screen.width, sh = Screen.height;

        // Dark overlay
        var old = GUI.color;
        GUI.color = new Color(0f, 0f, 0.05f, 0.88f);
        GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
        GUI.color = old;

        // Header
        string header = wavesCompleted == 0
            ? "Choose your starting spell!"
            : $"Wave {wavesCompleted} cleared!\nChoose a spell:";
        GUI.Label(new Rect(sw / 2 - 320f, sh / 2 - 190f, 640f, 80f), header,
            BigLabel(24, Color.white, TextAnchor.MiddleCenter));

        // Buttons
        const float btnW = 150f, btnH = 70f, gap = 20f;
        float totalW = 4 * btnW + 3 * gap;
        float x0 = sw / 2f - totalW / 2f;
        float by = sh / 2f - btnH / 2f;

        DrawBtn("Water",       new Color(0.2f, 0.8f, 1f),   ElementType.Water,       x0,                  by, btnW, btnH);
        DrawBtn("Fire",        new Color(1f,   0.4f, 0f),   ElementType.Fire,        x0 + (btnW+gap),     by, btnW, btnH);
        DrawBtn("Electricity", new Color(1f,   1f,   0.1f), ElementType.Electricity, x0 + 2*(btnW+gap),   by, btnW, btnH);
        DrawBtn("Light",       Color.white,                  ElementType.Light,       x0 + 3*(btnW+gap),   by, btnW, btnH);

        // Level labels
        float ly = by + btnH + 6f;
        DrawLvl(ElementType.Water,       x0,                ly, btnW);
        DrawLvl(ElementType.Fire,        x0 + (btnW+gap),   ly, btnW);
        DrawLvl(ElementType.Electricity, x0+2*(btnW+gap),   ly, btnW);
        DrawLvl(ElementType.Light,       x0+3*(btnW+gap),   ly, btnW);
    }

    void DrawBtn(string label, Color color, ElementType elem, float x, float y, float w, float h)
    {
        var bgOld = GUI.backgroundColor;
        GUI.backgroundColor = color;
        if (GUI.Button(new Rect(x, y, w, h), label, BigLabel(18, Color.black, TextAnchor.MiddleCenter)))
            onPick?.Invoke(elem);
        GUI.backgroundColor = bgOld;
    }

    void DrawLvl(ElementType elem, float x, float y, float w)
    {
        int lvl = PlayerSpellInventory.Instance != null ? PlayerSpellInventory.Instance.GetLevel(elem) : 0;
        GUI.Label(new Rect(x, y, w, 28f), lvl == 0 ? "NEW" : $"Lv.{lvl}",
            BigLabel(16, Color.white, TextAnchor.MiddleCenter));
    }

    static GUIStyle BigLabel(int size, Color color, TextAnchor align) => new GUIStyle(GUI.skin.label)
    {
        fontSize  = size,
        alignment = align,
        normal    = { textColor = color }
    };
}
