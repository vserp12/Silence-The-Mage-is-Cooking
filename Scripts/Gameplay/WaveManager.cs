using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WaveState { Idle, SpellSelection, SpawningWave, WaveActive, WaveCooldown }

// ── Editor-only: auto-wires WaveManager + SpellCaster every time Play is pressed ──
#if UNITY_EDITOR
[UnityEditor.InitializeOnLoadMethod]
static class WaveManagerPlayHook
{
    static WaveManagerPlayHook()
    {
        UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChange;
    }

    static void OnPlayModeChange(UnityEditor.PlayModeStateChange state)
    {
        if (state != UnityEditor.PlayModeStateChange.ExitingEditMode) return;

        var wm = Object.FindFirstObjectByType<WaveManager>();
        if (wm != null)
        {
            Wire(ref wm.elfMeleePrefab, "Assets/Prefabs/Enemies/ElfMelee.prefab");
            Wire(ref wm.elfMagicPrefab, "Assets/Prefabs/Enemies/ElfMagic.prefab");
            Wire(ref wm.santaPrefab,    "Assets/Prefabs/Enemies/Santa.prefab");
        }

        var sc = Object.FindFirstObjectByType<SpellCaster>();
        if (sc != null && sc.spellDatabase == null)
        {
            var db = UnityEditor.AssetDatabase.LoadAssetAtPath<SpellDatabase>(
                "Assets/ScriptableObjects/SpellDatabase.asset");
            if (db != null) sc.spellDatabase = db;
        }
    }

    static void Wire(ref GameObject field, string path)
    {
        if (field != null) return;
        field = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }
}
#endif
// ──────────────────────────────────────────────────────────────────────────────

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [Header("Enemy Prefabs")]
    public GameObject enemyPrefab;
    public GameObject elfMeleePrefab;
    public GameObject elfMagicPrefab;
    public GameObject santaPrefab;

    [Header("Spawn Settings")]
    public Transform player;
    public float spawnRadius = 8f;
    public float spawnDelay = 0.4f;     // seconds between each individual spawn

    [Header("Wave Timing")]
    public float cooldownDuration = 5f;

    [Header("UI")]
    public SpellSelectionUI spellSelectionUI;

    private int currentWave = 0;
    private int enemiesAlive = 0;
    private WaveState state = WaveState.Idle;

    public WaveState State => state;
    public int CurrentWave => currentWave;

    void Awake()
    {
        Instance = this;
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (spellSelectionUI == null)
            spellSelectionUI = FindObjectOfType<SpellSelectionUI>();
    }

    void Start()
    {
        // Delay one frame so all Awake() calls (including GameBootstrap) complete first
        StartCoroutine(DelayedInit());
    }

    IEnumerator DelayedInit()
    {
        yield return null;
        if (spellSelectionUI == null)
            spellSelectionUI = FindObjectOfType<SpellSelectionUI>();
        EnterSpellSelection();
    }

    // ── Public API ─────────────────────────────────────────────────────────

    public void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null || player == null) return;
        float angle = Random.Range(0f, Mathf.PI * 2f);
        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnRadius;
        Instantiate(prefab, player.position + offset, Quaternion.identity);
        enemiesAlive++;
    }

    // Called by Enemy when it dies
    public void EnemyDied()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        if (state == WaveState.WaveActive && enemiesAlive == 0)
            StartCoroutine(DelayedWaveClear());
    }

    // Called by SpellSelectionUI once the player confirms a spell choice
    public void OnSpellSelected()
    {
        if (spellSelectionUI != null) spellSelectionUI.Hide();
        StartCoroutine(RunWave());
    }

    // ── State Machine ──────────────────────────────────────────────────────

    void EnterSpellSelection()
    {
        state = WaveState.SpellSelection;
        if (spellSelectionUI != null)
        {
            spellSelectionUI.Show(currentWave);
        }
        else
        {
            // Create an OnGUI-based selection that needs no scene setup
            var rss = GetComponent<RuntimeSpellSelection>() ?? gameObject.AddComponent<RuntimeSpellSelection>();
            rss.Show(currentWave, elem =>
            {
                rss.Hide();
                int lvl = PlayerSpellInventory.Instance?.SelectElement(elem) ?? 1;
                FindObjectOfType<SpellCaster>()?.SetSpellByElement(elem, lvl);
                OnSpellSelected();
            });
        }
    }

    IEnumerator RunWave()
    {
        currentWave++;
        state = WaveState.SpawningWave;
        enemiesAlive = 0;

        foreach (var (prefab, count) in BuildWaveSpawns(currentWave))
        {
            for (int i = 0; i < count; i++)
            {
                SpawnEnemy(prefab);
                yield return new WaitForSeconds(spawnDelay);
            }
        }

        state = WaveState.WaveActive;
        // Edge case: if all enemies somehow died during spawning
        if (enemiesAlive == 0)
            StartCoroutine(DelayedWaveClear());
    }

    IEnumerator DelayedWaveClear()
    {
        yield return new WaitForSeconds(1.2f); // let death animations play
        if (enemiesAlive > 0) yield break;     // more enemies spawned meanwhile
        state = WaveState.WaveCooldown;
        yield return new WaitForSeconds(cooldownDuration);
        EnterSpellSelection();
    }

    // ── Procedural wave composition ────────────────────────────────────────

    List<(GameObject prefab, int count)> BuildWaveSpawns(int wave)
    {
        var list = new List<(GameObject, int)>();

        int totalElves = 2 + wave * 2;
        int magicCount = wave / 2;
        int meleeCount = totalElves - magicCount;
        int santaCount = Mathf.Max(0, (wave - 4) / 5); // first Santa at wave 5

        if (elfMeleePrefab != null && meleeCount > 0) list.Add((elfMeleePrefab, meleeCount));
        if (elfMagicPrefab != null && magicCount > 0)  list.Add((elfMagicPrefab, magicCount));
        if (santaPrefab != null && santaCount > 0)     list.Add((santaPrefab, santaCount));

        return list;
    }
}