using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

/// <summary>
/// Run via Tools > Setup Enemies (Issue #1) to create all animation clips,
/// animator controllers, and wire up the enemy prefabs.
/// </summary>
public static class EnemySetup
{
    [MenuItem("Tools/Setup Enemies (Issue #1)")]
    public static void SetupAll()
    {
        EnsureFolders();

        // ── Load sprites ──────────────────────────────────────────────────────
        var elfSprites = LoadSprites("Assets/Sprites/elfs-and-weapons.png");
        var santaSprites = LoadSprites("Assets/Sprites/Santa-and-staff.png");
        var summonSprites = LoadSprites("Assets/Sprites/santa-spawn-attack.png");
        var projSprites = LoadSprites("Assets/Sprites/enemy_atacks.png");

        Sprite elfBody0 = Get(elfSprites, "mediapila_0");
        Sprite elfBody1 = Get(elfSprites, "mediapila_1");
        Sprite rakeSprite = Get(elfSprites, "mediapila_2");
        Sprite staffSprite = Get(elfSprites, "mediapila_4");

        Sprite santaIdle0 = Get(santaSprites, "chacarera_0");
        Sprite santaIdle1 = Get(santaSprites, "chacarera_1");
        // Frames 2-6: ice crosier swinging right; 7-11: left
        Sprite[] magicFrames = Range(santaSprites, "chacarera_", 2, 5);
        Sprite crosierStatic = Get(santaSprites, "chacarera_2");

        Sprite[] summonFrames = Range(summonSprites, "cuartapila_", 2, 6);

        Sprite elfProjSprite = Get(projSprites, "enemy_atacks_0");
        Sprite santaProjSprite = Get(projSprites, "enemy_atacks_1");

        // ── Animation clips ───────────────────────────────────────────────────
        var elfIdle = SaveClip(MakeSpriteClip("ElfIdle", new[] { elfBody0, elfBody1 }, 4, loop: true),
            "Assets/Animations/Enemies/ElfIdle.anim");

        var elfWalk = SaveClip(MakeSpriteClip("ElfWalk", new[] { elfBody0, elfBody1 }, 8, loop: true),
            "Assets/Animations/Enemies/ElfWalk.anim");

        var santaIdle = SaveClip(MakeSpriteClip("SantaIdle", new[] { santaIdle0, santaIdle1 }, 2, loop: true),
            "Assets/Animations/Enemies/SantaIdle.anim");

        var santaMagic = SaveClip(MakeSpriteClip("SantaMagicAttack", magicFrames, 10, loop: false),
            "Assets/Animations/Enemies/SantaMagicAttack.anim");

        var santaSummon = SaveClip(MakeSpriteClip("SantaSummonAttack", summonFrames, 6, loop: false),
            "Assets/Animations/Enemies/SantaSummonAttack.anim");

        // ── Animator controllers ──────────────────────────────────────────────
        var elfCtrl = BuildElfController(elfIdle, elfWalk);
        var santaCtrl = BuildSantaController(santaIdle, santaMagic, santaSummon);

        // ── Configure enemy prefabs ───────────────────────────────────────────
        ConfigureElfMelee(elfBody0, rakeSprite, elfCtrl);
        ConfigureElfMagic(elfBody0, staffSprite, elfCtrl);
        ConfigureSanta(santaIdle0, crosierStatic, santaCtrl);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[EnemySetup] Done — all enemies set up for Issue #1.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Animation helpers
    // ─────────────────────────────────────────────────────────────────────────

    static AnimationClip MakeSpriteClip(string clipName, Sprite[] frames, float fps, bool loop)
    {
        var clip = new AnimationClip { name = clipName, frameRate = fps };
        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");

        var keys = new ObjectReferenceKeyframe[frames.Length];
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
        if (existing != null)
        {
            EditorUtility.CopySerialized(clip, existing);
            return existing;
        }
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Animator controller builders
    // ─────────────────────────────────────────────────────────────────────────

    static AnimatorController BuildElfController(AnimationClip idle, AnimationClip walk)
    {
        string path = "Assets/Prefabs/Enemies/ElfAnimator.controller";
        var ctrl = GetOrCreateController(path, "ElfAnimator");

        AddParam(ctrl, "isMoving", AnimatorControllerParameterType.Bool);
        AddParam(ctrl, "Attack", AnimatorControllerParameterType.Trigger);
        AddParam(ctrl, "isDead", AnimatorControllerParameterType.Bool);

        var sm = ctrl.layers[0].stateMachine;
        var idleS = AddOrUpdateState(sm, "Idle", idle);
        var walkS = AddOrUpdateState(sm, "Walk", walk);
        var atkS = AddOrUpdateState(sm, "Attack", idle);
        var deadS = AddOrUpdateState(sm, "Death", idle);
        sm.defaultState = idleS;

        AddTransitionIfMissing(idleS, walkS, "isMoving", AnimatorConditionMode.If, 0, 0.1f, false);
        AddTransitionIfMissing(walkS, idleS, "isMoving", AnimatorConditionMode.IfNot, 0, 0.1f, false);
        AddAnyTransitionIfMissing(sm, atkS, "Attack", AnimatorConditionMode.If, 0, 0.05f, false);
        AddExitTransitionIfMissing(atkS, idleS, 1f, 0.1f);
        AddAnyTransitionIfMissing(sm, deadS, "isDead", AnimatorConditionMode.If, 0, 0.1f, false);

        EditorUtility.SetDirty(ctrl);
        return ctrl;
    }

    static AnimatorController BuildSantaController(AnimationClip idle, AnimationClip magic, AnimationClip summon)
    {
        string path = "Assets/Prefabs/Enemies/SantaAnimator.controller";
        var ctrl = GetOrCreateController(path, "SantaAnimator");

        AddParam(ctrl, "isMoving", AnimatorControllerParameterType.Bool);
        AddParam(ctrl, "Attack", AnimatorControllerParameterType.Trigger);
        AddParam(ctrl, "SummonAttack", AnimatorControllerParameterType.Trigger);
        AddParam(ctrl, "isDead", AnimatorControllerParameterType.Bool);

        var sm = ctrl.layers[0].stateMachine;
        var idleS = AddOrUpdateState(sm, "Idle", idle);
        var walkS = AddOrUpdateState(sm, "Walk", idle);
        var magicS = AddOrUpdateState(sm, "MagicAttack", magic);
        var summonS = AddOrUpdateState(sm, "SummonAttack", summon);
        var deadS = AddOrUpdateState(sm, "Death", idle);
        sm.defaultState = idleS;

        AddTransitionIfMissing(idleS, walkS, "isMoving", AnimatorConditionMode.If, 0, 0.1f, false);
        AddTransitionIfMissing(walkS, idleS, "isMoving", AnimatorConditionMode.IfNot, 0, 0.1f, false);
        AddAnyTransitionIfMissing(sm, magicS, "Attack", AnimatorConditionMode.If, 0, 0.05f, false);
        AddAnyTransitionIfMissing(sm, summonS, "SummonAttack", AnimatorConditionMode.If, 0, 0.05f, false);
        AddExitTransitionIfMissing(magicS, idleS, 1f, 0.1f);
        AddExitTransitionIfMissing(summonS, idleS, 1f, 0.1f);
        AddAnyTransitionIfMissing(sm, deadS, "isDead", AnimatorConditionMode.If, 0, 0.1f, false);

        EditorUtility.SetDirty(ctrl);
        return ctrl;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Prefab configurators
    // ─────────────────────────────────────────────────────────────────────────

    static void ConfigureElfMelee(Sprite body, Sprite weapon, AnimatorController ctrl)
    {
        string path = "Assets/Prefabs/Enemies/ElfMelee.prefab";
        using var scope = new PrefabUtility.EditPrefabContentsScope(path);
        var root = scope.prefabContentsRoot;
        SetBodySprite(root, body);
        var enemy = root.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.bodySprite = body;
            enemy.weaponSprite = weapon;
            enemy.weaponOffset = new Vector2(0.25f, -0.1f);
            enemy.attackType = EnemyAttackType.Melee;
        }
        SetAnimatorController(root, ctrl);
    }

    static void ConfigureElfMagic(Sprite body, Sprite weapon, AnimatorController ctrl)
    {
        string path = "Assets/Prefabs/Enemies/ElfMagic.prefab";
        var elfProj = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectiles/ElfProjectile.prefab");
        using var scope = new PrefabUtility.EditPrefabContentsScope(path);
        var root = scope.prefabContentsRoot;
        SetBodySprite(root, body);
        var enemy = root.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.bodySprite = body;
            enemy.weaponSprite = weapon;
            enemy.weaponOffset = new Vector2(0.3f, 0.05f);
            enemy.attackType = EnemyAttackType.Ranged;
            enemy.attackRange = 6f;
            enemy.projectilePrefab = elfProj;
            enemy.projectileSpeed = 7f;
        }
        SetAnimatorController(root, ctrl);
    }

    static void ConfigureSanta(Sprite body, Sprite crosier, AnimatorController ctrl)
    {
        string path = "Assets/Prefabs/Enemies/Santa.prefab";
        var santaProj = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectiles/SantaProjectile.prefab");
        var elfMelee = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/ElfMelee.prefab");
        var elfMagic = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/ElfMagic.prefab");

        using var scope = new PrefabUtility.EditPrefabContentsScope(path);
        var root = scope.prefabContentsRoot;
        SetBodySprite(root, body);

        var santa = root.GetComponent<Santa>();
        if (santa != null)
        {
            santa.bodySprite = body;
            santa.weaponSprite = crosier;
            santa.weaponOffset = new Vector2(0.55f, 0.1f);
            santa.attackType = EnemyAttackType.Ranged;
            santa.attackRange = 7f;
            santa.projectilePrefab = santaProj;
            santa.projectileSpeed = 6f;
            santa.elfMeleePrefab = elfMelee;
            santa.elfMagicPrefab = elfMagic;
        }
        SetAnimatorController(root, ctrl);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Generic helpers
    // ─────────────────────────────────────────────────────────────────────────

    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Animations"))
            AssetDatabase.CreateFolder("Assets", "Animations");
        if (!AssetDatabase.IsValidFolder("Assets/Animations/Enemies"))
            AssetDatabase.CreateFolder("Assets/Animations", "Enemies");
    }

    static Sprite[] LoadSprites(string assetPath) =>
        AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>().ToArray();

    static Sprite Get(Sprite[] sprites, string name) =>
        sprites.FirstOrDefault(s => s.name == name);

    static Sprite[] Range(Sprite[] sprites, string prefix, int start, int count) =>
        Enumerable.Range(start, count).Select(i => Get(sprites, $"{prefix}{i}")).Where(s => s != null).ToArray();

    static AnimatorController GetOrCreateController(string path, string name)
    {
        var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (existing != null) return existing;
        return AnimatorController.CreateAnimatorControllerAtPath(path);
    }

    static void AddParam(AnimatorController ctrl, string paramName, AnimatorControllerParameterType type)
    {
        if (!ctrl.parameters.Any(p => p.name == paramName))
            ctrl.AddParameter(paramName, type);
    }

    static AnimatorState AddOrUpdateState(AnimatorStateMachine sm, string stateName, Motion motion)
    {
        foreach (var cs in sm.states)
            if (cs.state.name == stateName) { cs.state.motion = motion; return cs.state; }
        return sm.AddState(stateName, motion);
    }

    static void AddTransitionIfMissing(AnimatorState from, AnimatorState to,
        string param, AnimatorConditionMode mode, float threshold, float duration, bool hasExit)
    {
        if (from.transitions.Any(t => t.destinationState == to)) return;
        var t2 = from.AddTransition(to);
        t2.AddCondition(mode, threshold, param);
        t2.duration = duration;
        t2.hasExitTime = hasExit;
    }

    static void AddAnyTransitionIfMissing(AnimatorStateMachine sm, AnimatorState to,
        string param, AnimatorConditionMode mode, float threshold, float duration, bool hasExit)
    {
        if (sm.anyStateTransitions.Any(t => t.destinationState == to)) return;
        var t2 = sm.AddAnyStateTransition(to);
        t2.AddCondition(mode, threshold, param);
        t2.duration = duration;
        t2.hasExitTime = hasExit;
        t2.canTransitionToSelf = false;
    }

    static void AddExitTransitionIfMissing(AnimatorState from, AnimatorState to,
        float exitTime, float duration)
    {
        if (from.transitions.Any(t => t.destinationState == to && t.hasExitTime)) return;
        var t2 = from.AddTransition(to);
        t2.hasExitTime = true;
        t2.exitTime = exitTime;
        t2.duration = duration;
    }

    static void SetBodySprite(GameObject root, Sprite sprite)
    {
        var sr = root.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sprite = sprite;
    }

    static void SetAnimatorController(GameObject root, AnimatorController ctrl)
    {
        var anim = root.GetComponent<Animator>();
        if (anim != null) anim.runtimeAnimatorController = ctrl;
    }

    // Helper overload — state without explicit position
    static AnimatorState AddState(this AnimatorStateMachine sm, string name, Motion motion)
    {
        var state = sm.AddState(name);
        state.motion = motion;
        return state;
    }
}
