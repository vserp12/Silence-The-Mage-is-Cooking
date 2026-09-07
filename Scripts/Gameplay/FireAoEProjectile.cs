using UnityEngine;

// Fire level-4: swirling fireball that pulses and explodes in an area on impact.
public class FireAoEProjectile : MonoBehaviour, ISpellBehavior
{
    public float baseScale = 0.5f;
    private Vector3 direction;
    private float speed;
    private float damage;
    private float radius = 2f;
    private float lifetime = 5f;
    private SpriteRenderer sr;

    public void Fire(Vector3 dir, SpellData data)
    {
        direction = dir.normalized;
        speed = data.projectileSpeed > 0 ? data.projectileSpeed : 8f;
        damage = data.damage;
        radius = data.impactRadius > 0 ? data.impactRadius : 2f;

        sr = GetComponent<SpriteRenderer>();
        if (sr != null && data.projectileVisuals != null)
        {
            if (data.projectileVisuals.sprite != null) sr.sprite = data.projectileVisuals.sprite;
            sr.color = data.projectileVisuals.color;
        }

        transform.localScale = Vector3.one * baseScale;
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        // Fiery swirl & pulse
        transform.Rotate(0f, 0f, 240f * Time.deltaTime);
        float pulse = baseScale * (1f + Mathf.Sin(Time.time * 10f) * 0.12f);
        transform.localScale = new Vector3(pulse, pulse, 1f);

        lifetime -= Time.deltaTime;
        if (lifetime <= 0f) Explode();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        Explode();
    }

    void Explode()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, Mathf.Max(radius, 1.2f));
        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<Enemy>();
            if (enemy != null) enemy.TakeDamage(damage);
        }

        // Spawn quick fading explosion visual
        var boom = new GameObject("FireExplosion");
        boom.transform.position = transform.position;
        var boomSR = boom.AddComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            boomSR.sprite = sr.sprite;
            boomSR.color = new Color(1f, 0.4f, 0.1f, 0.8f);
        }
        boom.transform.localScale = Vector3.one * (radius * 0.8f);
        Destroy(boom, 0.25f);

        Destroy(gameObject);
    }
}
