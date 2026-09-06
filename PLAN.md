# Silence! The Mage is Cooking — Implementation Plan (updated)

## Issue Status
| # | Title | Status |
|---|-------|--------|
| 1 | Make elf enemies, Santa, and more | ✅ CLOSED (done via EnemySetup) |
| 2 | PR for issue #1 | ✅ MERGED |
| 3 | Making character movement | 🔴 Code written, assets not created yet |
| 4 | Applying actual spells | 🔴 Code written, assets not created yet |
| 5 | Making the waves system | 🟡 Partially working (auto-starts, no UI yet) |

---

## Honest Status of Each Piece

### ✅ Actually working right now (no editor scripts needed)
- WaveManager state machine — now auto-starts a wave after 1 second if SpellSelectionUI is missing
- SpellCaster — can cast if a `fallbackSpell` is assigned in the inspector (add one of the old SpellData SOs)
- PlayerMovement + CharacterBob — bob activates as soon as CharacterBob component is on the player (not on prefab yet)
- Enemy movement, attack, health
- Enemy death → notifies WaveManager
- Elf facing deadzone (no more rapid flip oscillation)
- All spell behavior scripts (SpreadImpact, WaterFlowerAttack, FireAoEProjectile, ChainLightningProjectile)

### ❌ Requires running editor scripts (Tools menu in Unity, Game scene open)
- **Staff not visible** (player + elf): run `Tools > Setup Enemies` then `Tools > Setup Player`
- **CharacterBob on enemies**: run `Tools > Setup Spells`
- **Spell animations + prefabs + SpellDatabase**: run `Tools > Setup Spells`
- **SpellSelectionUI panel in scene**: run `Tools > Setup Game Scene`
- **WaveManager wired to SpellSelectionUI**: run `Tools > Setup Game Scene`

