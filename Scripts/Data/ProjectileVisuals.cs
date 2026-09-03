using UnityEngine;

[CreateAssetMenu(fileName = "NuevoProyectil", menuName = "Silence/Projectile Visuals")]
public class ProjectileVisuals : ScriptableObject
{
    public Sprite sprite;
    public RuntimeAnimatorController animatorController;
    public Color color = Color.white;
}
