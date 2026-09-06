using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Electricity level-4: hits first enemy then chains to nearby enemies.
public class ChainLightningProjectile : MonoBehaviour, ISpellBehavior
{
    private float damage;
    private float speed;
    private float chainRadius = 5f;
    private int chainsLeft = 3;
    private HashSet<Enemy> alreadyHit = new HashSet<Enemy>();
    private float lifetime = 4f;
    private Vector3 direction;

    public void Fire(Vector3 dir, SpellData data)
    {
        direction = dir.normalized;
        speed = data.projectileSpeed;
        damage = data.damage;
        chainRadius = data.impactRadius > 0f ? data.impactRadius : 5f;

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
        if (nearest == null) { Destroy(gameObject); yield break; }

        direction = (nearest.transform.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
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
