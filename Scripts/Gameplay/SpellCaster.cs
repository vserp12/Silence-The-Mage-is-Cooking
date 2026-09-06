using UnityEngine;
using UnityEngine.InputSystem;

public class SpellCaster : MonoBehaviour
{
    public SpellDatabase spellDatabase;
    public SpellData fallbackSpell; // used until SpellSelectionUI assigns one

    private SpellData currentSpell;
    private float castProgress;
    private bool isCasting;
    private Animator playerAnimator;
    private PlayerStats stats;

    // Simple code-based cast bar drawn above the player
    private Transform castBarFill;

    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        stats = GetComponent<PlayerStats>() ?? gameObject.AddComponent<PlayerStats>();
        if (spellDatabase == null)
            spellDatabase = Resources.Load<SpellDatabase>("SpellDatabase");
        BuildCastBar();
    }

    // Called by SpellSelectionUI after the player picks an element
    public void SetSpellByElement(ElementType element, int level)
    {
        if (spellDatabase == null)
            spellDatabase = Resources.Load<SpellDatabase>("SpellDatabase");
        if (spellDatabase == null) return;

        currentSpell = spellDatabase.GetSpell(element, level);
        castProgress = 0f;
        isCasting = false;
        UpdateCastBarVisual(0f);
    }

    void Update()
    {
        SpellData spell = currentSpell ?? fallbackSpell;
        // Auto-use the lowest available spell so casting works before UI is set up
        if (spell == null && spellDatabase != null)
            spell = spellDatabase.GetSpell(ElementType.Water, 1);
        if (spell == null || spell.projectilePrefab == null) return;

        bool pressing = false;
        if (Mouse.current != null)
            pressing = Mouse.current.leftButton.isPressed;
        if (!pressing)
        {
            try { pressing = Input.GetMouseButton(0); } catch { }
        }

        if (pressing)
        {
            if (!isCasting) isCasting = true;

            float cdMultiplier = stats != null ? stats.GetCooldownMultiplier() : 1f;
            float effectiveCastTime = Mathf.Max(0.1f, spell.castTime * cdMultiplier);

            castProgress += Time.deltaTime / effectiveCastTime;
            UpdateCastBarVisual(Mathf.Clamp01(castProgress));

            if (castProgress >= 1f)
            {
                CastSpell(spell);
                castProgress = 0f;
                isCasting = false;
                UpdateCastBarVisual(0f);
            }
        }
        else
        {
            if (isCasting)
            {
                castProgress = 0f;
                isCasting = false;
                UpdateCastBarVisual(0f);
            }
        }
    }

    void CastSpell(SpellData spell)
    {
        if (Camera.main == null) return;

        Vector3 mouseScreen = Vector3.zero;
        if (Mouse.current != null)
        {
            Vector2 m = Mouse.current.position.ReadValue();
            mouseScreen = new Vector3(m.x, m.y, 0f);
        }
        else
        {
            try { mouseScreen = Input.mousePosition; } catch { }
        }

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = transform.position.z;
        Vector3 dir = (mouseWorld - transform.position).normalized;
        if (dir.sqrMagnitude < 0.001f) dir = Vector3.right;

        // Clone SpellData so damage multiplier applies to this cast without modifying asset permanently
        var runtimeSpell = ScriptableObject.Instantiate(spell);
        if (stats != null)
            runtimeSpell.damage *= stats.GetDamageMultiplier();

        var proj = Instantiate(runtimeSpell.projectilePrefab, transform.position, Quaternion.identity);
        var behavior = proj.GetComponent<ISpellBehavior>();
        behavior?.Fire(dir, runtimeSpell);

        if (playerAnimator != null)
            playerAnimator.SetTrigger("Attack");
    }

    // ── Cast-bar visual built entirely in code ─────────────────────────────

    void BuildCastBar()
    {
        var existing = transform.Find("CastBarBG");
        if (existing != null)
        {
            var f = existing.Find("CastBarFill");
            if (f != null) castBarFill = f;
            return;
        }

        var barBG = new GameObject("CastBarBG");
        barBG.transform.SetParent(transform, false);
        barBG.transform.localPosition = new Vector3(0f, 0.75f, 0f);

        var bgSR = barBG.AddComponent<SpriteRenderer>();
        bgSR.sprite = CreateFlatSprite();
        bgSR.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);
        bgSR.sortingOrder = 10;
        barBG.transform.localScale = new Vector3(1f, 0.12f, 1f);

        var fill = new GameObject("CastBarFill");
        fill.transform.SetParent(barBG.transform, false);
        fill.transform.localPosition = new Vector3(-0.5f, 0f, -0.01f);

        var fillSR = fill.AddComponent<SpriteRenderer>();
        fillSR.sprite = bgSR.sprite;
        fillSR.color = new Color(0.2f, 0.7f, 1f, 1f);
        fillSR.sortingOrder = 11;
        fill.transform.localScale = new Vector3(0f, 1f, 1f);
        castBarFill = fill.transform;
    }

    void UpdateCastBarVisual(float t)
    {
        if (castBarFill == null) return;
        var s = castBarFill.localScale;
        s.x = t;
        castBarFill.localScale = s;
        var p = castBarFill.localPosition;
        p.x = (t - 1f) * 0.5f;
        castBarFill.localPosition = p;
    }

    static Sprite CreateFlatSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}