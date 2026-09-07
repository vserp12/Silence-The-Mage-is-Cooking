using System.Collections;
using UnityEngine;

// Projectile that spawns an impact/spread prefab and deals area damage on hit.
// Used for Water-3, Electricity-3, Fire-4.
public class SpreadImpact : MonoBehaviour, ISpellBehavior
{
    private Vector3 direction;
    private float speed;
    private float damage;
    private float impactRadius;
    private GameObject impactPrefab;
    private float lifetime = 4f;

    public void Fire(Vector3 dir, SpellData data)
    {
        direction = dir.normalized;
        speed = data.projectileSpeed;
        damage = data.damage;
        impactRadius = data.impactRadius;
        impactPrefab = data.impactPrefab;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && data.projectileVisuals != null)
        {
            if (data.projectileVisuals.sprite != null) sr.sprite = data.projectileVisuals.sprite;
            sr.color = data.projectileVisuals.color;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        Explode();
    }

    void Explode()
    {
        // Spawn visual impact
        if (impactPrefab != null)
            Instantiate(impactPrefab, transform.position, Quaternion.identity);

        // Area damage
        var hits = Physics2D.OverlapCircleAll(transform.position, Mathf.Max(impactRadius, 0.5f));
        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<Enemy>();
            if (enemy != null) enemy.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
