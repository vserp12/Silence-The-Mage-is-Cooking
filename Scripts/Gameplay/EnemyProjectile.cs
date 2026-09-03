using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float damage;
    private float lifetime = 5f;

    public void Setup(Vector3 dir, float spd, float dmg, ProjectileVisuals visuals)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;

        if (visuals != null)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (visuals.sprite != null) sr.sprite = visuals.sprite;
                sr.color = visuals.color;
            }

            Animator anim = GetComponent<Animator>();
            if (anim != null && visuals.animatorController != null)
                anim.runtimeAnimatorController = visuals.animatorController;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        lifetime -= Time.deltaTime;
        if (lifetime <= 0) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
