# Technical Design Document (TDD)

# **Spell It! — Top-Down Magic Shooter**

> **Phiên bản:** 0.2
> **Ngày:** 2026-04-25
> **Tham chiếu:** [01 - GDD.md](./01%20-%20GDD.md)

---

## Mục lục

1. [Tech Stack & Ràng buộc kỹ thuật](#1-tech-stack--ràng-buộc-kỹ-thuật)
2. [Kiến trúc tổng quan](#2-kiến-trúc-tổng-quan)
3. [Cấu trúc thư mục dự án](#3-cấu-trúc-thư-mục-dự-án)
4. [Quy ước viết code](#4-quy-ước-viết-code)
5. [Core Systems — Thiết kế chi tiết](#5-core-systems--thiết-kế-chi-tiết)
6. [ScriptableObject Data Architecture](#6-scriptableobject-data-architecture)
7. [State Machine Framework](#7-state-machine-framework)
8. [Hệ thống Input](#8-hệ-thống-input)
9. [Hệ thống Camera](#9-hệ-thống-camera)
10. [Player System](#10-player-system)
11. [Spell System](#11-spell-system)
12. [Enemy System](#12-enemy-system)
13. [Spawner System](#13-spawner-system)
14. [Elite Boss System](#14-elite-boss-system)
15. [Loot & Drop System](#15-loot--drop-system)
16. [Power-Up System](#16-power-up-system)
17. [Health & Damage System](#17-health--damage-system)
18. [Status Effect System](#18-status-effect-system)
19. [Level Management](#19-level-management)
20. [Save & Progression System](#20-save--progression-system)
21. [UI System](#21-ui-system)
22. [Audio System](#22-audio-system)
23. [VFX System](#23-vfx-system)
24. [Object Pooling](#24-object-pooling)
25. [Phụ lục — Danh sách ScriptableObject](#25-phụ-lục--danh-sách-scriptableobject)

---

## 1. Tech Stack & Ràng buộc kỹ thuật

### 1.1 Engine & Render Pipeline

| Thành phần | Lựa chọn | Lý do |
|---|---|---|
| Engine | **Unity 2022.3 LTS** | Ổn định, hỗ trợ dài hạn |
| Render Pipeline | **URP (Universal Render Pipeline)** | Hiệu năng tốt cho top-down 3D, shader đơn giản |
| .NET | **.NET Standard 2.1 / C# 9+** | Tương thích rộng |

### 1.2 Packages bắt buộc

| Package | Mục đích |
|---|---|
| `com.unity.inputsystem` | New Input System — xử lý WASD, chuột, Shift |
| `com.unity.textmeshpro` | Render text UI (HUD, menu, notification) |
| `com.unity.ai.navigation` | NavMesh cho AI pathfinding |
| `com.unity.cinemachine` | Camera top-down smooth follow |
| `com.unity.ugui` | Canvas UI cho HUD, menu |

### 1.3 Ràng buộc kỹ thuật

- **Không hardcode** bất kỳ chỉ số gameplay nào — tất cả đọc từ ScriptableObject.
- **Không dùng** `GameObject.Find()`, `FindObjectOfType()` trong runtime — chỉ inject qua Inspector hoặc self-registration pattern.
- **Timer/Cooldown:** Ưu tiên Coroutine hoặc custom `Timer` utility. Cho phép `Update()` cho cooldown đơn giản theo frame (ví dụ: `SpellBase`).
- **Object Pooling** bắt buộc cho: đạn (projectile), enemy, VFX, drop item.
- Physics chỉ dùng **3D Physics** (Rigidbody, Collider 3D) — không trộn 2D.
- Tất cả AI di chuyển qua **NavMeshAgent** — không tự viết pathfinding.

---

## 2. Kiến trúc tổng quan

### 2.1 Mô hình kiến trúc

Game sử dụng kiến trúc **Component-Based + Data-Driven + Event-Driven**:

```
┌─────────────────────────────────────────────────────────────────┐
│                     GAME MANAGER (Singleton)                    │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌───────────────┐   │
│  │GameState  │  │LevelMgr  │  │SaveMgr   │  │AudioMgr       │   │
│  │Machine    │  │          │  │          │  │               │   │
│  └──────────┘  └──────────┘  └──────────┘  └───────────────┘   │
├─────────────────────────────────────────────────────────────────┤
│                     EVENT BUS (ScriptableObject Events)         │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────┐  ┌──────────┐  ┌──────────┐  ┌───────────────┐   │
│  │Player   │  │Enemy     │  │Spell     │  │UI             │   │
│  │System   │  │System    │  │System    │  │System         │   │
│  └─────────┘  └──────────┘  └──────────┘  └───────────────┘   │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────┐  ┌──────────┐  ┌──────────┐  ┌───────────────┐   │
│  │Health   │  │Loot/Drop │  │Spawner   │  │Object Pool    │   │
│  │System   │  │System    │  │System    │  │               │   │
│  └─────────┘  └──────────┘  └──────────┘  └───────────────┘   │
├─────────────────────────────────────────────────────────────────┤
│                 DATA LAYER (ScriptableObjects)                  │
│  SpellDataSO · EnemyDataSO · EliteBossDataSO · LevelDataSO     │
│  SpawnerDataSO · DropTableSO · GameConfigSO                     │
└─────────────────────────────────────────────────────────────────┘
```

### 2.2 Nguyên tắc giao tiếp giữa system

| Giao tiếp | Cơ chế | Ví dụ |
|---|---|---|
| System → System | **SO Event Channel** | `OnSpawnerDestroyed` → LevelManager lắng nghe |
| Data → Logic | **ScriptableObject** (readonly runtime) | `SpellDataSO` cung cấp damage, cooldown |
| UI ← Game | **SO Event Channel** hoặc **UnityEvent** | `OnPlayerHPChanged` → HP Bar cập nhật |
| Input → Player | **Input Action Asset** → PlayerInput component | Action `Move`, `Cast`, `Dash` |

### 2.3 SO Event Channel Pattern

```csharp
// Event không tham số
[CreateAssetMenu(menuName = "Events/Void Event Channel")]
public class VoidEventChannelSO : ScriptableObject
{
    private System.Action m_OnEventRaised;
    public void RaiseEvent() => m_OnEventRaised?.Invoke();
    public void Subscribe(System.Action _listener) => m_OnEventRaised += _listener;
    public void Unsubscribe(System.Action _listener) => m_OnEventRaised -= _listener;
}

// Event có tham số generic
[CreateAssetMenu(menuName = "Events/Int Event Channel")]
public class IntEventChannelSO : ScriptableObject
{
    private System.Action<int> m_OnEventRaised;
    public void RaiseEvent(int _value) => m_OnEventRaised?.Invoke(_value);
    public void Subscribe(System.Action<int> _listener) => m_OnEventRaised += _listener;
    public void Unsubscribe(System.Action<int> _listener) => m_OnEventRaised -= _listener;
}

// Event HP cần truyền cả current và max
[System.Serializable]
public struct HPData { public int Current; public int Max; }

[CreateAssetMenu(menuName = "Events/HP Event Channel")]
public class HPEventChannelSO : ScriptableObject
{
    private System.Action<HPData> m_OnEventRaised;
    public void RaiseEvent(HPData _value) => m_OnEventRaised?.Invoke(_value);
    public void Subscribe(System.Action<HPData> _listener) => m_OnEventRaised += _listener;
    public void Unsubscribe(System.Action<HPData> _listener) => m_OnEventRaised -= _listener;
}
```

### 2.4 Quy tắc giao tiếp: SO Event Channel vs C# Action

| Trường hợp | Cơ chế | Ví dụ |
|---|---|---|
| **Cross-system** (khác GameObject/system) | **SO Event Channel** | Player HP → HUD, Spawner → LevelManager |
| **Local** (cùng GameObject/component) | **C# Action** | HealthComponent.OnDeath → EnemyBase.HandleDeath |

**Danh sách Event Channel cần tạo:**

| Event Channel | Kiểu dữ liệu | Publisher | Subscriber |
|---|---|---|---|
| `OnPlayerHPChanged` | `HPEventChannelSO` | HealthComponent (Player) | HUD HP Bar |
| `OnPlayerStaminaChanged` | `FloatEventChannelSO` | StaminaComponent | HUD Stamina Bar |
| `OnSpellEquipped` | `SpellDataEventChannelSO` | PlayerSpellController | HUD Spell Panel |
| `OnSpellCooldownUpdated` | `FloatEventChannelSO` | SpellBase | HUD Cooldown Bar |
| `OnSpawnerDestroyed` | `VoidEventChannelSO` | SpawnerController | LevelManager |
| `OnAllSpawnersDestroyed` | `VoidEventChannelSO` | LevelManager | EliteBossController |
| `OnEliteDefeated` | `VoidEventChannelSO` | EliteBoss | LevelManager, UI |
| `OnSpellUnlocked` | `SpellDataEventChannelSO` | SaveManager | UI Unlock Panel |
| `OnPlayerDeath` | `VoidEventChannelSO` | HealthComponent (Player) | GameManager |
| `OnItemPickedUp` | `StringEventChannelSO` | DropItem | HUD Notification |
| `OnGameWin` | `VoidEventChannelSO` | LevelManager | UI Win Screen |
| `OnGameLose` | `VoidEventChannelSO` | GameManager | UI Lose Screen |

---

## 3. Cấu trúc thư mục dự án

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Core/                    # Singleton, Event Channel, Object Pool
│   │   │   ├── GameManager.cs
│   │   │   ├── EventChannels/       # Tất cả SO Event Channel scripts
│   │   │   └── ObjectPool/          # Generic pool system
│   │   ├── Player/
│   │   │   ├── PlayerController.cs
│   │   │   ├── PlayerSpellController.cs
│   │   │   ├── StaminaComponent.cs
│   │   │   ├── DashController.cs
│   │   │   └── PlayerStateMachine/  # FSM cho player states
│   │   ├── Spell/
│   │   │   ├── SpellBase.cs         # Abstract base
│   │   │   ├── FireballSpell.cs
│   │   │   ├── SplashSpell.cs
│   │   │   ├── InfernoBreatheSpell.cs
│   │   │   ├── StaticFieldSpell.cs
│   │   │   ├── BlizzardSpell.cs
│   │   │   └── SpellPool.cs         # Service quản lý pool phép
│   │   ├── Enemy/
│   │   │   ├── EnemyBase.cs
│   │   │   ├── MeleeEnemy.cs
│   │   │   ├── RangedEnemy.cs
│   │   │   └── EnemyStateMachine/
│   │   ├── EliteBoss/
│   │   │   ├── EliteBossController.cs
│   │   │   └── EliteAI/
│   │   ├── Spawner/
│   │   │   └── SpawnerController.cs
│   │   ├── Loot/
│   │   │   ├── LootDropper.cs
│   │   │   ├── DropItem.cs          # Base class
│   │   │   ├── SpellDropItem.cs
│   │   │   ├── PowerUpDropItem.cs
│   │   │   └── LootBox.cs
│   │   ├── Combat/
│   │   │   ├── HealthComponent.cs   # Dùng chung Player, Enemy, Spawner, LootBox
│   │   │   ├── DamageDealer.cs      # Gắn lên projectile/spell/enemy attack
│   │   │   └── StatusEffectController.cs
│   │   ├── PowerUp/
│   │   │   └── PowerUpDropItem.cs   # Logic nhặt + áp dụng effect
│   │   ├── Level/
│   │   │   └── LevelManager.cs
│   │   ├── Save/
│   │   │   └── SaveManager.cs
│   │   ├── UI/
│   │   │   ├── HUDController.cs
│   │   │   ├── HPBarUI.cs
│   │   │   ├── StaminaBarUI.cs
│   │   │   ├── SpellPanelUI.cs
│   │   │   ├── SpawnerCounterUI.cs
│   │   │   ├── EliteHPBarUI.cs
│   │   │   ├── MinimapUI.cs
│   │   │   ├── SpellSelectionUI.cs
│   │   │   ├── WinScreenUI.cs
│   │   │   ├── LoseScreenUI.cs
│   │   │   └── NotificationUI.cs
│   │   ├── Audio/
│   │   │   └── AudioManager.cs
│   │   └── Utility/
│   │       ├── Timer.cs
│   │       └── WeightedRandom.cs
│   ├── Data/                        # ScriptableObject assets
│   │   ├── Spells/                  # SpellDataSO instances
│   │   ├── Enemies/                 # EnemyDataSO instances
│   │   ├── EliteBosses/             # EliteBossDataSO instances
│   │   ├── Levels/                  # LevelDataSO instances
│   │   ├── PowerUps/                # PowerUpDataSO instances
│   │   ├── DropTables/              # DropTableSO instances
│   │   ├── Config/                  # GameConfigSO
│   │   └── Events/                  # Event Channel SO instances
│   ├── Prefabs/
│   │   ├── Player/
│   │   ├── Enemies/
│   │   ├── Spells/                  # Spell VFX prefabs
│   │   ├── Drops/
│   │   ├── UI/
│   │   └── VFX/
│   ├── Art/
│   │   ├── Models/
│   │   ├── Materials/
│   │   ├── Textures/
│   │   └── Animations/
│   ├── Audio/
│   │   ├── SFX/
│   │   └── BGM/
│   └── Scenes/
│       ├── MainMenu.unity
│       ├── Level_01.unity
│       ├── Level_02.unity
│       └── Level_03.unity
```

---

## 4. Quy ước viết code

> Tuân thủ theo `architecture_02.md`.

### 4.1 Naming Convention

| Loại | Quy tắc | Ví dụ |
|---|---|---|
| Private field | `m_FieldName` | `m_CurrentHP` |
| Constant | `c_ConstantName` | `c_MaxHP` |
| Static | `s_StaticName` | `s_Instance` |
| Class / Struct | `PascalCase` | `PlayerController` |
| Property | `PascalCase` | `CurrentHP` |
| Method | `PascalCase()` | `TakeDamage()` |
| Argument | `_argumentName` | `_amount` |
| Local variable | `camelCase` | `remainingHP` |
| ScriptableObject | Hậu tố `SO` | `SpellDataSO` |
| Event Channel | Hậu tố `EventChannelSO` | `VoidEventChannelSO` |

### 4.2 Cấu trúc file .cs

```csharp
public class ClassName : MonoBehaviour
{
    #region Constants
    #endregion

    #region Serialized Fields
    // [SerializeField] private ...
    #endregion

    #region Private Fields
    #endregion

    #region Public Properties
    #endregion

    #region Unity Lifecycle
    // Awake, Start, Update, OnDestroy, OnEnable, OnDisable
    #endregion

    #region Public Methods
    #endregion

    #region Private Methods
    #endregion

    #if UNITY_EDITOR
    // Debug / Editor-only
    #endif
}
```

### 4.3 Quy tắc bắt buộc

1. Mỗi class chỉ **một trách nhiệm** (Single Responsibility).
2. Dùng `[SerializeField]` thay vì `public` field.
3. Dùng `TryGetComponent<T>()` thay vì `GetComponent<T>()` khi không chắc có component.
4. Dùng `TextMeshPro` cho mọi text.
5. Wrap editor code trong `#if UNITY_EDITOR`.
6. Comment bằng `///` cho public API.

---

## 5. Core Systems — Thiết kế chi tiết

### 5.1 GameManager (Singleton)

**Trách nhiệm:** Quản lý game state tổng thể, transition giữa các scene.

```csharp
public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager s_Instance;
    public static GameManager Instance => s_Instance;
    #endregion

    #region Serialized Fields
    [SerializeField] private VoidEventChannelSO m_OnGameWin;
    [SerializeField] private VoidEventChannelSO m_OnGameLose;
    #endregion

    // Game States: MainMenu → SpellSelection → Gameplay → Win/Lose
    // Dùng enum GameState + switch hoặc FSM đơn giản
}
```

**Game State Flow:**

```
MainMenu → SpellSelection → Gameplay_Spawner → Gameplay_Elite → Win
                                    ↓                  ↓
                                  Lose               Lose
```

---

## 6. ScriptableObject Data Architecture

### 6.1 SpellDataSO

```csharp
[CreateAssetMenu(menuName = "Data/Spell Data")]
public class SpellDataSO : ScriptableObject
{
    #region Basic Info
    [SerializeField] private string m_Id;
    [SerializeField] private string m_DisplayName;
    [SerializeField] private Sprite m_Icon;
    [SerializeField] private string m_Description;
    #endregion

    #region Classification
    [SerializeField] private SpellCategory m_Category;  // Basic, Unlocked
    [SerializeField] private SpellBehavior m_Behavior;   // AreaLob, PiercingProjectile, ConeSpray, PassiveAura, Transform
    [SerializeField] private SpellInputType m_InputType; // Click, Hold, Passive
    #endregion

    #region Stats
    [SerializeField] private float m_Damage;
    [SerializeField] private float m_Cooldown;
    [SerializeField] private float m_Range;
    #endregion

    #region Behavior Params
    [SerializeField] private SpellBehaviorParams m_BehaviorParams;
    #endregion

    #region Prefab
    [SerializeField] private GameObject m_SpellPrefab;   // Prefab chứa SpellBase concrete
    [SerializeField] private GameObject m_VfxPrefab;
    #endregion
}

public enum SpellCategory { Basic, Unlocked }
public enum SpellBehavior
{
    // Player spells
    AreaLob,              // Fireball
    PiercingProjectile,   // Splash
    ConeSpray,            // Inferno Breathe
    PassiveAura,          // Static Field
    Transform,            // Blizzard
    // Elite Boss spells (để mở rộng)
    Projectile,           // Đạn bay thẳng (không xuyên)
    Homing,               // Đạn bám mục tiêu
    MeleeBurst,           // Sóng lan cận chiến
    Beam                  // Tia liên tục
}
public enum SpellInputType { Click, Hold, Passive }

[System.Serializable]
public class SpellBehaviorParams
{
    public float ProjectileSpeed;
    public float AoeRadius;
    public float ConeAngle;
    public float ConeRadius;
    public float TickRate;
    public float DamagePerTick;
    public float Duration;
    public float StartupDelay;
    public float RotateSpeedOverride;
    public float SlowAmount;
    public float SlowDuration;
    public float AuraRadius;
}
```

> **Ghi chú thiết kế:** `SpellBehaviorParams` dùng flat struct chứa tất cả params. Mỗi spell chỉ dùng một số fields liên quan. Trong Inspector, sử dụng `#if UNITY_EDITOR` custom PropertyDrawer để ẩn fields không cần thiết dựa trên `SpellBehavior` đang chọn.

### 6.2 EnemyDataSO

```csharp
[CreateAssetMenu(menuName = "Data/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    [SerializeField] private string m_Id;
    [SerializeField] private string m_DisplayName;
    [SerializeField] private EnemyType m_Type;          // Melee, Ranged
    [SerializeField] private int m_HP;
    [SerializeField] private float m_Speed;
    [SerializeField] private float m_Damage;
    [SerializeField] private float m_AttackRange;
    [SerializeField] private float m_AttackRate;
    [SerializeField] private float m_PreferredDistanceMin;  // Chỉ Ranged (6m)
    [SerializeField] private float m_PreferredDistanceMax;  // Chỉ Ranged (10m)
    [SerializeField] private float m_LoseAggroTime;
    [SerializeField] private DropTableSO m_DropTable;
    [SerializeField] private GameObject m_Prefab;
}

public enum EnemyType { Melee, Ranged }
```

### 6.3 EliteBossDataSO

```csharp
[CreateAssetMenu(menuName = "Data/Elite Boss Data")]
public class EliteBossDataSO : ScriptableObject
{
    [SerializeField] private string m_Id;
    [SerializeField] private string m_DisplayName;
    [SerializeField] private int m_HP;
    [SerializeField] private float m_Speed;
    [SerializeField] private List<EliteSpellEntry> m_SpellList;
    [SerializeField] private SpellDataSO m_UnlockedSpellReward;
    [SerializeField] private GameObject m_Prefab;
}

[System.Serializable]
public class EliteSpellEntry
{
    public SpellDataSO SpellData;       // Base data (behavior, VFX)
    public int CastPriority;
    public float MinRange;
    public float MaxRange;
    // Override fields — cho phép Elite config riêng, độc lập với player spell
    public float DamageOverride;
    public float CooldownOverride;
    public float ProjectileSpeedOverride;
}
```

### 6.4 SpawnerDataSO

```csharp
[CreateAssetMenu(menuName = "Data/Spawner Data")]
public class SpawnerDataSO : ScriptableObject
{
    [SerializeField] private string m_Id;
    [SerializeField] private int m_HP;
    [SerializeField] private float m_SpawnInterval;       // Default: 5s
    [SerializeField] private int m_SpawnCountMin;          // Default: 1
    [SerializeField] private int m_SpawnCountMax;          // Default: 3
    [SerializeField] private float m_SpawnRadius;           // Bán kính spawn xung quanh
    [SerializeField] private List<EnemyDataSO> m_EnemyPool;
    [SerializeField] private DropTableSO m_DropTable;
}
```

### 6.5 LootBoxDataSO

```csharp
[CreateAssetMenu(menuName = "Data/LootBox Data")]
public class LootBoxDataSO : ScriptableObject
{
    [SerializeField] private int m_HP;                     // Default: 30
    [SerializeField] private DropTableSO m_DropTable;
    [SerializeField] private bool m_BlocksPathfinding;     // Có block NavMesh không
}
```

### 6.6 LevelDataSO

```csharp
[CreateAssetMenu(menuName = "Data/Level Data")]
public class LevelDataSO : ScriptableObject
{
    [SerializeField] private string m_LevelName;
    [SerializeField] private int m_StartingSpellPoolSize;
    [SerializeField] private List<SpellDataSO> m_SpellDropPool;
    [SerializeField] private LevelScalingConfig m_ScalingConfig;
    [SerializeField] private EliteBossDataSO m_EliteBossData;
    [SerializeField] private string m_SceneName;
}

[System.Serializable]
public class LevelScalingConfig
{
    [Range(0.5f, 3f)] public float EnemyHPMult = 1f;
    [Range(0.5f, 3f)] public float EnemyDmgMult = 1f;
    [Range(0.5f, 3f)] public float EnemySpeedMult = 1f;
    [Range(0.5f, 3f)] public float SpawnerHPMult = 1f;
    [Range(0.1f, 2f)] public float SpawnRateMult = 1f;
    [Range(0.5f, 5f)] public float EliteHPMult = 1f;
    [Range(0.5f, 3f)] public float EliteDmgMult = 1f;
}
```

### 6.7 DropTableSO & GameConfigSO

```csharp
[CreateAssetMenu(menuName = "Data/Drop Table")]
public class DropTableSO : ScriptableObject
{
    [SerializeField] private List<WeightedDropEntry> m_Entries;
    // Method: Roll() → DropItemType
}

[System.Serializable]
public class WeightedDropEntry
{
    public DropItemType ItemType;  // Nothing, PowerUpRandom, SpellRandom, Heal
    public int Weight;
}

public enum DropItemType { Nothing, PowerUpRandom, SpellRandom, Heal }
```

> **Exception:** `GameConfigSO` dùng `public` fields thay vì `[SerializeField] private` để đơn giản hóa truy xuất config readonly. Đây là ngoại lệ duy nhất cho convention §4.3.

```csharp
[CreateAssetMenu(menuName = "Data/Game Config")]
public class GameConfigSO : ScriptableObject
{
    [Header("Player")]
    public int PlayerMaxHP = 100;
    public int PlayerMaxStamina = 100;
    public float PlayerMoveSpeed = 5f;
    public float PlayerRotateSpeed = 720f;

    [Header("Dash")]
    public float DashDistance = 4f;
    public float DashDuration = 0.2f;
    public int DashStaminaCost = 25;
    public float DashIframeDuration = 0.15f;
    public float StaminaRegenDelay = 0.5f;
    public float StaminaRegenRate = 20f;

    [Header("Drop Lifetime")]
    public float SpellDropLifetime = 15f;
    public float PowerUpLifetime = 10f;
    public float DropWarningTime = 3f;

    [Header("Power-Up")]
    public int HealAmount = 25;
    public float HasteAmount = 0.6f;
    public float HasteDuration = 8f;
    public float InvincibleDuration = 5f;

    [Header("Camera")]
    public float CameraLerpSpeed = 5f;
}
```

---

## 7. State Machine Framework

Dùng chung cho Player, Enemy, và Elite Boss. Mỗi entity có FSM riêng.

### 7.1 Interface & Base Class

```csharp
public interface IState
{
    void Enter();
    void Execute();  // Gọi mỗi frame
    void Exit();
}

public class StateMachine
{
    private IState m_CurrentState;
    public IState CurrentState => m_CurrentState;

    public void ChangeState(IState _newState)
    {
        m_CurrentState?.Exit();
        m_CurrentState = _newState;
        m_CurrentState.Enter();
    }

    public void Update() => m_CurrentState?.Execute();
}
```

### 7.2 Player States

| State | Mô tả | Transition |
|---|---|---|
| `IdleState` | Đứng yên, chờ input | → `MoveState` khi WASD, → `DashState` khi Shift |
| `MoveState` | Di chuyển theo WASD | → `IdleState` khi dừng, → `DashState` khi Shift |
| `DashState` | Lướt nhanh, i-frame active | → `IdleState`/`MoveState` khi dash kết thúc |
| `DeadState` | Chết, disable input | → Game Over |
| `BlizzardState` | Transform mode (Blizzard spell) | → Previous state khi hết duration |

> **Lưu ý:** Casting phép **không phải một state riêng** — player có thể vừa Move vừa Cast. Spell casting được xử lý bởi `PlayerSpellController` song song với movement state.

### 7.3 Enemy States

| State | Mô tả |
|---|---|
| `ChaseState` | Truy đuổi player qua NavMesh |
| `AttackState` | Tấn công khi trong tầm |
| `RetreatState` | Lùi lại (chỉ Ranged khi player < 3m) |
| `PatrolState` | Tuần tra quanh vị trí spawn khi mất aggro |

### 7.4 Elite Boss States

| State | Mô tả |
|---|---|
| `SpawnState` | Animation xuất hiện, invulnerable |
| `EvaluateState` | Đánh giá khoảng cách, chọn spell |
| `CastState` | Đang cast spell |
| `MoveState` | Di chuyển về phía player khi tất cả spell đang cooldown |
| `DeathState` | Animation chết, trigger unlock |

---

## 8. Hệ thống Input

### 8.1 Input Action Asset

Tạo file `PlayerInputActions.inputactions`:

| Action Map | Action | Binding | Type |
|---|---|---|---|
| `Player` | `Move` | WASD | `Value<Vector2>` |
| `Player` | `Cast` | LMB | `Button` (Hold support) |
| `Player` | `Dash` | Shift | `Button` |
| `Player` | `MousePosition` | Mouse Position | `Value<Vector2>` |
| `UI` | `Navigate` | Arrow keys | `Value<Vector2>` |
| `UI` | `Submit` | Enter / LMB | `Button` |

### 8.2 PlayerInputHandler

```csharp
public class PlayerInputHandler : MonoBehaviour
{
    // Đọc Input Action Asset, expose:
    public Vector2 MoveInput { get; private set; }
    public Vector3 MouseWorldPosition { get; private set; }  // Raycast from camera
    public bool IsCastPressed { get; private set; }
    public bool IsCastHeld { get; private set; }
    public bool IsDashPressed { get; private set; }

    // Mouse → World: Raycast từ Camera.main qua mouse screen pos
    // xuống Plane(Vector3.up, 0) → lấy điểm hit = MouseWorldPosition
    // Con trỏ chuột GIỚI HẠN trong viewport (Cursor.lockState = Confined)
}
```

---

## 9. Hệ thống Camera

### 9.1 Cinemachine Setup

- **CinemachineVirtualCamera** follow player.
- **Body:** Transposer — Top-down offset `(0, 15, -8)` (tùy chỉnh, góc ~60-70°).
- **Aim:** Do Nothing (camera cố định góc nhìn).
- **Damping:** `CameraLerpSpeed` từ `GameConfigSO` → set vào Transposer damping.
- **Confine:** CinemachineConfiner3D nếu cần giới hạn camera trong arena.

---

## 10. Player System

### 10.1 Tổng quan component trên Player GameObject

```
Player (GameObject)
├── PlayerController          // FSM, movement, rotation
├── PlayerInputHandler        // Đọc input
├── PlayerSpellController     // Equip, cast spell
├── HealthComponent           // HP
├── StaminaComponent          // Stamina
├── DashController            // Dash logic + i-frame
├── StatusEffectController    // Slow, Shield, Invincible, Haste
├── CharacterController       // Unity CharacterController cho movement
├── Animator                  // Animation controller
└── [Child] TriggerZone (GameObject)
    ├── CapsuleCollider (isTrigger = true)
    └── Rigidbody (Kinematic)  // Cần cho trigger detection
```

> **Không dùng** `Rigidbody` trực tiếp trên Player root — `CharacterController` đã có built-in collision capsule. Trigger collider (để nhặt drop, phát hiện enemy contact) nằm trên child GameObject riêng.

### 10.2 PlayerController

```csharp
public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameConfigSO m_Config;
    [SerializeField] private PlayerInputHandler m_Input;

    private StateMachine m_StateMachine;
    private float m_CurrentMoveSpeed; // Có thể bị modify bởi StatusEffect

    // Update:
    // 1. m_StateMachine.Update()
    // 2. RotateTowardMouse() — Quaternion.RotateTowards, speed = m_Config.PlayerRotateSpeed
    //    (bị override nếu đang dùng Inferno Breathe)
}
```

### 10.3 DashController

```csharp
public class DashController : MonoBehaviour
{
    // Khi Dash:
    // 1. Check StaminaComponent.CanConsume(dashCost)
    // 2. Xác định hướng: nếu có WASD input → dùng input direction
    //                     nếu không → hướng về phía mouse
    // 3. Di chuyển CharacterController.Move() qua khoảng cách DashDistance
    //    trong DashDuration (dùng Coroutine lerp)
    // 4. Bật i-frame: StatusEffectController.SetInvincible(DashIframeDuration)
    // 5. Consume stamina
}
```

### 10.4 StaminaComponent

```csharp
public class StaminaComponent : MonoBehaviour
{
    // m_CurrentStamina, m_MaxStamina (từ GameConfigSO)
    // Khi tiêu thụ → reset regen timer = StaminaRegenDelay
    // Sau delay → hồi StaminaRegenRate mỗi giây
    // Raise OnPlayerStaminaChanged event khi giá trị thay đổi
}
```

---

## 11. Spell System

### 11.1 SpellBase (Abstract)

```csharp
public abstract class SpellBase : MonoBehaviour
{
    protected SpellDataSO m_Data;
    protected float m_CurrentCooldown;

    public void Initialize(SpellDataSO _data) => m_Data = _data;
    public abstract void Cast(Vector3 _targetPoint);
    public virtual void OnEquip() { }
    public virtual void OnUnequip() { }
    public virtual void OnCastHoldStart() { }   // Cho InfernoBreathe
    public virtual void OnCastHoldEnd() { }     // Cho InfernoBreathe
    public float GetCooldownRatio() => m_CurrentCooldown / m_Data.Cooldown;
    public bool IsReady => m_CurrentCooldown <= 0f;
    protected void StartCooldown() => m_CurrentCooldown = m_Data.Cooldown;
    // Update cooldown trong Update() — giảm m_CurrentCooldown theo deltaTime
}
```

### 11.2 Concrete Spell Classes

#### FireballSpell (AreaLob)

```csharp
// Cast(): Instantiate fireball projectile bay cầu vồng đến targetPoint
//   → Khi chạm đất: Physics.OverlapSphere(aoeRadius) → DealDamage cho mỗi enemy
//   → Play VFX vfx_lob_impact, SFX sfx_cast_area_lob
//   → StartCooldown()
// Quỹ đạo cầu: dùng DOTween hoặc Coroutine lerp vị trí + sin curve cho y
```

#### SplashSpell (PiercingProjectile)

```csharp
// Cast(): Instantiate projectile bay thẳng theo hướng target
//   → OnTriggerEnter: gây sát thương, KHÔNG destroy (xuyên qua)
//   → Destroy khi bay hết range hoặc lifetime
//   → Từ Object Pool
```

#### InfernoBreatheSpell (ConeSpray — Hold)

```csharp
// OnCastHoldStart():
//   → Delay StartupDelay → bật cone VFX
//   → Mỗi TickRate: Physics.OverlapSphere + angle check → DealDamage + ApplySlow
//   → Override PlayerController.RotateSpeed = InfernoRotateSpeed
// OnCastHoldEnd():
//   → Tắt cone VFX
//   → Restore PlayerController.RotateSpeed
// Không có cooldown
```

#### StaticFieldSpell (PassiveAura)

```csharp
// OnEquip(): Bật aura VFX quanh player
//   → Mỗi TickRate: Physics.OverlapSphere(auraRadius) → DealDamage cho enemy trong vùng
// Cast(): KHÔNG LÀM GÌ (passive — LMB vô hiệu)
// OnUnequip(): Tắt aura VFX
```

#### BlizzardSpell (Transform)

```csharp
// Cast():
//   → Chuyển PlayerController sang BlizzardState
//   → Bật VFX storm quanh player
//   → SetInvincible(duration)
//   → Chuyển Player sang "BlizzardLayer" (Physics.IgnoreLayerCollision)
//      để xuyên qua Enemy layer — KHÔNG disable collider
//   → Mỗi TickRate: OverlapSphere → DealDamage cho enemy tiếp xúc
//   → Sau Duration: tắt VFX, restore layer, StartCooldown
```

### 11.3 PlayerSpellController

```csharp
public class PlayerSpellController : MonoBehaviour
{
    [SerializeField] private SpellDataEventChannelSO m_OnSpellEquipped;
    private SpellBase m_EquippedSpell;

    public void EquipSpell(SpellDataSO _data)
    {
        // 1. m_EquippedSpell?.OnUnequip() → Destroy old spell GO
        // 2. Instantiate spell prefab from _data.SpellPrefab
        // 3. spell.Initialize(_data)
        // 4. spell.OnEquip()
        // 5. m_OnSpellEquipped.RaiseEvent(_data) → UI update
    }

    public void TryCast(Vector3 _mouseWorldPos)
    {
        // Check IsReady → m_EquippedSpell.Cast(_mouseWorldPos)
    }
}
```

### 11.4 SpellPool (Service)

```csharp
public class SpellPoolService : MonoBehaviour
{
    [SerializeField] private List<SpellDataSO> m_AllBasicSpells;
    [SerializeField] private SaveManager m_SaveManager;

    public List<SpellDataSO> GetStartingPool()
        // => m_AllBasicSpells + m_SaveManager.GetUnlockedSpells()
    public List<SpellDataSO> GetRandomStartingSpells(int _count)
        // => Rút ngẫu nhiên N phép từ GetStartingPool
    public SpellDataSO GetRandomFromDropPool(LevelDataSO _level)
        // => _level.SpellDropPool filtered by unlocked
}
```

> **Không dùng static class** cho SpellPool và LootDropper — cần inject dependencies qua Inspector.

---

## 12. Enemy System

### 12.1 EnemyBase

```csharp
public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] private EnemyDataSO m_Data;
    private StateMachine m_StateMachine;
    private NavMeshAgent m_Agent;
    private HealthComponent m_Health;

    // Awake: Setup states, NavMeshAgent speed = m_Data.Speed * levelScaling
    // OnDeath: LootDropper.SpawnDrop(m_Data.DropTable, position)
    //          → Trả về pool
}
```

### 12.2 MeleeEnemy

```
ChaseState: NavMeshAgent.SetDestination(player.position)
  → khi distance ≤ AttackRange → chuyển AttackState
AttackState: Gây Damage mỗi AttackRate giây
  → khi distance > AttackRange → chuyển ChaseState
PatrolState: Khi mất target > LoseAggroTime
  → di chuyển random quanh spawnPosition
```

### 12.3 RangedEnemy

```
ChaseState: Tiến đến PreferredDistance
  → khi trong tầm → AttackState
AttackState: Bắn projectile mỗi AttackRate
  → projectile = pool object, bay thẳng đến player position lúc bắn
RetreatState: Khi player < 3m → lùi lại đến PreferredDistance
PatrolState: Như Melee
```

### 12.4 Enemy Projectile

```csharp
// EnemyProjectile : MonoBehaviour (pooled)
// Bay thẳng về hướng target khi bắn
// OnTriggerEnter(Player) → DealDamage → return to pool
// Lifetime → auto return to pool
```

---

## 13. Spawner System

```csharp
public class SpawnerController : MonoBehaviour
{
    [SerializeField] private SpawnerDataSO m_Data;    // hp, interval, count, enemyPool, dropTable
    [SerializeField] private VoidEventChannelSO m_OnSpawnerDestroyed;
    private HealthComponent m_Health;

    // Coroutine SpawnLoop:
    //   WaitForSeconds(interval * levelScaling.SpawnRateMult)
    //   count = Random.Range(countMin, countMax+1)
    //   For each: ObjectPool.Get(randomEnemyFromPool) → place in radius around spawner

    // OnDeath():
    //   StopCoroutine(SpawnLoop)
    //   LootDropper.SpawnDrop(m_Data.DropTable, position) // guaranteed
    //   m_OnSpawnerDestroyed.RaiseEvent()
}
```

---

## 14. Elite Boss System

```csharp
public class EliteBossController : MonoBehaviour
{
    [SerializeField] private EliteBossDataSO m_Data;
    [SerializeField] private VoidEventChannelSO m_OnEliteDefeated;
    private StateMachine m_StateMachine;
    private HealthComponent m_Health;
    private List<EliteSpellRuntime> m_Spells;  // Runtime cooldown tracking

    // Awake: tạo EliteSpellRuntime cho mỗi entry trong m_Data.SpellList
    // Activate(): Gọi từ LevelManager khi tất cả spawner chết
    //   → Enable GO, play spawn VFX/SFX
    //   → ChangeState(SpawnState) → sau animation → EvaluateState

    // EvaluateState:
    //   float dist = Distance(player)
    //   Duyệt m_Spells (sorted by CastPriority)
    //   Chọn spell phù hợp MinRange ≤ dist ≤ MaxRange && spell.IsReady
    //   → CastState(chosenSpell) hoặc MoveState nếu không có spell ready

    // OnDeath():
    //   SaveManager.UnlockSpell(m_Data.UnlockedSpellReward)
    //   m_OnEliteDefeated.RaiseEvent()
}

/// Runtime wrapper theo dõi cooldown cho từng spell của Elite Boss
[System.Serializable]
public class EliteSpellRuntime
{
    public EliteSpellEntry Entry;
    public float CurrentCooldown;
    public bool IsReady => CurrentCooldown <= 0f;
    public void StartCooldown() => CurrentCooldown = Entry.CooldownOverride;
    public void UpdateCooldown(float _deltaTime) => CurrentCooldown -= _deltaTime;
}
```

---

## 15. Loot & Drop System

### 15.1 LootDropper (Service)

```csharp
public class LootDropperService : MonoBehaviour
{
    [SerializeField] private SpellPoolService m_SpellPool;
    [SerializeField] private GameConfigSO m_Config;

    public void SpawnDrop(DropTableSO _table, Vector3 _position, LevelDataSO _levelData)
    {
        // 1. DropItemType result = _table.Roll()  (weighted random)
        // 2. if Nothing → return
        // 3. if SpellRandom → m_SpellPool.GetRandomFromDropPool(_levelData)
        //                    → ObjectPool.Get(SpellDropItem) → setup(spell, position)
        // 4. if PowerUpRandom → roll PowerUpType → ObjectPool.Get(PowerUpDropItem)
        // 5. if Heal → spawn Heal power-up specifically
    }
}
```

### 15.2 DropItem (Base)

```csharp
public abstract class DropItem : MonoBehaviour
{
    // OnEnable: StartCoroutine(LifetimeCountdown)
    // LifetimeCountdown:
    //   Wait(lifetime - warningTime) → bắt đầu nhấp nháy (flash renderer)
    //   Wait(warningTime) → Destroy / return to pool
    // OnTriggerEnter: if Player → OnPickup() → raise OnItemPickedUp event → return to pool
    public abstract void OnPickup(PlayerController _player);
}
```

### 15.3 LootBox

```csharp
public class LootBox : MonoBehaviour
{
    [SerializeField] private LootBoxDataSO m_Data;  // HP, dropTable, blocksPathfinding
    private HealthComponent m_Health;

    // Awake: m_Health.Initialize(m_Data.HP)
    // OnDeath: LootDropperService.SpawnDrop(m_Data.DropTable, position) // guaranteed
}
```

---

## 16. Power-Up System

```csharp
public enum PowerUpType { Heal, Haste, Shield, Invincible }

// PowerUpDropItem : DropItem
// OnPickup(_player):
//   switch(m_Type):
//     Heal → _player.Health.Heal(config.HealAmount)
//     Haste → _player.StatusEffect.ApplyHaste(config.HasteAmount, config.HasteDuration)
//     Shield → _player.StatusEffect.ApplyShield()
//     Invincible → _player.StatusEffect.ApplyInvincible(config.InvincibleDuration)
```

---

## 17. Health & Damage System

### 17.1 HealthComponent (Dùng chung)

```csharp
public class HealthComponent : MonoBehaviour
{
    [SerializeField] private int m_MaxHP;
    private int m_CurrentHP;

    // Local events (cùng GameObject — ví dụ: EnemyBase.HandleDeath)
    public System.Action OnDeath;

    // Cross-system events (optional, chỉ Player cần)
    [Header("SO Events (chỉ dùng cho Player)")]
    [SerializeField] private HPEventChannelSO m_OnHPChangedChannel;
    [SerializeField] private VoidEventChannelSO m_OnDeathChannel;

    public int CurrentHP => m_CurrentHP;
    public int MaxHP => m_MaxHP;

    public void Initialize(int _maxHP) { m_MaxHP = _maxHP; m_CurrentHP = _maxHP; }
    public void TakeDamage(int _amount)
    {
        // Check StatusEffect: Shield → absorb, Invincible → ignore
        // m_CurrentHP = Mathf.Max(0, m_CurrentHP - _amount)
        // m_OnHPChangedChannel?.RaiseEvent(new HPData { Current = m_CurrentHP, Max = m_MaxHP })
        // if m_CurrentHP <= 0 → OnDeath?.Invoke(); m_OnDeathChannel?.RaiseEvent()
    }
    public void Heal(int _amount)
    {
        // m_CurrentHP = Mathf.Min(m_MaxHP, m_CurrentHP + _amount)
        // m_OnHPChangedChannel?.RaiseEvent(...)
    }
}
```

### 17.2 DamageDealer

```csharp
/// Gắn lên projectile, spell hitbox, enemy attack collider
public class DamageDealer : MonoBehaviour
{
    [SerializeField] private int m_BaseDamage;
    [SerializeField] private LayerMask m_TargetLayers;   // Nhắm Enemy hoặc Player
    private float m_ScalingMult = 1f;                     // Từ LevelScalingConfig

    public void SetDamage(int _base, float _scaling) { m_BaseDamage = _base; m_ScalingMult = _scaling; }

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.TryGetComponent<HealthComponent>(out var health))
        {
            health.TakeDamage(Mathf.RoundToInt(m_BaseDamage * m_ScalingMult));
        }
    }
}
```

### 17.3 Damage Flow

```
Spell/Projectile (có DamageDealer component)
  → OnTriggerEnter → TryGetComponent<HealthComponent>
  → health.TakeDamage(baseDamage * scalingMult)
  → HealthComponent kiểm tra Shield / Invincible trước khi trừ HP
  → Raise SO Event (nếu Player) hoặc C# Action (nếu Enemy)
```

---

## 18. Status Effect System

```csharp
public class StatusEffectController : MonoBehaviour
{
    // Active effects (mỗi loại chỉ 1 instance, không stack):
    private bool m_IsShielded;
    private float m_InvincibleTimer;
    private float m_HasteTimer;
    private float m_HasteAmount;
    private float m_SlowAmount;
    private float m_SlowTimer;

    public bool IsInvincible => m_InvincibleTimer > 0f;
    public bool IsShielded => m_IsShielded;
    public float SpeedMultiplier =>
        (1f + (m_HasteTimer > 0 ? m_HasteAmount : 0f))
        * (1f - (m_SlowTimer > 0 ? m_SlowAmount : 0f));

    public void ApplyShield() { m_IsShielded = true; }
    public void ConsumeShield() { m_IsShielded = false; /* VFX */ }
    public void ApplyInvincible(float _duration) { m_InvincibleTimer = _duration; }
    public void ApplyHaste(float _amount, float _duration) { m_HasteAmount = _amount; m_HasteTimer = _duration; }
    public void ApplySlow(float _amount, float _duration)
    {
        // Lấy giá trị cao nhất (không stack)
        m_SlowAmount = Mathf.Max(m_SlowAmount, _amount);
        m_SlowTimer = _duration;
    }
    // Update: giảm timer theo deltaTime
}
```

---

## 19. Level Management

```csharp
public class LevelManager : MonoBehaviour
{
    [SerializeField] private LevelDataSO m_LevelData;
    [SerializeField] private VoidEventChannelSO m_OnAllSpawnersDestroyed;
    [SerializeField] private VoidEventChannelSO m_OnGameWin;
    [SerializeField] private EliteBossController m_EliteBoss;

    private List<SpawnerController> m_Spawners = new List<SpawnerController>();
    private int m_DestroyedSpawnerCount;

    // Spawner tự đăng ký (self-registration — không dùng FindObjectsOfType)
    public void RegisterSpawner(SpawnerController _spawner) => m_Spawners.Add(_spawner);
    public void UnregisterSpawner(SpawnerController _spawner) => m_Spawners.Remove(_spawner);

    // OnSpawnerDestroyed() (lắng nghe SO Event):
    //   m_DestroyedSpawnerCount++
    //   if m_DestroyedSpawnerCount >= m_Spawners.Count
    //     → m_OnAllSpawnersDestroyed.RaiseEvent()
    //     → m_EliteBoss.Activate()

    // OnEliteDefeated():
    //   → m_OnGameWin.RaiseEvent()
}
```

> **Self-registration pattern:** Mỗi `SpawnerController` gọi `LevelManager.RegisterSpawner(this)` trong `OnEnable` và `UnregisterSpawner(this)` trong `OnDisable`. Tránh dùng `FindObjectsOfType`.

**Spawner Counter Property (cho UI):**

```csharp
public int TotalSpawners => m_Spawners.Count;
public int DestroyedSpawners => m_DestroyedSpawnerCount;
```

---

## 20. Save & Progression System

```csharp
public class SaveManager : MonoBehaviour
{
    // Lưu bằng JsonUtility → PlayerPrefs hoặc File
    private SaveData m_SaveData;

    public void UnlockSpell(SpellDataSO _spell)
    {
        if (!m_SaveData.UnlockedSpellIds.Contains(_spell.Id))
        {
            m_SaveData.UnlockedSpellIds.Add(_spell.Id);
            SaveToDisk();
        }
    }

    public bool IsSpellUnlocked(SpellDataSO _spell)
        => m_SaveData.UnlockedSpellIds.Contains(_spell.Id);

    public List<string> GetUnlockedSpellIds() => m_SaveData.UnlockedSpellIds;
}

[System.Serializable]
public class SaveData
{
    public List<string> UnlockedSpellIds = new List<string>();
}
```

**Lưu trữ:** `Application.persistentDataPath + "/save.json"` — JSON format.

---

## 21. UI System

### 21.1 Kiến trúc UI

- Toàn bộ UI dùng **Unity Canvas (Screen Space - Overlay)**.
- Mỗi panel UI là 1 script lắng nghe **SO Event Channel** — không reference trực tiếp game system.
- Dùng **TextMeshProUGUI** cho toàn bộ text.

### 21.2 Danh sách UI Scripts & Event Binding

| Script | Lắng nghe Event | Hiển thị |
|---|---|---|
| `HPBarUI` | `OnPlayerHPChanged` | Thanh HP + text "80/100" |
| `StaminaBarUI` | `OnPlayerStaminaChanged` | Thanh Stamina |
| `SpellPanelUI` | `OnSpellEquipped`, `OnSpellCooldownUpdated` | Icon + tên + cooldown overlay |
| `SpawnerCounterUI` | `OnSpawnerDestroyed` | "Spawner: X / N" |
| `EliteHPBarUI` | `OnAllSpawnersDestroyed`, Elite HP event | Thanh HP Elite boss |
| `MinimapUI` | Poll hoặc event | Chấm đỏ/xanh/xám/vàng |
| `SpellSelectionUI` | — | Grid chọn phép khởi đầu trước level |
| `NotificationUI` | `OnItemPickedUp`, `OnSpellUnlocked` | Pop-up text fade ~1.5s |
| `WinScreenUI` | `OnGameWin` | Stats + Unlock info |
| `LoseScreenUI` | `OnGameLose` | Retry / Main Menu buttons |

### 21.3 SpellSelectionUI Flow

> **SpellSelection là UI panel overlay** trong Level scene, không phải scene riêng biệt. Hiển thị khi bắt đầu màn, ẩn sau khi chọn phép.

```
1. LevelManager yêu cầu SpellPoolService.GetRandomStartingSpells(N)
2. SpellSelectionUI hiển thị N phép: icon, tên, loại, mô tả, chỉ số chính
3. Player chọn 1 → PlayerSpellController.EquipSpell(chosenSpell)
4. Ẩn panel, bắt đầu Gameplay
```

---

## 22. Audio System

```csharp
public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioLibrarySO m_Library;
    // Singleton, DontDestroyOnLoad
    // 2 AudioSource: BGM (loop) + SFX pool (multiple one-shot)

    public void PlaySFX(string _id, float _volume = 1f)
    {
        // AudioClip clip = m_Library.GetClip(_id);
        // Play clip on SFX AudioSource
    }
    public void PlayBGM(string _id, float _fadeDuration = 1f);
    public void StopBGM(float _fadeDuration = 1f);
}
```

**AudioLibrarySO** — Unity không serialize `Dictionary`, dùng `List` + runtime lookup:

```csharp
[CreateAssetMenu(menuName = "Data/Audio Library")]
public class AudioLibrarySO : ScriptableObject
{
    [SerializeField] private List<AudioEntry> m_Entries;
    private Dictionary<string, AudioClip> m_Lookup;  // Build trong OnEnable

    public AudioClip GetClip(string _id)
    {
        if (m_Lookup == null) BuildLookup();
        return m_Lookup.TryGetValue(_id, out var clip) ? clip : null;
    }

    private void BuildLookup()
    {
        m_Lookup = new Dictionary<string, AudioClip>();
        foreach (var entry in m_Entries)
            m_Lookup[entry.Id] = entry.Clip;
    }
}

[System.Serializable]
public struct AudioEntry
{
    public string Id;        // ví dụ: "sfx_dash", "bgm_elite"
    public AudioClip Clip;
}
```

---

## 23. VFX System

- VFX dùng **URP Particle System** hoặc **VFX Graph** (tùy phức tạp).
- Mỗi VFX là prefab riêng, managed qua **Object Pool**.
- Spell prefab chứa VFX component con — auto play khi instantiate.
- `VFXManager` (optional): helper để spawn one-shot VFX tại vị trí bất kỳ.

```csharp
public class VFXManager : MonoBehaviour
{
    public void SpawnVFX(GameObject _vfxPrefab, Vector3 _position, float _duration)
    {
        // ObjectPool.Get(_vfxPrefab) → set position → auto return after _duration
    }
}
```

---

## 24. Object Pooling

### 24.1 Generic Pool

```csharp
public class ObjectPool<T> where T : MonoBehaviour
{
    private Queue<T> m_Pool;
    private T m_Prefab;
    private Transform m_Parent;

    public T Get()
    {
        // Dequeue hoặc Instantiate nếu pool rỗng
        // SetActive(true)
    }

    public void Return(T _obj)
    {
        // SetActive(false) → Enqueue
    }
}
```

### 24.2 Đối tượng cần pool bắt buộc

| Đối tượng | Estimated pool size | Lý do |
|---|---|---|
| Enemy (mỗi loại) | 20–30 | Spawner liên tục sinh |
| Spell Projectile (mỗi loại) | 10–15 | Cast liên tục |
| Enemy Projectile | 15 | Ranged enemy bắn |
| VFX (mỗi loại) | 5–10 | Hit effect, death effect |
| Drop Item | 10 | Nhiều enemy chết cùng lúc |

---

## 25. Phụ lục — Danh sách ScriptableObject cần tạo

### 25.1 Data Assets

| SO Type | Instances cần tạo | Đường dẫn |
|---|---|---|
| `SpellDataSO` | Fireball, Splash, InfernoBreathe, StaticField, Blizzard | `Data/Spells/` |
| `EnemyDataSO` | Melee, Ranged | `Data/Enemies/` |
| `EliteBossDataSO` | Elite_LV1, Elite_LV2, Elite_LV3 | `Data/EliteBosses/` |
| `SpawnerDataSO` | Tùy level design (mỗi spawner 1 instance) | `Data/Spawners/` |
| `LootBoxDataSO` | Tùy level design | `Data/LootBoxes/` |
| `LevelDataSO` | Level_01, Level_02, Level_03 | `Data/Levels/` |
| `DropTableSO` | MeleeDrop, RangedDrop, SpawnerDrop, LootBoxDrop | `Data/DropTables/` |
| `GameConfigSO` | GameConfig (1 instance duy nhất) | `Data/Config/` |
| `AudioLibrarySO` | AudioLibrary (1 instance) | `Data/Config/` |

### 25.2 Event Channel Assets

| SO Type | Instances | Đường dẫn |
|---|---|---|
| `VoidEventChannelSO` | OnSpawnerDestroyed, OnAllSpawnersDestroyed, OnEliteDefeated, OnPlayerDeath, OnGameWin, OnGameLose | `Data/Events/` |
| `HPEventChannelSO` | OnPlayerHPChanged | `Data/Events/` |
| `FloatEventChannelSO` | OnPlayerStaminaChanged, OnSpellCooldownUpdated | `Data/Events/` |
| `SpellDataEventChannelSO` | OnSpellEquipped, OnSpellUnlocked | `Data/Events/` |
| `StringEventChannelSO` | OnItemPickedUp | `Data/Events/` |

---

### Tổng kết ràng buộc thực thi

| # | Ràng buộc | Áp dụng |
|---|---|---|
| 1 | Mọi chỉ số gameplay → ScriptableObject, không hardcode | Toàn bộ |
| 2 | Object Pooling cho mọi object spawn thường xuyên | Enemy, Projectile, VFX, Drop |
| 3 | NavMesh cho AI pathfinding | Enemy, Elite Boss |
| 4 | SO Event Channel cho giao tiếp cross-system, C# Action cho local | Toàn bộ |
| 5 | New Input System | Player Input |
| 6 | URP render pipeline | Rendering |
| 7 | Cinemachine cho camera | Camera |
| 8 | TextMeshPro cho text | UI |
| 9 | State Machine pattern cho entity behavior | Player, Enemy, Elite |
| 10 | Single Responsibility — 1 class 1 việc | Toàn bộ scripts |
| 11 | Self-registration pattern thay vì FindObjectsOfType | Spawner → LevelManager |
| 12 | Physics Layer để ignore collision (Blizzard) thay vì disable collider | Player |

---

_Tài liệu v0.2 — Cập nhật: Sửa 22 issues từ rà soát (ISSUES.md). Thêm SpawnerDataSO, LootBoxDataSO, DamageDealer, EliteSpellRuntime, HPEventChannelSO. Đổi LootDropper/SpellPool sang service. HealthComponent dùng hybrid SO Event + C# Action._
