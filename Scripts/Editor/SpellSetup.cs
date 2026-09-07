using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

/// Tools > Setup Spells (Issue #4)
/// Creates every spell animation clip, animator controller, prefab,
/// ProjectileVisuals SO, SpellData SO, and the SpellDatabase SO.
public static class SpellSetup
{
    [MenuItem("Tools/Setup Spells (Issue #4)")]
    public static void SetupAll()
    {
        EnsureFolders();

        // ── Build all spell assets ─────────────────────────────────────────
        var water1  = MakeBasicSpell("Water1",    "water-lvl-1.png",                 "water-lvl-1",        6,  ElementType.Water,       1, 0.8f, 15f, 12f, 0.35f, Color.cyan);
        var water2  = MakeBasicSpell("Water2",    "water-lvl-2-with-spread.png",     "pixil-frame-2",     21,  ElementType.Water,       2, 1.0f, 25f, 11f, 0.40f, Color.cyan);
        var water3  = MakeSpreadSpell("Water3",   "water-lvl-3.png",                 "pixil-frame-5",      1,
                                                  "water-lvl-3-spread.png",          "pixil-frame-9",     27,
                                                  ElementType.Water, 3, 1.2f, 40f, 10f, 0.70f, Color.cyan);
        var water4  = MakeFlowerSpell("Water4",   ElementType.Water, 4, 1.5f, 20f);

        var fire1   = MakeBasicSpell("Fire1",     "fire-lvl-1.png",                  "julinaserrano",     14,  ElementType.Fire,        1, 0.7f, 15f, 14f, 0.35f, new Color(1f, 0.4f, 0f));
        var fire2   = MakeBasicSpell("Fire2",     "fire-lvl-2-with-spread.png",      "pixil-frame-3",     11,  ElementType.Fire,        2, 0.9f, 30f, 12f, 0.40f, new Color(1f, 0.4f, 0f));
        var fire3   = MakeBasicSpell("Fire3",     "fire-lvl-3.png",                  "pixil-frame-6",      4,  ElementType.Fire,        3, 1.1f, 50f, 10f, 0.45f, new Color(1f, 0.2f, 0f));
        var fire4   = MakeAoESpell("Fire4",       "fire-lvl-4.png",                  "wachin_0",
                                                  ElementType.Fire, 4, 1.5f, 80f, 4f, 2.0f, new Color(1f, 0.1f, 0f));

        var elec1   = MakeSingleFrameSpell("Elec1", "electricity-lvl-1.png",         "pixil-frame-0_0",
                                                  ElementType.Electricity, 1, 0.5f, 12f, 18f, 0.35f, Color.yellow);
        var elec2   = MakeBasicSpell("Elec2",    "electricity-lvl-2-with-spread.png","pixil-frame-4",     14,  ElementType.Electricity, 2, 0.7f, 25f, 16f, 0.40f, Color.yellow);
        var elec3   = MakeSpreadSpell("Elec3",   "electricity-lvl-3.png",             "pixil-frame-7",     1,
                                                  "electricity-lvl-3-spread.png",    "pixil-frame-8",     93,
                                                  ElementType.Electricity, 3, 1.0f, 45f, 15f, 0.70f, Color.yellow);
        var elec4   = MakeChainSpell("Elec4",    "electricity-lvl-4.png",            "wacho_0",
                                                  ElementType.Electricity, 4, 1.3f, 35f, 18f, 5f, Color.yellow);

        var light   = MakeBasicSpell("Light1",   "light-sprite-sheet.png",           "pixil-frame-1",      3,  ElementType.Light,       1, 0.4f, 20f, 20f, 0.35f, Color.white);

        // ── Populate SpellDatabase ─────────────────────────────────────────
        BuildSpellDatabase(water1, water2, water3, water4,
                           fire1,  fire2,  fire3,  fire4,
                           elec1,  elec2,  elec3,  elec4,
                           light);

        // ── Add CharacterBob + WeaponSwing to enemy prefabs ───────────────
        PatchEnemyPrefabs();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[SpellSetup] Done — all spell assets created.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Spell factory methods
    // ─────────────────────────────────────────────────────────────────────────

    // Standard animated projectile (uses Projectile.cs + ISpellBehavior)
    static SpellData MakeBasicSpell(string id, string sheetPath, string framePrefix, int frameCount,
        ElementType elem, int level, float castTime, float damage, float speed, float scale, Color tint)
    {
        var sprites = LoadFrames(sheetPath, framePrefix, frameCount);
        var clip    = SaveClip(SpriteClip($"{id}Fly", sprites, 12, loop: true),
                               $"Assets/Animations/Spells/{id}Fly.anim");
        var ctrl    = BuildSingleStateController($"Assets/Animations/Spells/{id}Ctrl.controller", id, clip);

        var visuals = MakeVisuals(id, sprites.FirstOrDefault(), tint, ctrl);

        var prefab  = MakeProjectilePrefab($"Assets/Prefabs/Spells/{id}.prefab", visuals, typeof(Projectile), scale);
        return MakeSpellData(id, elem, level, castTime, damage, speed, visuals, prefab, null, 0f);
    }

    // Projectile with impact effect that plays a spread animation + area damage
    static SpellData MakeSpreadSpell(string id,
        string flySheet, string flyPrefix, int flyFrames,
        string spreadSheet, string spreadPrefix, int spreadFrames,
        ElementType elem, int level, float castTime, float damage, float speed, float radius, Color tint)
    {
        // Fly prefab
        var flySprites  = LoadFrames(flySheet, flyPrefix, flyFrames);
        var flyClip     = SaveClip(SpriteClip($"{id}Fly", flySprites, 10, loop: true),
                                    $"Assets/Animations/Spells/{id}Fly.anim");
        var flyCtrl     = BuildSingleStateController($"Assets/Animations/Spells/{id}FlyCtrl.controller", id + "Fly", flyClip);
        var visuals     = MakeVisuals(id, flySprites.FirstOrDefault(), tint, flyCtrl);

        // Impact prefab
        var spreadSprites = LoadFrames(spreadSheet, spreadPrefix, spreadFrames);
        var spreadClip    = SaveClip(SpriteClip($"{id}Impact", spreadSprites, 18, loop: false),
                                      $"Assets/Animations/Spells/{id}Impact.anim");
        var spreadCtrl    = BuildSingleStateController($"Assets/Animations/Spells/{id}ImpactCtrl.controller", id + "Impact", spreadClip);
        var impactPrefab  = MakeImpactPrefab($"Assets/Prefabs/Spells/{id}Impact.prefab", spreadSprites.FirstOrDefault(), tint, spreadCtrl, radius);

        var flyPrefab = MakeProjectilePrefab($"Assets/Prefabs/Spells/{id}.prefab", visuals, typeof(SpreadImpact), 1f);
        return MakeSpellData(id, elem, level, castTime, damage, speed, visuals, flyPrefab, impactPrefab, radius);
    }

    // Water level-4 flower attack
    static SpellData MakeFlowerSpell(string id, ElementType elem, int level, float castTime, float damage)
    {
        // Re-use Water-3 visuals as petal
        var water3Petal = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Spells/Water3.prefab");
        var water3Vis   = AssetDatabase.LoadAssetAtPath<ProjectileVisuals>("Assets/ScriptableObjects/Spells/Water3Visuals.asset");

        var prefabPath = $"Assets/Prefabs/Spells/{id}.prefab";
        GameObject prefab;
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existing == null)
        {
            var go = new GameObject(id);
            var flower = go.AddComponent<WaterFlowerAttack>();
            flower.petalCount = 6;
            prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
        }
        else prefab = existing;

        // Wire petal prefab after the prefab exists (EditPrefabContentsScope)
        using var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath);
        var root = scope.prefabContentsRoot;
        var w = root.GetComponent<WaterFlowerAttack>();
        if (w != null) w.petalPrefab = water3Petal;

