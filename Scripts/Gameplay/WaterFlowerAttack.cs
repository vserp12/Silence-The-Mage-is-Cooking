using UnityEngine;

// Water level-4: spawns N petals in a ring around the caster, each flying outward.
// The "direction" parameter is ignored; petals spread in all directions.
public class WaterFlowerAttack : MonoBehaviour, ISpellBehavior
{
    public int petalCount = 6;
    public GameObject petalPrefab; // set by SpellSetup

    public void Fire(Vector3 direction, SpellData data)
    {
        if (petalPrefab == null) { Destroy(gameObject); return; }

        float angleStep = 360f / petalCount;
        for (int i = 0; i < petalCount; i++)
        {
            float rad = (i * angleStep) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
            var petal = Instantiate(petalPrefab, transform.position, Quaternion.identity);
            var proj = petal.GetComponent<Projectile>();
            if (proj != null)
                proj.Setup(dir, data.projectileSpeed, data.damage, data.projectileVisuals);
        }

        Destroy(gameObject);
    }
}
