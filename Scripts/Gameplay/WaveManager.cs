using UnityEngine;
using UnityEngine.InputSystem;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    public GameObject enemyPrefab;
    public GameObject elfMeleePrefab;
    public GameObject elfMagicPrefab;
    public GameObject santaPrefab;

    public Transform player;
    public float spawnRadius = 8f; // A qué distancia del jugador aparecen
    public float timeBetweenWaves = 3f; // Tiempo entre oleadas

    private int currentWave = 0;
    private int enemiesAlive = 0;
    private bool waveInProgress = false;

    void Awake()
    {
        Instance = this;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    // Crea un enemigo con el prefab dado en un punto aleatorio alrededor del jugador
    public void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null) { Debug.LogWarning("WaveManager: prefab no asignado"); return; }
        if (player == null) { Debug.LogWarning("WaveManager: player no encontrado"); return; }

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * spawnRadius;
        Instantiate(prefab, player.position + offset, Quaternion.identity);
    }

    // Mantiene compatibilidad con el prefab genérico
    public void SpawnEnemy() => SpawnEnemy(enemyPrefab);

    // SOLO PARA PROBAR: T = enemigo genérico, E = elfo melee, M = elfo mágico, S = Santa
    void Update()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.tKey.wasPressedThisFrame) SpawnEnemy(enemyPrefab);
        if (Keyboard.current.eKey.wasPressedThisFrame) SpawnEnemy(elfMeleePrefab);
        if (Keyboard.current.mKey.wasPressedThisFrame) SpawnEnemy(elfMagicPrefab);
        if (Keyboard.current.sKey.wasPressedThisFrame) SpawnEnemy(santaPrefab);
    }
}