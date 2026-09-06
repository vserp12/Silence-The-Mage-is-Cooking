using UnityEngine;

// Manages upgradable stats: HP, Speed, Damage, Cooldown Reduction
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    public int hpLevel = 0;
    public int speedLevel = 0;
    public int damageLevel = 0;
    public int cooldownLevel = 0;

    public const int MaxLevel = 5;

    public int availableUpgradePoints = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(this);
    }

    public void AwardWavePoints(int points = 1)
    {
        availableUpgradePoints += points;
    }

    // ── Stat Multipliers & Values ──────────────────────────────────────────

    public float GetMaxHealthBonus() => hpLevel * 25f;

    public float GetSpeedMultiplier() => 1f + speedLevel * 0.15f;

    public float GetDamageMultiplier() => 1f + damageLevel * 0.20f;

    public float GetCooldownMultiplier() => Mathf.Max(0.4f, 1f - cooldownLevel * 0.12f);

    // ── Upgrades ───────────────────────────────────────────────────────────

    public bool UpgradeHP()
    {
        if (hpLevel >= MaxLevel || availableUpgradePoints <= 0) return false;
        hpLevel++;
        availableUpgradePoints--;

        var health = GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.OnMaxHealthUpgraded(GetMaxHealthBonus());
        }
        return true;
    }

    public bool UpgradeSpeed()
    {
        if (speedLevel >= MaxLevel || availableUpgradePoints <= 0) return false;
        speedLevel++;
        availableUpgradePoints--;
        return true;
    }

    public bool UpgradeDamage()
    {
        if (damageLevel >= MaxLevel || availableUpgradePoints <= 0) return false;
        damageLevel++;
        availableUpgradePoints--;
        return true;
    }

    public bool UpgradeCooldown()
    {
        if (cooldownLevel >= MaxLevel || availableUpgradePoints <= 0) return false;
        cooldownLevel++;
        availableUpgradePoints--;
        return true;
    }

    public void ResetStats()
    {
        hpLevel = 0;
        speedLevel = 0;
        damageLevel = 0;
        cooldownLevel = 0;
        availableUpgradePoints = 0;
    }
}
