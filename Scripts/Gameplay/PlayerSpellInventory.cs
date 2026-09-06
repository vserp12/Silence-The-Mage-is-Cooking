using System.Collections.Generic;
using UnityEngine;

// Tracks which elements the player owns and at what level (max 4).
public class PlayerSpellInventory : MonoBehaviour
{
    public static PlayerSpellInventory Instance;

    private readonly Dictionary<ElementType, int> levels = new Dictionary<ElementType, int>();
    private ElementType activeElement = ElementType.Water;

    void Awake()
    {
        Instance = this;
    }

    // Returns the new level after selection.
    public int SelectElement(ElementType element)
    {
        if (levels.ContainsKey(element))
            levels[element] = Mathf.Min(levels[element] + 1, 4);
        else
            levels[element] = 1;

        activeElement = element;
        return levels[element];
    }

    public int GetLevel(ElementType element) =>
        levels.TryGetValue(element, out int lvl) ? lvl : 0;

    public ElementType ActiveElement => activeElement;
    public int ActiveLevel => GetLevel(activeElement);
}
