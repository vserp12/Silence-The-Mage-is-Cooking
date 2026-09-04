using UnityEngine;

public enum EnemyAttackType { Melee, Ranged }

public class Enemy : MonoBehaviour
{
    public float maxHealth = 30f;
    private float currentHealth;

    public float moveSpeed = 3f;
    public float attackRange = 1f;
    public float damage = 10f;
    public float attackCooldown = 1f;

    public EnemyAttackType attackType = EnemyAttackType.Melee;
    public GameObject projectilePrefab;
    public float projectileSpeed = 8f;
    public ProjectileVisuals projectileVisuals;

    public Sprite bodySprite;
    public Sprite weaponSprite;
    public Color bodyColor = Color.white;
    public Vector2 weaponOffset = new Vector2(0.4f, -0.2f);

    public Transform healthBarFill;
    public GameObject healthBarContainer;
    private Vector3 originalBarScale;

    protected Transform player;
    protected float lastAttackTime = 0f;
    protected Animator animator;

    void Start()
    {
        currentHealth = maxHealth;

        animator = GetComponent<Animator>();

        SpriteRenderer bodySR = GetComponent<SpriteRenderer>();
        if (bodySR != null)
        {
            bodySR.color = bodyColor;
            if (bodySprite != null) bodySR.sprite = bodySprite;
        }

        if (weaponSprite != null)
        {
            GameObject weaponObj = new GameObject("Weapon");
            weaponObj.transform.SetParent(transform);
            weaponObj.transform.localPosition = (Vector3)weaponOffset;
            SpriteRenderer weaponSR = weaponObj.AddComponent<SpriteRenderer>();
            weaponSR.sprite = weaponSprite;
            weaponSR.sortingOrder = (bodySR != null ? bodySR.sortingOrder : 0) + 1;
        }

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
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool isMoving = attackType == EnemyAttackType.Melee || distanceToPlayer > attackRange;

        if (isMoving)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );
        }

        if (animator != null)
            animator.SetBool("isMoving", isMoving);

        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
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

    protected virtual void Attack()
    {
        lastAttackTime = Time.time;

        if (animator != null)
            animator.SetTrigger("Attack");

        if (attackType == EnemyAttackType.Melee)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null) playerHealth.TakeDamage(damage);
        }
        else
        {
            ShootProjectileAtPlayer();
        }
    }

    protected void ShootProjectileAtPlayer()
    {
        if (projectilePrefab == null || player == null) return;
        Vector3 dir = (player.position - transform.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
        if (ep != null) ep.Setup(dir, projectileSpeed, damage, projectileVisuals);
    }

    void Die()
    {
        if (animator != null)
            animator.SetBool("isDead", true);
        gameObject.SetActive(false);
    }
}