// Common interface implemented by all player spell prefab root components.
// SpellCaster instantiates the prefab and calls Fire to initialize it.
public interface ISpellBehavior
{
    void Fire(UnityEngine.Vector3 direction, SpellData data);
}
