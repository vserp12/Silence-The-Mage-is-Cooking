using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float baseMaxHealth = 100f;
    public float maxHealth = 100f;
    private float currentHealth;

    public Transform healthBarFill;
    private Vector3 originalBarScale;
    private PlayerStats stats;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
        if (stats == null) stats = gameObject.AddComponent<PlayerStats>();
    }

    void Start()
    {
        maxHealth = baseMaxHealth + (stats != null ? stats.GetMaxHealthBonus() : 0f);
        currentHealth = maxHealth;

        if (healthBarFill != null)
        {
            originalBarScale = healthBarFill.localScale;
        }

        UpdateHealthBar();
    }

    public void OnMaxHealthUpgraded(float bonus)
    {
        maxHealth = baseMaxHealth + bonus;
        // Also restore 25 health when upgraded
        Heal(25f);
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthBar();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        UpdateHealthBar();
        Debug.Log("Vida del jugador: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            float healthPercent = Mathf.Clamp01(currentHealth / Mathf.Max(maxHealth, 1f));
            healthBarFill.localScale = new Vector3(
                originalBarScale.x * healthPercent,
                originalBarScale.y,
                originalBarScale.z
            );
        }
    }

    void Die()
    {
        Time.timeScale = 0f;
        if (Camera.main != null && Camera.main.GetComponent<AnimatedDeathScreen>() == null)
            Camera.main.gameObject.AddComponent<AnimatedDeathScreen>();
    }
}