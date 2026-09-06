using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

/// Tools > Setup Player (Issue #3)
/// Creates player animation clips, a PlayerAnimator controller, and adds
/// CharacterBob + PlayerSpellInventory to the player GameObject in the active scene.
public static class PlayerSetup
{
    [MenuItem("Tools/Setup Player (Issue #3)")]
    public static void SetupAll()
    {
        EnsureFolders();

        // ── Load sprites ──────────────────────────────────────────────────
        var idleSprites  = LoadSprites("Assets/Sprites/player-and-dead-player.png");
        var attackSprites = LoadSprites("Assets/Sprites/player-staff-with-animations.png");

        // doblepila_0, _1 = idle frames; _2, _3 = dead frames
        Sprite idle0 = Get(idleSprites, "doblepila_0");
        Sprite idle1 = Get(idleSprites, "doblepila_1");
        Sprite dead0 = Get(idleSprites, "doblepila_2");
        Sprite dead1 = Get(idleSprites, "doblepila_3");

        // pixil-frame-0 (3)_0 .. _10 = attack frames
        Sprite[] attackFrames = Enumerable.Range(0, 11)
            .Select(i => Get(attackSprites, $"pixil-frame-0 (3)_{i}"))
            .Where(s => s != null).ToArray();

        // ── Animation clips ───────────────────────────────────────────────
        var idleClip   = SaveClip(SpriteClip("PlayerIdle",   new[] { idle0, idle1 }, 4,  loop: true),
                                  "Assets/Animations/Player/PlayerIdle.anim");
        var attackClip = SaveClip(SpriteClip("PlayerAttack", attackFrames,           12, loop: false),
                                  "Assets/Animations/Player/PlayerAttack.anim");
        var deadClip   = SaveClip(SpriteClip("PlayerDead",   new[] { dead0, dead1 }, 4,  loop: false),
                                  "Assets/Animations/Player/PlayerDead.anim");

        // ── Animator controller ───────────────────────────────────────────
        BuildPlayerController(idleClip, attackClip, deadClip);

        // ── Patch player GameObject in scene ─────────────────────────────
        PatchPlayerInScene();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[PlayerSetup] Done — player animations and components set up.");
    }

    static AnimatorController BuildPlayerController(AnimationClip idle, AnimationClip attack, AnimationClip dead)
    {
        const string path = "Assets/Animations/Player/PlayerAnimator.controller";
        AnimatorController ctrl;
        var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (existing != null)
            ctrl = existing;
        else
            ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);

        AddParam(ctrl, "Attack", AnimatorControllerParameterType.Trigger);
        AddParam(ctrl, "isDead", AnimatorControllerParameterType.Bool);

        var sm = ctrl.layers[0].stateMachine;
        var idleS   = AddOrUpdateState(sm, "Idle",   idle);
        var attackS = AddOrUpdateState(sm, "Attack", attack);
        var deadS   = AddOrUpdateState(sm, "Dead",   dead);
        sm.defaultState = idleS;

        AddAnyTransitionIfMissing(sm, attackS, "Attack", AnimatorConditionMode.If,    0f, 0.05f);
        AddExitTransitionIfMissing(attackS, idleS, 1f, 0.1f);
        AddAnyTransitionIfMissing(sm, deadS,   "isDead", AnimatorConditionMode.If,    0f, 0.1f);

        EditorUtility.SetDirty(ctrl);
        return ctrl;
    }

    static void PatchPlayerInScene()
    {
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(
            "Assets/Animations/Player/PlayerAnimator.controller");

        // Find the player by tag
        var players = GameObject.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        if (players.Length == 0) { Debug.LogWarning("[PlayerSetup] No PlayerMovement found in scene."); return; }

        foreach (var pm in players)
        {
            var go = pm.gameObject;

            // Animator
            var anim = go.GetComponent<Animator>();
            if (anim == null) anim = go.AddComponent<Animator>();
            if (ctrl != null) anim.runtimeAnimatorController = ctrl;

            // CharacterBob
            if (go.GetComponent<CharacterBob>() == null)
                go.AddComponent<CharacterBob>();

            // PlayerSpellInventory
            if (go.GetComponent<PlayerSpellInventory>() == null)
                go.AddComponent<PlayerSpellInventory>();

            EditorUtility.SetDirty(go);
            Debug.Log($"[PlayerSetup] Patched: {go.name}");
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Animations"))
            AssetDatabase.CreateFolder("Assets", "Animations");
        if (!AssetDatabase.IsValidFolder("Assets/Animations/Player"))
            AssetDatabase.CreateFolder("Assets/Animations", "Player");
    }

    static Sprite[] LoadSprites(string path) =>
        AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();

    static Sprite Get(Sprite[] sprites, string name) =>
        sprites.FirstOrDefault(s => s.name == name);

    static AnimationClip SpriteClip(string clipName, Sprite[] frames, float fps, bool loop)
    {
        var clip    = new AnimationClip { name = clipName, frameRate = fps };
        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        var keys    = new ObjectReferenceKeyframe[frames.Length];
        for (int i = 0; i < frames.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i] };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime  = loop;
        settings.stopTime  = frames.Length / fps;
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

    static void AddParam(AnimatorController ctrl, string name, AnimatorControllerParameterType type)
    {
        if (!ctrl.parameters.Any(p => p.name == name)) ctrl.AddParameter(name, type);
    }

    static AnimatorState AddOrUpdateState(AnimatorStateMachine sm, string name, Motion motion)
    {
        foreach (var cs in sm.states)
        {
            if (cs.state.name != name) continue;
            cs.state.motion = motion;
            cs.state.writeDefaultValues = false;
            return cs.state;
        }
        var s = sm.AddState(name);
        s.motion = motion;
        s.writeDefaultValues = false;
        return s;
    }

    static void AddAnyTransitionIfMissing(AnimatorStateMachine sm, AnimatorState to,
        string param, AnimatorConditionMode mode, float threshold, float duration)
    {
        if (sm.anyStateTransitions.Any(t => t.destinationState == to)) return;
        var t2 = sm.AddAnyStateTransition(to);
        t2.AddCondition(mode, threshold, param);
        t2.duration = duration;
        t2.hasExitTime = false;
        t2.canTransitionToSelf = false;
    }

    static void AddExitTransitionIfMissing(AnimatorState from, AnimatorState to,
        float exitTime, float duration)
    {
        if (from.transitions.Any(t => t.destinationState == to && t.hasExitTime)) return;
        var t2 = from.AddTransition(to);
        t2.hasExitTime = true;
        t2.exitTime    = exitTime;
        t2.duration    = duration;
    }
}
