using UnityEngine;

// Fullscreen OnGUI overlay for:
// 1) Initial Spell Selection at game start
// 2) 20-second Intermission recovery period with Stats Upgrade + Spell Upgrade / Selection
public class RuntimeSpellSelection : MonoBehaviour
{
    private bool visible;
    private bool isIntermission;
    private int currentWave;
    private float intermissionTimer = 20f;
    private System.Action<ElementType> onSpellPick;
    private System.Action onWaveStart;

    public void ShowInitialSelection(System.Action<ElementType> onPick)
    {
        visible = true;
        isIntermission = false;
        currentWave = 0;
        onSpellPick = onPick;
        Time.timeScale = 0f;
    }

    public void ShowIntermission(int clearedWave, float duration, System.Action<ElementType> onPick, System.Action onStartWave)
    {
        visible = true;
        isIntermission = true;
        currentWave = clearedWave;
        intermissionTimer = duration;
        onSpellPick = onPick;
        onWaveStart = onStartWave;
        Time.timeScale = 0f;

        // Award 1 stat upgrade point for clearing the wave
        PlayerStats.Instance?.AwardWavePoints(1);
        // Small recovery heal
        var health = FindObjectOfType<PlayerHealth>();
        if (health != null) health.Heal(20f);
    }

    public void Hide()
    {
        visible = false;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (!visible) return;

        if (isIntermission)
        {
            intermissionTimer -= Time.unscaledDeltaTime;
            if (intermissionTimer <= 0f)
            {
                intermissionTimer = 0f;
                StartNextWave();
            }
        }
    }

    void StartNextWave()
    {
        Hide();
        onWaveStart?.Invoke();
    }

    void OnGUI()
    {
        if (!visible) return;

        float sw = Screen.width, sh = Screen.height;

        // Dark background overlay
        var oldColor = GUI.color;
        GUI.color = new Color(0.02f, 0.02f, 0.08f, 0.92f);
        GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
        GUI.color = oldColor;

        if (!isIntermission)
        {
            DrawInitialMenu(sw, sh);
        }
        else
        {
            DrawIntermissionMenu(sw, sh);
        }
    }

    void DrawInitialMenu(float sw, float sh)
    {
        // Title
        GUI.Label(new Rect(sw / 2 - 350f, sh / 2 - 180f, 700f, 60f),
            "SILENCIO: THE MAGE IS COOKING",
            MakeLabel(28, new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleCenter, true));

        GUI.Label(new Rect(sw / 2 - 350f, sh / 2 - 120f, 700f, 40f),
            "Choose your starting spell to begin Wave 1!",
            MakeLabel(20, Color.white, TextAnchor.MiddleCenter, false));

        // 4 Spell Buttons
        const float btnW = 150f, btnH = 80f, gap = 24f;
        float totalW = 4 * btnW + 3 * gap;
        float x0 = sw / 2f - totalW / 2f;
        float by = sh / 2f - 20f;

        DrawSpellButton("Water", new Color(0.2f, 0.8f, 1f), ElementType.Water, x0, by, btnW, btnH);
        DrawSpellButton("Fire", new Color(1f, 0.45f, 0.1f), ElementType.Fire, x0 + (btnW + gap), by, btnW, btnH);
        DrawSpellButton("Electricity", new Color(1f, 0.95f, 0.2f), ElementType.Electricity, x0 + 2 * (btnW + gap), by, btnW, btnH);
        DrawSpellButton("Light", Color.white, ElementType.Light, x0 + 3 * (btnW + gap), by, btnW, btnH);
    }

    void DrawIntermissionMenu(float sw, float sh)
    {
        // Header
        string title = $"Wave {currentWave} Cleared!";
        GUI.Label(new Rect(sw / 2 - 300f, 25f, 600f, 45f), title,
            MakeLabel(26, new Color(0.4f, 1f, 0.4f), TextAnchor.MiddleCenter, true));

        // Timer
        string timerStr = $"Next wave in: {intermissionTimer:0.0}s";
        GUI.Label(new Rect(sw / 2 - 200f, 70f, 400f, 35f), timerStr,
            MakeLabel(22, new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleCenter, true));

        // Skip / Start Now button
        var bgOld = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.2f, 0.9f, 0.4f);
        if (GUI.Button(new Rect(sw / 2 - 130f, 110f, 260f, 42f), "Start Wave Now >>", MakeButton(18)))
        {
            StartNextWave();
        }
        GUI.backgroundColor = bgOld;

        // ── Section 1: Stats Upgrades ──────────────────────────────────────────
        int pts = PlayerStats.Instance != null ? PlayerStats.Instance.availableUpgradePoints : 0;
        GUI.Label(new Rect(sw / 2 - 350f, 165f, 700f, 32f),
            $"CHARACTER UPGRADES  (Available Points: {pts})",
            MakeLabel(18, new Color(0.9f, 0.9f, 1f), TextAnchor.MiddleCenter, true));

