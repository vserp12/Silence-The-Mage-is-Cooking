using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    public Transform healthBarFill;
    private Vector3 originalBarScale;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBarFill != null)
        {
            originalBarScale = healthBarFill.localScale;
        }

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
            float healthPercent = Mathf.Clamp01(currentHealth / maxHealth);
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

        // Attach the animated death screen to the main camera (OnGUI, no scene setup needed)
        if (Camera.main != null && Camera.main.GetComponent<AnimatedDeathScreen>() == null)
            Camera.main.gameObject.AddComponent<AnimatedDeathScreen>();

        // Also trigger legacy GameOverUI if it's in the scene
        GameOverUI gameOver = FindObjectOfType<GameOverUI>();
        if (gameOver != null)
            gameOver.ShowGameOver();
    }
}