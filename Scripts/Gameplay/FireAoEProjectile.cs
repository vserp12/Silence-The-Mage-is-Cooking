using UnityEngine;

// Fire level-4: slow-moving large fireball that explodes in an area on impact.
public class FireAoEProjectile : MonoBehaviour, ISpellBehavior
{
    private Vector3 direction;
    private float speed;
    private float damage;
    private float radius;
    private float lifetime = 6f;

    public void Fire(Vector3 dir, SpellData data)
    {
        direction = dir.normalized;
        speed = data.projectileSpeed;
        damage = data.damage;
        radius = data.impactRadius;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && data.projectileVisuals != null)
        {
            if (data.projectileVisuals.sprite != null) sr.sprite = data.projectileVisuals.sprite;
            sr.color = data.projectileVisuals.color;
        }

        // Slow, large visual
        transform.localScale = Vector3.one * 1.5f;
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        // Slow pulse for visual flair
        float pulse = 1.5f + Mathf.Sin(Time.time * 6f) * 0.1f;
        transform.localScale = Vector3.one * pulse;

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
        var hits = Physics2D.OverlapCircleAll(transform.position, Mathf.Max(radius, 1f));
        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<Enemy>();
            if (enemy != null) enemy.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
