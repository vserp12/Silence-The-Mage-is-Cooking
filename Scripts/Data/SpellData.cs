using UnityEngine;

[CreateAssetMenu(fileName = "NuevoHechizo", menuName = "Silence/Spell Data")]
public class SpellData : ScriptableObject
{
    public string spellName = "Nuevo Hechizo";
    public ElementType element;
    public int level = 1;
    public float castTime = 1f;
    public float damage = 10f;
    public float projectileSpeed = 10f;
    public ProjectileVisuals projectileVisuals;
    public GameObject projectilePrefab;
    // Optional secondary prefab spawned on impact (spread/area effects)
    public GameObject impactPrefab;
    public float impactRadius = 1.5f;
}   