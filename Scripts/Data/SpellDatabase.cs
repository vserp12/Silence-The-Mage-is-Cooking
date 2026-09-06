using UnityEngine;

[CreateAssetMenu(fileName = "SpellDatabase", menuName = "Silence/Spell Database")]
public class SpellDatabase : ScriptableObject
{
    public SpellData[] waterSpells;        // indices 0-3 = levels 1-4
    public SpellData[] fireSpells;
    public SpellData[] electricitySpells;
    public SpellData lightSpell;           // single prefab; levels change stats only

    public SpellData GetSpell(ElementType element, int level)
    {
        level = Mathf.Clamp(level, 1, 4);
        switch (element)
        {
            case ElementType.Water:
                return (waterSpells != null && level <= waterSpells.Length) ? waterSpells[level - 1] : null;
            case ElementType.Fire:
                return (fireSpells != null && level <= fireSpells.Length) ? fireSpells[level - 1] : null;
            case ElementType.Electricity:
                return (electricitySpells != null && level <= electricitySpells.Length) ? electricitySpells[level - 1] : null;
            case ElementType.Light:
                return lightSpell;
        }
        return null;
    }
}
