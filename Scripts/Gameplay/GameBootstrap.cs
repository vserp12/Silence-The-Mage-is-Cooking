using UnityEngine;
using UnityEngine.SceneManagement;

// Automatically initializes and verifies the entire game environment at runtime.
// Guarantees zero manual setup required from the user in the Unity Inspector.
public class GameBootstrap : MonoBehaviour
{
    [Header("Prefabs & Assets")]
    public GameObject elfMeleePrefab;
    public GameObject elfMagicPrefab;
    public GameObject santaPrefab;
    public SpellDatabase spellDatabase;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoBootstrapOnSceneLoad()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.name == "Game")
        {
            var bootstrap = FindObjectOfType<GameBootstrap>();
            if (bootstrap == null)
            {
                var go = new GameObject("GameBootstrap");
                bootstrap = go.AddComponent<GameBootstrap>();
            }
            bootstrap.InitializeAll();
        }
    }

    void Awake()
    {
        InitializeAll();
    }

    public void InitializeAll()
    {
        LoadAssetsIfNull();
        EnsurePlayerComponents();
        EnsureWaveManager();
        EnsureSpellCaster();
        EnsureCameraFollow();
        EnsureMusic();
        Debug.Log("[GameBootstrap] All game systems, prefabs, and components verified successfully.");
    }

    void LoadAssetsIfNull()
    {
        #if UNITY_EDITOR
        if (elfMeleePrefab == null)
            elfMeleePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/ElfMelee.prefab");
        if (elfMagicPrefab == null)
            elfMagicPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/ElfMagic.prefab");
        if (santaPrefab == null)
            santaPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Santa.prefab");
        if (spellDatabase == null)
            spellDatabase = UnityEditor.AssetDatabase.LoadAssetAtPath<SpellDatabase>("Assets/ScriptableObjects/SpellDatabase.asset");
        #endif

        if (elfMeleePrefab == null) elfMeleePrefab = Resources.Load<GameObject>("Enemies/ElfMelee") ?? Resources.Load<GameObject>("ElfMelee");
        if (elfMagicPrefab == null) elfMagicPrefab = Resources.Load<GameObject>("Enemies/ElfMagic") ?? Resources.Load<GameObject>("ElfMagic");
        if (santaPrefab == null)    santaPrefab    = Resources.Load<GameObject>("Enemies/Santa")    ?? Resources.Load<GameObject>("Santa");
        if (spellDatabase == null)  spellDatabase  = Resources.Load<SpellDatabase>("SpellDatabase");
    }

    void EnsurePlayerComponents()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        if (player.GetComponent<PlayerStats>() == null)
            player.AddComponent<PlayerStats>();

        if (player.GetComponent<PlayerHealth>() == null)
            player.AddComponent<PlayerHealth>();

        if (player.GetComponent<CharacterBob>() == null)
            player.AddComponent<CharacterBob>();

        if (player.GetComponent<PlayerSpellInventory>() == null)
            player.AddComponent<PlayerSpellInventory>();

        var caster = player.GetComponent<SpellCaster>();
        if (caster == null) caster = player.AddComponent<SpellCaster>();
        if (spellDatabase != null && caster.spellDatabase == null)
            caster.spellDatabase = spellDatabase;
    }

    void EnsureWaveManager()
    {
        var wm = FindObjectOfType<WaveManager>();
        if (wm == null)
        {
            var go = new GameObject("WaveManager");
            wm = go.AddComponent<WaveManager>();
        }

        if (elfMeleePrefab != null && wm.elfMeleePrefab == null) wm.elfMeleePrefab = elfMeleePrefab;
        if (elfMagicPrefab != null && wm.elfMagicPrefab == null) wm.elfMagicPrefab = elfMagicPrefab;
        if (santaPrefab != null    && wm.santaPrefab    == null) wm.santaPrefab    = santaPrefab;
    }

    void EnsureSpellCaster()
    {
        var sc = FindObjectOfType<SpellCaster>();
        if (sc != null && spellDatabase != null && sc.spellDatabase == null)
        {
            sc.spellDatabase = spellDatabase;
        }
    }

    void EnsureCameraFollow()
    {
        var cam = Camera.main;
        if (cam == null) return;

        var follow = cam.GetComponent<CameraFollow>();
        if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();

        if (follow.target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) follow.target = p.transform;
        }
        follow.clampToBounds = true;
    }

    void EnsureMusic()
    {
        var sm = FindObjectOfType<SceneMusic>();
        if (sm != null && sm.music == null)
        {
            #if UNITY_EDITOR
            sm.music = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/GameMusic.mp3");
            #endif
            if (sm.music == null) sm.music = Resources.Load<AudioClip>("Audio/GameMusic") ?? Resources.Load<AudioClip>("GameMusic");
        }
    }
}
