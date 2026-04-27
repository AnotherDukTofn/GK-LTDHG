using System.Collections.Generic;
using UnityEngine;

namespace SpellStrike.Data
{
    [CreateAssetMenu(menuName = "Data/Elite Boss Data")]
    public class EliteBossDataSO : ScriptableObject
    {
        #region Serialized Fields

        [Header("Basic Info")]
        [SerializeField] private string m_Id;
        [SerializeField] private string m_DisplayName;

        [Header("Stats")]
        [SerializeField] private int m_HP;
        [SerializeField] private float m_Speed;

        [Header("Spell List")]
        [SerializeField] private List<EliteSpellEntry> m_SpellList;

        [Header("Reward")]
        [SerializeField] private SpellDataSO m_UnlockedSpellReward;

        [Header("Prefab")]
        [SerializeField] private GameObject m_Prefab;

        #endregion

        #region Public Properties

        public string Id => m_Id;
        public string DisplayName => m_DisplayName;
        public int HP => m_HP;
        public float Speed => m_Speed;
        public List<EliteSpellEntry> SpellList => m_SpellList;
        public SpellDataSO UnlockedSpellReward => m_UnlockedSpellReward;
        public GameObject Prefab => m_Prefab;

        #endregion
    }

    /// <summary>
    /// Cấu hình từng spell của Elite Boss — có override damage/cooldown riêng.
    /// </summary>
    [System.Serializable]
    public class EliteSpellEntry
    {
        [Tooltip("Base data: behavior type, VFX prefab")]
        public SpellDataSO SpellData;
        public int CastPriority;
        public float MinRange;
        public float MaxRange;

        [Header("Override — độc lập với player spell")]
        public float DamageOverride;
        public float CooldownOverride;
        public float ProjectileSpeedOverride;
    }

    /// <summary>
    /// Runtime wrapper theo dõi cooldown cho từng spell của Elite Boss.
    /// Tạo lúc Awake từ EliteSpellEntry.
    /// </summary>
    [System.Serializable]
    public class EliteSpellRuntime
    {
        public EliteSpellEntry Entry;
        public float CurrentCooldown;

        public bool IsReady => CurrentCooldown <= 0f;

        public EliteSpellRuntime(EliteSpellEntry _entry)
        {
            Entry = _entry;
            CurrentCooldown = 0f;
        }

        public void StartCooldown() => CurrentCooldown = Entry.CooldownOverride;

        public void UpdateCooldown(float _deltaTime)
        {
            if (CurrentCooldown > 0f)
                CurrentCooldown -= _deltaTime;
        }
    }
}
