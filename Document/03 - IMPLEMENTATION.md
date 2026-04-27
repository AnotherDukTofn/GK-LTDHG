# Implementation Plan — SpellStrike

> **Phiên bản:** 1.0 | **Ngày:** 2026-04-25
> **Mục tiêu:** Playable build đầy đủ gameplay + visuals (Shader, VFX, Particle)
> **Tham chiếu:** [GDD](./01%20-%20GDD.md) · [TDD](./02%20-%20TDD.md)

---

## Nguyên tắc ưu tiên

1. **Playable Loop trước** — Core gameplay hoạt động dù placeholder art
2. **Hệ thống hỗ trợ trước khi polish** — Pooling, Event, Save trước khi thêm nội dung
3. **VFX & Shader song song với gameplay từ Phase 3** — Không để cuối mới làm visual
4. **Mỗi phase kết thúc bằng một build chạy được**

---

## PHASE 1 — Project Foundation & Core Infrastructure

> **Mục tiêu:** Codebase đúng cấu trúc, các hệ thống nền hoạt động.

### 1.1 Project Setup

- [x] Tạo Unity project (Unity 2022.3 LTS, URP template)
- [ ] Cài packages: Input System, Cinemachine, AI Navigation, TextMeshPro *(Unity Editor)*
- [x] Tạo cấu trúc thư mục theo TDD §3
- [ ] Cấu hình Physics Layers *(Unity Editor)*
- [ ] Tạo URP Asset + cấu hình Render Passes cơ bản *(Unity Editor)*
- [ ] Thiết lập Layer Collision Matrix *(Unity Editor)*

### 1.2 Core Systems

- [x] `VoidEventChannelSO` + `IntEventChannelSO` + `FloatEventChannelSO` + `StringEventChannelSO` + `HPEventChannelSO` + `SpellDataEventChannelSO`
- [x] `GameManager` singleton + `GameState` enum (MainMenu → Gameplay → Win/Lose)
- [x] `ObjectPool<T>` generic
- [x] `Timer` utility class (dùng cho các countdown không cần Update)
- [x] `WeightedRandom` utility

### 1.3 ScriptableObject Data Layer (scripts only, chưa cần tạo asset)

- [x] `GameConfigSO`
- [x] `SpellDataSO` + enums (`SpellBehavior`, `SpellCategory`, `SpellInputType`)
- [x] `SpellBehaviorParams` struct
- [x] `EnemyDataSO`
- [x] `EliteBossDataSO` + `EliteSpellEntry` + `EliteSpellRuntime`
- [x] `SpawnerDataSO`
- [x] `LevelDataSO` + `LevelScalingConfig`
- [x] `DropTableSO` + `WeightedDropEntry`
- [x] `LootBoxDataSO`
- [x] `AudioLibrarySO` + `AudioEntry`

### 1.4 State Machine Framework

- [x] `IState` interface
- [x] `StateMachine` class

---

## PHASE 2 — Player & Input

> **Mục tiêu:** Player di chuyển, xoay, dash, có HP/Stamina — chưa cần phép.

### 2.1 Input

- [ ] Tạo `PlayerInputActions.inputactions` (Move, Cast, Dash, MousePosition) *(Unity Editor)*
- [x] `PlayerInputHandler` component

### 2.2 Player Movement & States

- [ ] Player prefab với component đúng: `CharacterController`, `Animator`, child TriggerZone *(Unity Editor)*
- [x] `PlayerController` + FSM: `IdleState`, `MoveState`, `DashState`, `DeadState`
- [x] `StaminaComponent` (consume, regen, event)
- [x] `DashController` (Coroutine lerp, i-frame)
- [x] `RotateTowardMouse()` qua raycast đến Plane(y=0)

### 2.3 Combat Components (dùng chung, để sẵn)

