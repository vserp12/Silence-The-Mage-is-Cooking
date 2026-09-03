using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    public GameObject enemyPrefab;
    public Transform player;
    public float spawnRadius = 8f; // A qué distancia del jugador aparecen

    void Awake()
    {
        Instance = this;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    // Crea 1 enemigo en un punto aleatorio alrededor del jugador
    public void SpawnEnemy()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * spawnRadius;
        Vector3 spawnPos = player.position + offset;

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    // SOLO PARA PROBAR: apretá la tecla T y spawnea un enemigo
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            SpawnEnemy();
        }
    }
}