using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float damage;
    private float lifetime = 3f; // Se destruye a los 3 segundos si no pega a nada

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
        // Mover en espacio mundial (World Space) para que vaya derecho
        transform.position += direction * speed * Time.deltaTime;
        
        // Destruir si pasa mucho tiempo
        lifetime -= Time.deltaTime;
        if (lifetime <= 0) 
        {
            Destroy(gameObject);
        }
    }

    // Detectar colisión con enemigos
    void OnTriggerEnter2D(Collider2D other)
    {
        // Buscamos si lo que tocamos tiene el script Enemy
        Enemy enemy = other.GetComponent<Enemy>();
        
        if (enemy != null) 
        {
            enemy.TakeDamage(damage); // Le hacemos daño
            Destroy(gameObject);      // Y destruimos el proyectil
        }
    }
}