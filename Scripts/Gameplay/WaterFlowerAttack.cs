using UnityEngine;

// Water level-4: spawns N petals in a flower ring around the caster, flying outward in all directions.
public class WaterFlowerAttack : MonoBehaviour, ISpellBehavior
{
    public int petalCount = 8;
    public GameObject petalPrefab; // Water3 petal

    public void Fire(Vector3 direction, SpellData data)
    {
        if (petalPrefab == null)
        {
            petalPrefab = Resources.Load<GameObject>("Spells/Water3") ??
                          Resources.Load<GameObject>("Water3");
            #if UNITY_EDITOR
            if (petalPrefab == null)
                petalPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Spells/Water3.prefab");
            #endif
        }

        if (petalPrefab == null)
        {
            Destroy(gameObject);
            return;
        }

        float angleStep = 360f / petalCount;
        for (int i = 0; i < petalCount; i++)
        {
            float deg = i * angleStep;
            float rad = deg * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
            Vector3 spawnPos = transform.position + dir * 0.35f;

            Quaternion rot = Quaternion.Euler(0f, 0f, deg - 90f);
            var petal = Instantiate(petalPrefab, spawnPos, rot);
            var proj = petal.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Setup(dir, data.projectileSpeed > 0 ? data.projectileSpeed : 8f, data.damage, data.projectileVisuals);
            }
        }

        Destroy(gameObject);
    }
}
