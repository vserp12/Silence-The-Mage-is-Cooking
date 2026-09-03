using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float maxHealth = 30f;
    private float currentHealth;

    public float moveSpeed = 3f;
    public float attackRange = 1f;
    public float damage = 10f;
    public float attackCooldown = 1f;

    public Transform healthBarFill;
    public GameObject healthBarContainer;
    private Vector3 originalBarScale;

    private Transform player;
    private float lastAttackTime = 0f;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBarFill != null)
        {
            originalBarScale = healthBarFill.localScale;
        }

        UpdateHealthBar();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player != null)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer < attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
            }
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            float healthPercent = Mathf.Clamp01(currentHealth / maxHealth);
            healthBarFill.localScale = new Vector3(
                originalBarScale.x * healthPercent,
                originalBarScale.y,
                originalBarScale.z
            );
        }
    }

    void Attack()
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
        lastAttackTime = Time.time;
    }

    void Die()
    {
        gameObject.SetActive(false);
    }
}