using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Electricity level-4: hits first enemy then chains with lightning sparks to nearby enemies.
public class ChainLightningProjectile : MonoBehaviour, ISpellBehavior
{
    public float baseScale = 0.45f;
    private float damage;
    private float speed;
    private float chainRadius = 6f;
    private int chainsLeft = 4;
    private HashSet<Enemy> alreadyHit = new HashSet<Enemy>();
    private float lifetime = 5f;
    private Vector3 direction;
    private SpriteRenderer sr;
    private Color baseColor = new Color(1f, 1f, 0.2f, 1f);

    public void Fire(Vector3 dir, SpellData data)
    {
        direction = dir.normalized;
        speed = data.projectileSpeed > 0 ? data.projectileSpeed : 12f;
        damage = data.damage;
        chainRadius = data.impactRadius > 0f ? data.impactRadius : 6f;

        sr = GetComponent<SpriteRenderer>();
        if (sr != null && data.projectileVisuals != null)
        {
            if (data.projectileVisuals.sprite != null) sr.sprite = data.projectileVisuals.sprite;
            if (data.projectileVisuals.color != Color.clear) baseColor = data.projectileVisuals.color;
            sr.color = baseColor;
        }

        transform.localScale = Vector3.one * baseScale;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        // Electric crackle jitter & flash
        float flicker = Random.Range(0.8f, 1.2f);
        float scaleJitter = baseScale * Random.Range(0.9f, 1.15f);
        transform.localScale = new Vector3(scaleJitter, scaleJitter, 1f);

        if (sr != null)
        {
            sr.color = Random.value > 0.4f
                ? baseColor * flicker
                : new Color(0.6f, 0.9f, 1f, 1f);
        }

        lifetime -= Time.deltaTime;
        if (lifetime <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var enemy = other.GetComponent<Enemy>();
        if (enemy == null || alreadyHit.Contains(enemy)) return;

        alreadyHit.Add(enemy);
        enemy.TakeDamage(damage);

        if (chainsLeft > 0)
        {
            chainsLeft--;
            StartCoroutine(ChainToNext());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator ChainToNext()
    {
        yield return null; // one frame delay so hit registers
        Enemy nearest = FindNearestUnhit();
        if (nearest == null)
        {
            Destroy(gameObject);
            yield break;
        }

        direction = (nearest.transform.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        speed *= 1.15f; // speed up on subsequent chain leaps
    }

    Enemy FindNearestUnhit()
    {
        float best = chainRadius;
        Enemy result = null;
        foreach (var col in Physics2D.OverlapCircleAll(transform.position, chainRadius))
        {
            var e = col.GetComponent<Enemy>();
            if (e == null || alreadyHit.Contains(e)) continue;
            float d = Vector2.Distance(transform.position, e.transform.position);
            if (d < best) { best = d; result = e; }
        }
        return result;
    }
}