### ⚠️ Known gaps not yet implemented
- Player staff is NOT visible in idle/walk (doblepila sprites don't include staff). Staff only appears during attack animation after PlayerSetup runs.
- No wave number HUD (only the between-wave label in SpellSelectionUI panel).

---

## Editor Script Execution Order (Game scene must be open)
1. **Tools > Setup Enemies (Issue #1)** — creates elf/Santa weapon children with correct sprites
2. **Tools > Setup Player (Issue #3)** — creates player animation clips + adds Animator/CharacterBob/PlayerSpellInventory to player
3. **Tools > Setup Spells (Issue #4)** — creates all 13 spell animations, prefabs, SpellData SOs, SpellDatabase SO; adds CharacterBob+WeaponSwing to enemy prefabs
4. **Tools > Setup Game Scene (Issues #3 #4 #5)** — adds SpellSelectionUI canvas + wires WaveManager
5. Save scene (Ctrl+S)

---

## Issue #3 — Character Movement Animations

**Requirement:** Hop/bounce animation while moving. Weapons animated via spritesheet on attack (except elf weapons which use code).

- `CharacterBob.cs` ✅ written — squash-stretch scale on root transform when moving
- `WeaponSwing.cs` ✅ written — coroutine rotation swing for elf weapon children
- `Enemy.cs` ✅ updated — calls CharacterBob.SetMoving + WeaponSwing.Swing on attack
- `PlayerMovement.cs` ✅ updated — calls CharacterBob.SetMoving
- Player Animator (idle/attack/dead from spritesheets) ❌ needs PlayerSetup to run
- CharacterBob + WeaponSwing on enemy prefabs ❌ needs SpellSetup.PatchEnemyPrefabs to run

---

## Issue #4 — Actual Spells

**Sprite mapping:**
| Spell | Asset | Frames | Status |
|-------|-------|--------|--------|
| Light | `light-sprite-sheet.png` pixil-frame-1_0..2 | 3 | ❌ asset not created |
| Water 1 | `water-lvl-1.png` water-lvl-1_0..5 | 6 | ❌ asset not created |
| Water 2 | `water-lvl-2-with-spread.png` pixil-frame-2_0..20 | 21 | ❌ asset not created |
| Water 3 | `water-lvl-3.png` + spread | 1+27 | ❌ asset not created |
| Water 4 | Flower (6× Water-3 petals in ring) | code | ❌ asset not created |
| Fire 1 | `fire-lvl-1.png` julinaserrano_0..13 | 14 | ❌ asset not created |
| Fire 2 | `fire-lvl-2-with-spread.png` pixil-frame-3_0..10 | 11 | ❌ asset not created |
| Fire 3 | `fire-lvl-3.png` pixil-frame-6_0..3 | 4 | ❌ asset not created |
| Fire 4 | `fire-lvl-4.png` wachin_0 + AOE | 1+code | ❌ asset not created |
| Elec 1 | `electricity-lvl-1.png` pixil-frame-0_0 | 1 | ❌ asset not created |
| Elec 2 | `electricity-lvl-2-with-spread.png` pixil-frame-4_0..13 | 14 | ❌ asset not created |
| Elec 3 | `electricity-lvl-3.png` + spread (93 frames!) | 1+93 | ❌ asset not created |
| Elec 4 | `electricity-lvl-4.png` wacho_0 + chain | 1+code | ❌ asset not created |

SpellSetup.cs ✅ written — creates all the above when run from Tools menu.

---

## Issue #5 — Wave System

**Game loop:**
1. Game loads → WaveManager.Start() → EnterSpellSelection()
2. If SpellSelectionUI present: shows panel, player picks spell, wave starts
3. If SpellSelectionUI absent (fallback): auto-starts wave after 1 second
4. Kill all enemies → 5s cooldown → back to step 2
5. Wave N: `(2 + N×2)` elves, `floor(N/2)` magic, first Santa at wave 5
6. Player dies → GameOverUI shows wave number

**Status:**
- WaveManager state machine ✅ written + null-safe fallback added
- PlayerSpellInventory ✅ written
- SpellSelectionUI ✅ written (needs GameSceneSetup to add to scene)
- MainMenuUI.PlayGame() ✅ fixed (calls NameInputManager)
- GameOverUI ✅ shows wave reached


## Issue Status
| # | Title | Status |
|---|-------|--------|
| 1 | Make elf enemies, Santa, and more | ✅ CLOSED (done) |
| 2 | PR for issue #1 | ✅ MERGED |
| 3 | Making character movement | 🟡 OPEN — needs animation |
| 4 | Applying actual spells | 🟡 OPEN — needs full spell system |
| 5 | Making the waves system | 🟡 OPEN — needs wave loop |

---

## Issue #3 — Character Movement Animations

**Requirement:** All characters hop/bounce while moving. Weapons animate on attack.
- Enemies + player get a squash-stretch "hop" scale effect when moving.
- Player attack uses `player-staff-with-animations.png` (11 frames).
- Elf weapons swing via **code** (no spritesheet), Santa weapon uses spritesheet (already set up).

**New scripts:**
- `CharacterBob.cs` — scale-based squash-stretch bob (attached to root, drives itself from a `SetMoving(bool)` call)
- `WeaponSwing.cs` — coroutine rotation swing for elf weapon children

**Modified scripts:**
- `Enemy.cs` — get CharacterBob ref; call `SetMoving`; call `weaponSwing.Swing()` on attack; call `WaveManager.EnemyDied()` + delay destroy on die
- `PlayerMovement.cs` — get CharacterBob ref; call `SetMoving`

**Editor script:**
- `PlayerSetup.cs` (Tools > Setup Player) — creates player animation clips, builds PlayerAnimator controller, adds CharacterBob + Animator to player in Game scene

---

## Issue #4 — Actual Spells

**Sprite mapping:**
| Spell | Asset | Frames |
|-------|-------|--------|
| Light | `light-sprite-sheet.png` (pixil-frame-1_0..2) | 3 |
| Water 1 | `water-lvl-1.png` (water-lvl-1_0..5) | 6 |
| Water 2 | `water-lvl-2-with-spread.png` (pixil-frame-2_0..20) | 21 |
| Water 3 | `water-lvl-3.png` (pixil-frame-5_0) + `water-lvl-3-spread.png` (pixil-frame-9_0..26) | 1 + 27 |
| Water 4 | Flower — 6× Water-3 sprite arranged around player | code |
| Fire 1 | `fire-lvl-1.png` (julinaserrano_0..13) | 14 |
| Fire 2 | `fire-lvl-2-with-spread.png` (pixil-frame-3_0..10) | 11 |
| Fire 3 | `fire-lvl-3.png` (pixil-frame-6_0..3) | 4 |
| Fire 4 | `fire-lvl-4.png` (wachin_0) — big slow ball, AOE on impact | 1+code |
| Elec 1 | `electricity-lvl-1.png` (pixil-frame-0_0) | 1 |
| Elec 2 | `electricity-lvl-2-with-spread.png` (pixil-frame-4_0..13) | 14 |
| Elec 3 | `electricity-lvl-3.png` (pixil-frame-7_0) + `electricity-lvl-3-spread.png` (pixil-frame-8_0..92) | 1+93 |
| Elec 4 | `electricity-lvl-4.png` (wacho_0) — chain lightning | 1+code |

**Architecture:**
- `ISpellBehavior` interface — `void Fire(Vector3 dir, SpellData data)` — implemented by Projectile.cs + special scripts
- `SpreadImpact.cs` — plays impact animation + area damage (used by Water 3, Elec 3)
- `WaterFlowerAttack.cs` — instantiates 6 petal projectiles in a ring around the player
- `FireAoEProjectile.cs` — slow fireball; on hit spawns `AreaDamage` zone
- `ChainLightningProjectile.cs` — hits first enemy, jumps to nearest N enemies

**Data:**
- `ElementType.cs` — enum {Water, Fire, Electricity, Light}
- `SpellData.cs` — add `element` + `level` fields
- `SpellDatabase.cs` — ScriptableObject; holds all SpellData arrays; `GetSpell(element, level)` method

**Editor script:**
- `SpellSetup.cs` (Tools > Setup Spells) — creates all clips, controllers, prefabs, SO assets, and populates SpellDatabase

---

## Issue #5 — Wave System

**Game loop:**
1. Game.unity loads → WaveManager starts in Idle
2. SpellSelectionUI shows ("pick your starting spell")
3. Player picks element → PlayerSpellInventory records it, SpellCaster is updated
4. Wave 1 spawns: 2 melee elves + 2 magic elves
5. Kill all enemies → 5s cooldown → SpellSelectionUI again
6. Same element = upgrade level (+1, max 4). Different = new spell at lvl 1.
7. Each wave: +2 elves, more magic ratio, Santa starts at wave 5
8. Player dies → GameOverUI shows wave number reached

**Procedural wave formula:**
- Wave N: `(2 + N*2)` total elves, `floor(N/2)` are magic type
- Santas: `max(0, floor((N-4)/5))` per wave (first at wave 5)

**New scripts:**
- `PlayerSpellInventory.cs` — singleton on Player; `Dictionary<ElementType, int> levels`; `SelectElement(elem)` returns new level
- `SpellSelectionUI.cs` — 4 element buttons + wave label; calls inventory + updates SpellCaster + calls `WaveManager.OnSpellSelected()`
- `WaveManager.cs` (full rewrite) — state machine: Idle → SpellSelection → WaveActive → WaveCooldown → loop

**Modified scripts:**
- `MainMenuUI.cs` — fix `PlayGame()` to call `NameInputManager.ShowNameInput()`
- `GameOverUI.cs` — show current wave number on death

---

## Execution Order

1. Data layer (ElementType, SpellData, SpellDatabase)
2. CharacterBob + WeaponSwing
3. Update Enemy.cs + PlayerMovement.cs
4. Spell behavior scripts (ISpellBehavior, SpreadImpact, WaterFlower, FireAoE, ChainLightning)
5. SpellCaster overhaul
6. PlayerSpellInventory + WaveManager overhaul
7. SpellSelectionUI + UI fixes (MainMenu, GameOver)
8. Editor scripts (PlayerSetup, SpellSetup)
9. Run setup scripts → verify in Unity