        return MakeSpellData(id, elem, level, castTime, damage, 8f, water3Vis, prefab, null, 0f);
    }

    // Fire level-4 AoE fireball
    static SpellData MakeAoESpell(string id, string sheetPath, string spriteName,
        ElementType elem, int level, float castTime, float damage, float speed, float radius, Color tint)
    {
        var sprite = LoadSingleSprite(sheetPath, spriteName);
        var visuals = MakeVisuals(id, sprite, tint, null);
        var prefab  = MakeProjectilePrefab($"Assets/Prefabs/Spells/{id}.prefab", visuals, typeof(FireAoEProjectile), 1.5f);
        return MakeSpellData(id, elem, level, castTime, damage, speed, visuals, prefab, null, radius);
    }

    // Electricity level-1 (single-frame, fast)
    static SpellData MakeSingleFrameSpell(string id, string sheetPath, string spriteName,
        ElementType elem, int level, float castTime, float damage, float speed, float scale, Color tint)
    {
        var sprite  = LoadSingleSprite(sheetPath, spriteName);
        var visuals = MakeVisuals(id, sprite, tint, null);
        var prefab  = MakeProjectilePrefab($"Assets/Prefabs/Spells/{id}.prefab", visuals, typeof(Projectile), scale);
        return MakeSpellData(id, elem, level, castTime, damage, speed, visuals, prefab, null, 0f);
    }

    // Electricity level-4 chain lightning
    static SpellData MakeChainSpell(string id, string sheetPath, string spriteName,
        ElementType elem, int level, float castTime, float damage, float speed, float chainRadius, Color tint)
    {
        var sprite  = LoadSingleSprite(sheetPath, spriteName);
        var visuals = MakeVisuals(id, sprite, tint, null);
        var prefab  = MakeProjectilePrefab($"Assets/Prefabs/Spells/{id}.prefab", visuals, typeof(ChainLightningProjectile), 1f);
        return MakeSpellData(id, elem, level, castTime, damage, speed, visuals, prefab, null, chainRadius);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Asset creation helpers
    // ─────────────────────────────────────────────────────────────────────────

    static GameObject MakeProjectilePrefab(string path, ProjectileVisuals visuals,
        System.Type behaviorType, float scale)
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null)
        {
            // Just ensure the behavior component is present
            using var s = new PrefabUtility.EditPrefabContentsScope(path);
            EnsureComponent(s.prefabContentsRoot, behaviorType);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        var go = new GameObject(System.IO.Path.GetFileNameWithoutExtension(path));
        go.transform.localScale = Vector3.one * scale;

        var sr = go.AddComponent<SpriteRenderer>();
        if (visuals != null)
        {
            if (visuals.sprite != null) sr.sprite = visuals.sprite;
            sr.color = visuals.color;
        }

        if (visuals?.animatorController != null)
        {
            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = visuals.animatorController;
        }

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.25f;

        go.AddComponent(behaviorType);

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject MakeImpactPrefab(string path, Sprite sprite, Color tint,
        RuntimeAnimatorController ctrl, float radius)
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var go = new GameObject(System.IO.Path.GetFileNameWithoutExtension(path));
        go.transform.localScale = Vector3.one * radius;

        var sr = go.AddComponent<SpriteRenderer>();
        if (sprite != null) sr.sprite = sprite;
        sr.color = tint;

        if (ctrl != null)
        {
            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = ctrl;
        }

        go.AddComponent<DestroyAfterAnimation>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static ProjectileVisuals MakeVisuals(string id, Sprite sprite, Color tint, RuntimeAnimatorController ctrl)
    {
        const string dir = "Assets/ScriptableObjects/Spells/";
        string path = $"{dir}{id}Visuals.asset";
        var existing = AssetDatabase.LoadAssetAtPath<ProjectileVisuals>(path);
        if (existing != null)
        {
            existing.sprite = sprite;
            existing.color  = tint;
            existing.animatorController = ctrl;
            EditorUtility.SetDirty(existing);
            return existing;
        }
        var v = ScriptableObject.CreateInstance<ProjectileVisuals>();
        v.sprite             = sprite;
        v.color              = tint;
        v.animatorController = ctrl;
        AssetDatabase.CreateAsset(v, path);
        return v;
    }

    static SpellData MakeSpellData(string id, ElementType elem, int level,
        float castTime, float damage, float speed,
        ProjectileVisuals visuals, GameObject prefab, GameObject impactPrefab, float radius)
    {
        const string dir = "Assets/ScriptableObjects/Spells/";
        string path = $"{dir}{id}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<SpellData>(path);
        SpellData sd;
        if (existing != null) { sd = existing; }
        else { sd = ScriptableObject.CreateInstance<SpellData>(); AssetDatabase.CreateAsset(sd, path); }

        sd.spellName         = $"{elem} Level {level}";
        sd.element           = elem;
        sd.level             = level;
        sd.castTime          = castTime;
        sd.damage            = damage;
        sd.projectileSpeed   = speed;
        sd.projectileVisuals = visuals;
        sd.projectilePrefab  = prefab;
        sd.impactPrefab      = impactPrefab;
        sd.impactRadius      = radius;
        EditorUtility.SetDirty(sd);
        return sd;
    }

    static AnimatorController BuildSingleStateController(string path, string stateName, AnimationClip clip)
    {
        var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (existing != null)
        {
            var s = AddOrUpdateState(existing.layers[0].stateMachine, stateName, clip);
            existing.layers[0].stateMachine.defaultState = s;
            EditorUtility.SetDirty(existing);
            return existing;
        }
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);
        var sm   = ctrl.layers[0].stateMachine;
        var st   = sm.AddState(stateName);
        st.motion = clip;
        sm.defaultState = st;
        EditorUtility.SetDirty(ctrl);
        return ctrl;
    }

    static void BuildSpellDatabase(
        SpellData w1, SpellData w2, SpellData w3, SpellData w4,
        SpellData f1, SpellData f2, SpellData f3, SpellData f4,
        SpellData e1, SpellData e2, SpellData e3, SpellData e4,
        SpellData light)
    {
        const string path = "Assets/ScriptableObjects/SpellDatabase.asset";
        var db = AssetDatabase.LoadAssetAtPath<SpellDatabase>(path);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<SpellDatabase>();
            AssetDatabase.CreateAsset(db, path);
        }

        db.waterSpells       = new[] { w1, w2, w3, w4 };
        db.fireSpells        = new[] { f1, f2, f3, f4 };
        db.electricitySpells = new[] { e1, e2, e3, e4 };
        db.lightSpell        = light;
        EditorUtility.SetDirty(db);

        // Also save to Resources so SpellCaster can load it at runtime without inspector wiring
        EnsureFolder("Assets/Resources");
        const string resPath = "Assets/Resources/SpellDatabase.asset";
        if (AssetDatabase.LoadAssetAtPath<SpellDatabase>(resPath) == null)
            AssetDatabase.CopyAsset(path, resPath);

        // Wire SpellDatabase into SpellCaster in the scene
        var casters = Object.FindObjectsByType<SpellCaster>(FindObjectsSortMode.None);
        foreach (var c in casters)
        {
            c.spellDatabase = db;
            EditorUtility.SetDirty(c);
        }
    }

    // Add CharacterBob and WeaponSwing to enemy prefabs
    static void PatchEnemyPrefabs()
    {
        string[] enemyPrefabPaths = {
            "Assets/Prefabs/Enemies/ElfMelee.prefab",
            "Assets/Prefabs/Enemies/ElfMagic.prefab",
            "Assets/Prefabs/Enemies/Santa.prefab"
        };

        foreach (var path in enemyPrefabPaths)
        {
            if (!System.IO.File.Exists(path)) continue;
            using var scope = new PrefabUtility.EditPrefabContentsScope(path);
            var root = scope.prefabContentsRoot;
            EnsureComponent(root, typeof(CharacterBob));

            // WeaponSwing on the Weapon child (if present)
            var weapon = root.transform.Find("Weapon");
            if (weapon != null)
                EnsureComponent(weapon.gameObject, typeof(WeaponSwing));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Sprite helpers
    // ─────────────────────────────────────────────────────────────────────────

    static Sprite[] LoadFrames(string sheetPath, string prefix, int count)
    {
        var all = AssetDatabase.LoadAllAssetsAtPath($"Assets/Sprites/{sheetPath}")
                               .OfType<Sprite>().ToArray();
        return Enumerable.Range(0, count)
                         .Select(i => all.FirstOrDefault(s => s.name == $"{prefix}_{i}"))
                         .Where(s => s != null)
                         .ToArray();
    }

    static Sprite LoadSingleSprite(string sheetPath, string spriteName) =>
        AssetDatabase.LoadAllAssetsAtPath($"Assets/Sprites/{sheetPath}")
                     .OfType<Sprite>()
                     .FirstOrDefault(s => s.name == spriteName);

    static AnimationClip SpriteClip(string clipName, Sprite[] frames, float fps, bool loop)
    {
        if (frames == null || frames.Length == 0)
        {
            Debug.LogWarning($"[SpellSetup] No frames for clip '{clipName}'");
            return new AnimationClip { name = clipName };
        }
        var clip    = new AnimationClip { name = clipName, frameRate = fps };
        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        var keys    = new ObjectReferenceKeyframe[frames.Length];
        for (int i = 0; i < frames.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i] };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        settings.stopTime = frames.Length / fps;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        return clip;
    }

    static AnimationClip SaveClip(AnimationClip clip, string path)
    {
        var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (existing != null) { EditorUtility.CopySerialized(clip, existing); return existing; }
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    static AnimatorState AddOrUpdateState(AnimatorStateMachine sm, string name, Motion motion)
    {
        foreach (var cs in sm.states)
            if (cs.state.name == name) { cs.state.motion = motion; return cs.state; }
        var s = sm.AddState(name); s.motion = motion; return s;
    }

    static void EnsureFolders()
    {
        EnsureFolder("Assets/Animations");
        EnsureFolder("Assets/Animations/Spells");
        EnsureFolder("Assets/Prefabs/Spells");
        EnsureFolder("Assets/ScriptableObjects");
        EnsureFolder("Assets/ScriptableObjects/Spells");
    }

    static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            int last = path.LastIndexOf('/');
            AssetDatabase.CreateFolder(path[..last], path[(last + 1)..]);
        }
    }

    static void EnsureComponent(GameObject go, System.Type type)
    {
        if (go.GetComponent(type) == null)
            go.AddComponent(type);
    }
}
