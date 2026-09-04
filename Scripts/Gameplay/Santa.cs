using UnityEngine;

// Santa: alternates between spawning 3 elves and shooting a magic projectile
public class Santa : Enemy
{
    public GameObject elfMeleePrefab;
    public GameObject elfMagicPrefab;
    public float elfSpawnRadius = 2f;

    private bool spawnElvesNext = true;

    protected override void Attack()
    {
        lastAttackTime = Time.time;

        if (animator != null)
            animator.SetTrigger("Attack");

        if (spawnElvesNext)
            SpawnElves();
        else
            ShootProjectileAtPlayer();

        spawnElvesNext = !spawnElvesNext;
    }

    void SpawnElves()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject prefab = Random.value > 0.5f ? elfMagicPrefab : elfMeleePrefab;
            if (prefab == null) continue;

            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * elfSpawnRadius;
            Instantiate(prefab, transform.position + offset, Quaternion.identity);
        }
    }
}
