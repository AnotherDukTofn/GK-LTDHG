using UnityEngine;

namespace SpellStrike.Data
{
    #region Enums

    public enum SpellCategory { Basic, Unlocked }

    public enum SpellBehavior
    {
        // Player spells
        AreaLob,              // Fireball
        PiercingProjectile,   // Splash
        ConeSpray,            // Inferno Breathe
        PassiveAura,          // Static Field
        Transform,            // Blizzard
        // Elite Boss spells
        Projectile,           // Đạn bay thẳng (không xuyên)
        Homing,               // Đạn bám mục tiêu
        MeleeBurst,           // Sóng lan cận chiến
        Beam                  // Tia liên tục
    }

    public enum SpellInputType { Click, Hold, Passive }

    #endregion

    /// <summary>
    /// Cấu hình hành vi — flat struct, mỗi spell chỉ dùng fields liên quan.
    /// Dùng custom PropertyDrawer (#if UNITY_EDITOR) để ẩn fields không cần thiết.
    /// </summary>
    [System.Serializable]
    public class SpellBehaviorParams
    {
        [Header("Projectile")]
        public float ProjectileSpeed;

        [Header("AoE / Lob")]
        public float AoeRadius;

        [Header("Cone")]
        public float ConeAngle;
        public float ConeRadius;

        [Header("Tick / DoT")]
        public float TickRate;
        public float DamagePerTick;

        [Header("Duration / Timing")]
        public float Duration;
        public float StartupDelay;

        [Header("Override")]
        public float RotateSpeedOverride;

        [Header("Status Effect")]
        public float SlowAmount;
        public float SlowDuration;

        [Header("Aura")]
        public float AuraRadius;

        [Header("Lobbing")]
        public float LobTravelTime;
    }

    /// <summary>
    /// Data-driven spell configuration — toàn bộ chỉ số phép thuật.
    /// Tạo asset: CreateAssetMenu → Data/Spell Data.
    /// </summary>
    [CreateAssetMenu(menuName = "Data/Spell Data")]
    public class SpellDataSO : ScriptableObject
    {
        #region Basic Info

        [Header("Basic Info")]
        [SerializeField] private string m_Id;
        [SerializeField] private string m_DisplayName;
        [SerializeField] private Sprite m_Icon;
        [SerializeField][TextArea] private string m_Description;

        #endregion

        #region Classification

        [Header("Classification")]
        [SerializeField] private SpellCategory m_Category;
        [SerializeField] private SpellBehavior m_Behavior;
        [SerializeField] private SpellInputType m_InputType;

        #endregion

        #region Stats

        [Header("Stats")]
        [SerializeField] private float m_Damage;
        [SerializeField] private float m_Cooldown;
        [SerializeField] private float m_Range;

        #endregion

        #region Behavior Params

        [Header("Behavior Params")]
        [SerializeField] private SpellBehaviorParams m_BehaviorParams;

        #endregion

        #region Prefabs

        [Header("Prefabs")]
        [SerializeField] private GameObject m_SpellPrefab;
        [SerializeField] private GameObject m_VfxPrefab;

        #endregion

        #region Public Properties

        public string Id => m_Id;
        public string DisplayName => m_DisplayName;
        public Sprite Icon => m_Icon;
        public string Description => m_Description;
        public SpellCategory Category => m_Category;
        public SpellBehavior Behavior => m_Behavior;
        public SpellInputType InputType => m_InputType;
        public float Damage => m_Damage;
        public float Cooldown => m_Cooldown;
        public float Range => m_Range;
        public SpellBehaviorParams BehaviorParams => m_BehaviorParams;
        public GameObject SpellPrefab => m_SpellPrefab;
        public GameObject VfxPrefab => m_VfxPrefab;

        #endregion
    }
}
