using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WaveState { Idle, InitialSpellSelection, SpawningWave, WaveActive, Intermission }

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [Header("Enemy Prefabs")]
    public GameObject elfMeleePrefab;
    public GameObject elfMagicPrefab;
    public GameObject santaPrefab;

    [Header("Spawn Settings")]
    public Transform player;
    public float spawnRadius = 8f;
    public float spawnDelay = 0.35f;

    [Header("Intermission Timing")]
    public float intermissionDuration = 20f;

    private int currentWave = 0;
    private int enemiesAlive = 0;
    private WaveState state = WaveState.Idle;
    private RuntimeSpellSelection spellUI;

    public WaveState State => state;
    public int CurrentWave => currentWave;
    public int EnemiesAlive => enemiesAlive;

    void Awake()
    {
        Instance = this;
        ResolveReferences();
    }

    void Start()
    {
        StartCoroutine(DelayedStart());
    }

    void ResolveReferences()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        #if UNITY_EDITOR
        if (elfMeleePrefab == null)
            elfMeleePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/ElfMelee.prefab");
        if (elfMagicPrefab == null)
            elfMagicPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/ElfMagic.prefab");
        if (santaPrefab == null)
            santaPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Santa.prefab");
        #endif

        if (elfMeleePrefab == null) elfMeleePrefab = Resources.Load<GameObject>("Enemies/ElfMelee") ?? Resources.Load<GameObject>("ElfMelee");
        if (elfMagicPrefab == null) elfMagicPrefab = Resources.Load<GameObject>("Enemies/ElfMagic") ?? Resources.Load<GameObject>("ElfMagic");
        if (santaPrefab == null)    santaPrefab    = Resources.Load<GameObject>("Enemies/Santa")    ?? Resources.Load<GameObject>("Santa");

        spellUI = GetComponent<RuntimeSpellSelection>() ?? gameObject.AddComponent<RuntimeSpellSelection>();
    }

    IEnumerator DelayedStart()
    {
        yield return null; // wait one frame for all scene objects to initialize
        EnterInitialSpellSelection();
    }

    // ── Initial Selection ──────────────────────────────────────────────────

    void EnterInitialSpellSelection()
    {
        state = WaveState.InitialSpellSelection;
        if (spellUI == null) spellUI = GetComponent<RuntimeSpellSelection>() ?? gameObject.AddComponent<RuntimeSpellSelection>();

        spellUI.ShowInitialSelection(chosenElem =>
        {
            spellUI.Hide();
            int lvl = PlayerSpellInventory.Instance != null ? PlayerSpellInventory.Instance.SelectElement(chosenElem) : 1;
            FindObjectOfType<SpellCaster>()?.SetSpellByElement(chosenElem, lvl);
            StartCoroutine(RunWave(1));
        });
    }

    // ── Spawning and Waves ──────────────────────────────────────────────────

    public void OnEnemySpawned()
    {
        enemiesAlive++;
    }

    public void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null || player == null) return;
        float angle = Random.Range(0f, Mathf.PI * 2f);
        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * spawnRadius;
        Instantiate(prefab, player.position + offset, Quaternion.identity);
        enemiesAlive++;
    }

    // Called by Enemy when it dies
    public void EnemyDied()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        if (state == WaveState.WaveActive && enemiesAlive <= 0)
        {
            StartCoroutine(DelayedIntermission());
        }
    }

    IEnumerator RunWave(int waveNum)
    {
        currentWave = waveNum;
        state = WaveState.SpawningWave;
        enemiesAlive = 0;

        var spawns = BuildWaveSpawns(currentWave);
        foreach (var prefab in spawns)
        {
            SpawnEnemy(prefab);
            yield return new WaitForSeconds(spawnDelay);
        }

        state = WaveState.WaveActive;

        // In case an enemy died instantly
        if (enemiesAlive <= 0)
        {
            StartCoroutine(DelayedIntermission());
        }
    }

    IEnumerator DelayedIntermission()
    {
        yield return new WaitForSeconds(1f); // let death animations finish
        if (enemiesAlive > 0) yield break;

        EnterIntermission();
    }

    void EnterIntermission()
    {
        state = WaveState.Intermission;
        if (spellUI == null) spellUI = GetComponent<RuntimeSpellSelection>() ?? gameObject.AddComponent<RuntimeSpellSelection>();

        spellUI.ShowIntermission(
            clearedWave: currentWave,
            duration: intermissionDuration,
            onPick: chosenElem =>
            {
                int lvl = PlayerSpellInventory.Instance != null ? PlayerSpellInventory.Instance.SelectElement(chosenElem) : 1;
                FindObjectOfType<SpellCaster>()?.SetSpellByElement(chosenElem, lvl);
            },
            onStartWave: () =>
            {
                StartCoroutine(RunWave(currentWave + 1));
            }
        );
    }

    // ── Wave Composition ───────────────────────────────────────────────────

    List<GameObject> BuildWaveSpawns(int wave)
    {
        var list = new List<GameObject>();

        int totalElves = 2 + wave * 2;
        int magicCount = wave / 2;
        int meleeCount = Mathf.Max(1, totalElves - magicCount);

        // First Santa boss at Wave 5, then every 5 waves (wave 5, 10, 15...)
        int santaCount = (wave % 5 == 0) ? (wave / 5) : 0;

        // Interleave melee and magic so spawns are varied
        int mAdded = 0, mgAdded = 0;
        while (mAdded < meleeCount || mgAdded < magicCount)
        {
            if (mAdded < meleeCount && elfMeleePrefab != null)
            {
                list.Add(elfMeleePrefab);
                mAdded++;
            }
            if (mgAdded < magicCount && elfMagicPrefab != null)
            {
                list.Add(elfMagicPrefab);
                mgAdded++;
            }
        }

        for (int i = 0; i < santaCount; i++)
        {
            if (santaPrefab != null) list.Add(santaPrefab);
        }

        return list;
    }
}