using UnityEngine;

// Wires all inspector references at runtime so the game works even if the scene
// was never saved after running the editor setup scripts.
// GameSceneSetup creates this object and bakes the asset references into it.
public class GameBootstrap : MonoBehaviour
{
    [Header("Baked in by GameSceneSetup")]
    public GameObject elfMeleePrefab;
    public GameObject elfMagicPrefab;
    public GameObject santaPrefab;
    public SpellDatabase spellDatabase;

    void Awake()
    {
        WireWaveManager();
        WireSpellCaster();
        EnsurePlayerComponents();
    }

    void WireWaveManager()
    {
        var wm = FindObjectOfType<WaveManager>();
        if (wm == null) return;

        if (elfMeleePrefab != null && wm.elfMeleePrefab == null) wm.elfMeleePrefab = elfMeleePrefab;
        if (elfMagicPrefab != null && wm.elfMagicPrefab == null) wm.elfMagicPrefab = elfMagicPrefab;
        if (santaPrefab != null    && wm.santaPrefab    == null) wm.santaPrefab    = santaPrefab;
    }

    void WireSpellCaster()
    {
        var sc = FindObjectOfType<SpellCaster>();
        if (sc == null || spellDatabase == null) return;
        if (sc.spellDatabase == null) sc.spellDatabase = spellDatabase;
    }

    // Add components the player needs even if PlayerSetup was never run
    void EnsurePlayerComponents()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        if (player.GetComponent<CharacterBob>() == null)         player.AddComponent<CharacterBob>();
        if (player.GetComponent<PlayerSpellInventory>() == null) player.AddComponent<PlayerSpellInventory>();
    }
}
