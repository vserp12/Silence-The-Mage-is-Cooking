using UnityEngine;

// Esta línea hace que aparezca en el menú de crear de Unity
[CreateAssetMenu(fileName = "NuevoHechizo", menuName = "Silence/Spell Data")]
public class SpellData : ScriptableObject
{
    public string spellName = "Nombre del Hechizo";
    public float castTime = 1f;       // Segundos que tarda en lanzarse
    public float damage = 10f;        // Daño que hace
    public float projectileSpeed = 10f; // Velocidad del proyectil
    public ProjectileVisuals projectileVisuals;
    public GameObject projectilePrefab;
}   