- [x] `HealthComponent` (hybrid SO Event + C# Action)
- [x] `StatusEffectController` (Shield, Invincible, Haste, Slow)
- [x] `DamageDealer` component (LayerMask, scaling)

### 2.4 Camera

- [ ] Cinemachine Virtual Camera top-down follow, transposer offset `(0, 15, -8)` *(Unity Editor)*
- [x] `CameraFollow` script tạm (chờ cài Cinemachine)

### 2.5 HUD cơ bản (placeholder)

- [x] `HPBarUI` lắng nghe `HPEventChannelSO`
- [x] `StaminaBarUI` lắng nghe `FloatEventChannelSO`

---

## PHASE 3 — Spell System

> **Mục tiêu:** Player cast được ít nhất 1 phép, gây damage, có VFX placeholder.

### 3.1 Base & Infrastructure

- [x] `SpellBase` abstract MonoBehaviour
- [x] `PlayerSpellController` (EquipSpell, TryCast, Hold support)
- [x] `SpellPanelUI` lắng nghe `OnSpellEquipped`, `OnSpellCooldownUpdated`
- [x] `SpellPoolService` MonoBehaviour

### 3.2 Fireball (AreaLob) — implement đầu tiên để validate pipeline

- [x] `FireballSpell` : SpellBase
- [ ] Fireball projectile prefab (arc trajectory Coroutine, AoE OverlapSphere) *(Unity Editor)*
- [ ] **VFX:** Particle System lob trail + impact explosion (URP) *(Unity Editor)*
- [ ] **Shader:** Fireball sphere glow shader (URP Shader Graph: emissive + Fresnel) *(Unity Editor)*
- [ ] `SpellDataSO` asset: Fireball *(Unity Editor)*

### 3.3 Splash (PiercingProjectile)

- [x] `SplashSpell` : SpellBase
- [ ] Pooled piercing projectile prefab *(Unity Editor)*
- [ ] **VFX:** Electric spark trail particle + hit flash *(Unity Editor)*
- [ ] `SpellDataSO` asset: Splash *(Unity Editor)*

### 3.4 Inferno Breathe (ConeSpray — Hold)

- [x] `InfernoBreatheSpell` : SpellBase (`OnCastHoldStart/End`)
- [x] Cone overlap check bằng `Physics.OverlapSphere` + angle filter
- [ ] **VFX:** Cone flame particle system (billboard, loop) *(Unity Editor)*
- [ ] **Shader:** Flame distortion shader (URP Unlit + noise scroll UV) *(Unity Editor)*
- [x] Slow effect liên kết với `StatusEffectController`

### 3.5 Static Field (PassiveAura)

- [x] `StaticFieldSpell` : SpellBase (OnEquip tick)
- [ ] **VFX:** Aura ring particle (looping, rotates around player) *(Unity Editor)*
- [ ] **Shader:** Energy shield shader (Fresnel + animated rim) *(Unity Editor)*

### 3.6 Blizzard (Transform)

- [x] `BlizzardSpell` : SpellBase
- [x] Kích hoạt Coroutine Transform trong script (do state machine là declarative, tích hợp trực tiếp)
- [x] `Physics.IgnoreLayerCollision` để xuyên Enemy layer
- [ ] **VFX:** Snow/ice storm particle system quanh player *(Unity Editor)*
- [ ] **Shader:** Ice crystal glow shader (Shader Graph: iridescence + emissive) *(Unity Editor)*

---

## PHASE 4 — Enemy, Spawner & Combat Loop

> **Mục tiêu:** Enemy xuất hiện, tấn công, chết, loot rơi — gameplay loop hoàn chỉnh.

### 4.1 Enemy AI

- [x] `EnemyBase` : MonoBehaviour (FSM, NavMeshAgent, HealthComponent)
- [x] `MeleeEnemy` : EnemyBase (ChaseState, AttackState, PatrolState)
- [x] `RangedEnemy` : EnemyBase (ChaseState, AttackState, RetreatState, PatrolState)
- [x] Enemy Projectile (pooled, DamageDealer)
- [ ] `EnemyDataSO` assets: Melee, Ranged *(Unity Editor)*

### 4.2 Spawner

- [x] `SpawnerController` (self-register vào LevelManager, SpawnLoop Coroutine)
- [ ] `SpawnerDataSO` asset mẫu *(Unity Editor)*
- [ ] Health cho Spawner (HealthComponent, trông như cấu trúc) *(Unity Editor)*

### 4.3 Loot & Drop

- [x] `LootDropperService` MonoBehaviour (scene singleton)
- [x] `DropItem` abstract (lifetime countdown, nhấp nháy cảnh báo)
- [x] `SpellDropItem`, `PowerUpDropItem`
- [x] `LootBox` (LootBoxDataSO)
- [ ] `DropTableSO` assets: MeleeDrop, RangedDrop, SpawnerDrop, LootBoxDrop *(Unity Editor)*
- [ ] **VFX:** Item glow particle (spell drop = blue spiral, power-up = gold burst) *(Unity Editor)*
- [ ] `NotificationUI` pop-up fade khi nhặt item *(Unity Editor)*

### 4.4 Power-Up System

- [x] `PowerUpDropItem` : DropItem (Heal, Haste, Shield, Invincible)
- [x] Liên kết với `StatusEffectController`

---

## PHASE 5 — Level Flow & Elite Boss

> **Mục tiêu:** Level có đầu - giữa - cuối hoàn chỉnh.

### 5.1 Level Management

- [x] `LevelManager` (đếm spawner, kích hoạt Elite, win condition)
- [ ] `SpawnerCounterUI` *(Unity Editor)*
- [x] `WinScreenUI`, `LoseScreenUI` (Tích hợp ref trong LevelManager)

### 5.2 Spell Selection (Phase trước Gameplay)

- [x] `SpellSelectionUI` panel overlay (hiện khi bắt đầu level)
- [x] Gọi `SpellPoolService.GetRandomStartingSpells(N)`
- [x] Player chọn → `PlayerSpellController.EquipSpell`

### 5.3 Save & Progression

- [x] `SaveManager` (JSON to `persistentDataPath`)
- [x] `SaveData` : UnlockedSpellIds

### 5.4 Elite Boss

- [x] `EliteBossController` (FSM: ChaseState, AttackState, DeadState)
- [x] `EliteSpellRuntime` cooldown tracking (tích hợp trong Override của EliteBossDataSO)
- [ ] 3 `EliteBossDataSO` assets (Elite_LV1, LV2, LV3) với spell override values *(Unity Editor)*
- [ ] Unlock spell reward → `SaveManager.UnlockSpell` → `OnSpellUnlocked` event *(Cần setup Editor Drop)*
- [x] `EliteHPBarUI`
- [ ] **VFX:** Elite spawn cinematic particle (smoke pillars, screen flash) *(Unity Editor)*
- [ ] **VFX:** Elite death explosion (overdrive bloom, shockwave ring) *(Unity Editor)*
- [ ] **Shader:** Elite body glow shader (pulse emissive theo phase HP) *(Unity Editor)*

---

## PHASE 6 — Audio

> **Mục tiêu:** Game có âm thanh hoàn chỉnh.

- [ ] `AudioManager` singleton (DontDestroyOnLoad)
- [ ] `AudioLibrarySO` với tất cả clip entries: sfx_dash, sfx_fireball_cast, sfx_fireball_impact, sfx_splash_cast, sfx_inferno_loop, sfx_aura_tick, sfx_blizzard_enter, sfx_enemy_melee_hit, sfx_enemy_ranged_shoot, sfx_spawner_die, sfx_elite_spawn, sfx_elite_die, sfx_item_pickup, sfx_spell_pickup, sfx_ui_confirm, sfx_player_death, bgm_gameplay, bgm_elite, bgm_menu
- [ ] Gắn gọi AudioManager vào tất cả hệ thống gameplay

---

## PHASE 7 — Level Design & Visual Polish

> **Mục tiêu:** 3 level chơi được, map đẹp, ánh sáng + post-processing.

### 7.1 Level Design (3 level)

- [ ] Level_01: Arena nhỏ, 2 spawner, Elite_LV1
- [ ] Level_02: Arena trung, 3 spawner, Elite_LV2
- [ ] Level_03: Arena lớn, 5 spawner, Elite_LV3
- [ ] NavMesh bake cho mỗi level
- [ ] Minimap marker setup

### 7.2 Environment Art

- [ ] Ground material (URP Lit, tiling): stone / dark magic circle
- [ ] Arena wall / pillar models + materials
- [ ] Décor objects (barrels, crates dùng làm LootBox)
- [ ] **Shader:** Ground magic circle (Shader Graph: UV rotate + emissive glow mask)
- [ ] **Shader:** Barrier/wall force-field (Fresnel rim + animated scroll)

### 7.3 Lighting & Post-Processing

- [ ] Mixed lighting setup (Directional + baked GI point lights)
- [ ] URP Global Volume: Bloom (intensity 0.3-0.8), Vignette (subtle), Color Grading (moodier)
- [ ] Spell cast → local Bloom spike via Volume Weight animation
- [ ] Elite arena: Red/purple tint Global Volume khi Elite xuất hiện

### 7.4 Character & Enemy Visuals

- [ ] Player model + Rig + BasicAnimations (Idle, Walk, Dash)
- [ ] Melee Enemy model + Animations (Chase, Attack, Death)
- [ ] Ranged Enemy model + Animations
- [ ] Elite Boss model (3 biến thể) + Animations
- [ ] **Shader:** Enemy body dissolve on death (Shader Graph: noise clip dissolve + emissive edge)

### 7.5 UI Polish

- [ ] Font chọn (Google Fonts): premium fantasy/sci-fi font
- [ ] HUD layout design + icon set (spell icons)
- [ ] Menu Main screen UI
- [ ] Transition animations (fade in/out giữa state)

---

## PHASE 8 — Main Menu & Final Polish

- [ ] `MainMenu` scene: Logo, Play, Quit
- [ ] Game flow hoàn chỉnh: MainMenu → SpellSelection panel → Gameplay → Win/Lose → Retry
- [ ] Settings (Volume slider)
- [ ] Minimap hoàn chỉnh (Player chấm xanh, Enemy chấm đỏ, Spawner chấm xám, Elite chấm vàng)
- [ ] **VFX:** UI button hover particle (subtle sparkle)
- [ ] Performance pass: bake lighting, occlusion culling, pool size review
- [ ] Build & smoke test (Windows 64-bit)

---

## VFX & Shader Master Checklist

| Hệ thống | VFX | Shader |
|---|---|---|
| Fireball | Lob trail, impact explosion | Fireball glow (Emissive + Fresnel) |
| Splash | Spark trail, hit flash | — |
| Inferno Breathe | Cone flame loop | Flame distortion (noise UV scroll) |
| Static Field | Aura ring loop | Energy shield rim (Fresnel animated) |
| Blizzard | Ice storm swirl | Ice crystal (iridescence + emissive) |
| Enemy death | Dissolve burst | Dissolve (noise clip + emissive edge) |
| Elite spawn | Smoke pillars, flash | — |
| Elite death | Shockwave ring | Overdrive bloom spike |
| Elite body | — | Pulsing emissive phase indicator |
| Drop items | Glow idle loop | — |
| Ground arena | — | Magic circle (UV rotate + glow mask) |
| Arena walls | — | Force-field Fresnel scroll |

---

## Ước tính thời gian (Solo Dev)

| Phase | Nội dung | Ước tính |
|---|---|---|
| 1 | Foundation + Infrastructure | 3–4 ngày |
| 2 | Player + Input + Camera | 3–4 ngày |
| 3 | Spell System (5 phép) | 5–7 ngày |
| 4 | Enemy + Spawner + Loot | 4–5 ngày |
| 5 | Level Flow + Elite Boss | 4–6 ngày |
| 6 | Audio | 1–2 ngày |
| 7 | Level Design + Visual Polish | 6–8 ngày |
| 8 | Menu + Final Polish + Build | 2–3 ngày |
| **Tổng** | | **28–39 ngày** |

---

## Milestone Playable Builds

| Milestone | Điều kiện | Phase |
|---|---|---|
| **Alpha** | Player di chuyển, cast Fireball, Enemy spawn và chết | End P4 |
| **Beta** | 5 phép, Elite Boss, Level flow hoàn chỉnh, Save | End P5 |
| **RC** | Audio, 3 Level, VFX/Shader đầy đủ | End P7 |
| **Gold** | Menu, Polish, Build final | End P8 |