        float statW = 160f, statH = 55f, statGap = 16f;
        float statTotalW = 4 * statW + 3 * statGap;
        float sx0 = sw / 2f - statTotalW / 2f;
        float sy = 205f;

        DrawStatButton("Max HP", "+25 HP",
            PlayerStats.Instance != null ? PlayerStats.Instance.hpLevel : 0,
            pts > 0, () => PlayerStats.Instance?.UpgradeHP(),
            sx0, sy, statW, statH);

        DrawStatButton("Speed", "+15% Move",
            PlayerStats.Instance != null ? PlayerStats.Instance.speedLevel : 0,
            pts > 0, () => PlayerStats.Instance?.UpgradeSpeed(),
            sx0 + (statW + statGap), sy, statW, statH);

        DrawStatButton("Damage", "+20% Spells",
            PlayerStats.Instance != null ? PlayerStats.Instance.damageLevel : 0,
            pts > 0, () => PlayerStats.Instance?.UpgradeDamage(),
            sx0 + 2 * (statW + statGap), sy, statW, statH);

        DrawStatButton("Cooldown", "-12% Cast Time",
            PlayerStats.Instance != null ? PlayerStats.Instance.cooldownLevel : 0,
            pts > 0, () => PlayerStats.Instance?.UpgradeCooldown(),
            sx0 + 3 * (statW + statGap), sy, statW, statH);

        // ── Section 2: Spell Upgrades / Switch ──────────────────────────────────
        GUI.Label(new Rect(sw / 2 - 350f, 280f, 700f, 32f),
            "SPELL UPGRADE / SELECTION (Select to upgrade or equip)",
            MakeLabel(18, new Color(0.9f, 0.9f, 1f), TextAnchor.MiddleCenter, true));

        const float spellW = 150f, spellH = 75f, spellGap = 20f;
        float spellTotalW = 4 * spellW + 3 * spellGap;
        float spX0 = sw / 2f - spellTotalW / 2f;
        float spY = 320f;

        DrawSpellButton("Water", new Color(0.2f, 0.8f, 1f), ElementType.Water, spX0, spY, spellW, spellH);
        DrawSpellButton("Fire", new Color(1f, 0.45f, 0.1f), ElementType.Fire, spX0 + (spellW + spellGap), spY, spellW, spellH);
        DrawSpellButton("Electricity", new Color(1f, 0.95f, 0.2f), ElementType.Electricity, spX0 + 2 * (spellW + spellGap), spY, spellW, spellH);
        DrawSpellButton("Light", Color.white, ElementType.Light, spX0 + 3 * (spellW + spellGap), spY, spellW, spellH);
    }

    void DrawStatButton(string name, string bonus, int level, bool canBuy, System.Action onBuy, float x, float y, float w, float h)
    {
        var bgOld = GUI.backgroundColor;
        bool isMax = level >= PlayerStats.MaxLevel;

        if (isMax) GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f);
        else if (canBuy) GUI.backgroundColor = new Color(0.3f, 0.8f, 1f);
        else GUI.backgroundColor = new Color(0.25f, 0.25f, 0.35f);

        string label = isMax
            ? $"{name}\n[MAX]"
            : $"{name}\n{bonus} (Lv.{level}/5)";

        if (GUI.Button(new Rect(x, y, w, h), label, MakeButton(13)))
        {
            if (canBuy && !isMax) onBuy?.Invoke();
        }
        GUI.backgroundColor = bgOld;
    }

    void DrawSpellButton(string name, Color color, ElementType elem, float x, float y, float w, float h)
    {
        var bgOld = GUI.backgroundColor;
        GUI.backgroundColor = color;

        int lvl = PlayerSpellInventory.Instance != null ? PlayerSpellInventory.Instance.GetLevel(elem) : 0;
        bool isActive = PlayerSpellInventory.Instance != null && PlayerSpellInventory.Instance.ActiveElement == elem;

        string badge = lvl == 0 ? "NEW!" : (lvl >= 4 ? "Lv.4 [MAX]" : $"Lv.{lvl} -> Lv.{lvl + 1}");
        string activeTag = isActive ? " *EQUIPPED*" : "";
        string buttonText = $"{name}{activeTag}\n[{badge}]";

        if (GUI.Button(new Rect(x, y, w, h), buttonText, MakeButton(14)))
        {
            onSpellPick?.Invoke(elem);
        }
        GUI.backgroundColor = bgOld;
    }

    static GUIStyle MakeLabel(int size, Color color, TextAnchor align, bool bold) => new GUIStyle(GUI.skin.label)
    {
        fontSize = size,
        fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
        alignment = align,
        normal = { textColor = color }
    };

    static GUIStyle MakeButton(int size) => new GUIStyle(GUI.skin.button)
    {
        fontSize = size,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter
    };
}
