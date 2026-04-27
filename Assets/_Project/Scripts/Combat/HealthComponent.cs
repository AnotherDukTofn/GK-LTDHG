using System;
using SpellStrike.Core.EventChannels;
using UnityEngine;

namespace SpellStrike.Combat
{
    /// <summary>
    /// File quản lý HP dùng chung. Hỗ trợ Local Event (Action) và Global Event (SO).
    /// </summary>
    public class HealthComponent : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Global Events (Optional - For Player)")]
        [SerializeField] private HPEventChannelSO m_OnHPChangedChannel;
        [SerializeField] private VoidEventChannelSO m_OnDeathChannel;

        #endregion

        #region Private Fields

        [SerializeField] private int m_MaxHP;
        [SerializeField] private int m_CurrentHP;
        private StatusEffectController m_StatusEffect;
        private bool m_IsDead;

        #endregion

        #region Public Properties

        public int CurrentHP => m_CurrentHP;
        public int MaxHP => m_MaxHP;
        public bool IsDead => m_IsDead;

        #endregion

        #region Local Events

        /// <summary>
        /// Lắng nghe trực tiếp bởi EnemyBase/PlayerController trên cùng GameObject.
        /// </summary>
        public Action OnDeathLocal;
        public Action<int, int> OnDamageTakenLocal; // (amount, direction/knockback info can be added later)

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            m_StatusEffect = GetComponent<StatusEffectController>();
        }

        #endregion

        #region Public Methods

        public void Initialize(int _maxHP)
        {
            m_MaxHP = _maxHP;
            m_CurrentHP = _maxHP;
            m_IsDead = false;
            RaiseHPChangeEvent();
        }

        public void TakeDamage(int _amount)
        {
            if (m_IsDead) return;

            // Check Status Effects
            if (m_StatusEffect != null)
            {
                if (m_StatusEffect.IsInvincible) return;

                if (m_StatusEffect.IsShielded)
                {
                    m_StatusEffect.ConsumeShield();
                    return; // Chặn toàn bộ damage 1 hit
                }
            }

            m_CurrentHP = Mathf.Max(0, m_CurrentHP - _amount);
            OnDamageTakenLocal?.Invoke(_amount, m_CurrentHP);
            RaiseHPChangeEvent();

            if (m_CurrentHP <= 0)
            {
                Die();
            }
        }

        public void Heal(int _amount)
        {
            if (m_IsDead || _amount <= 0) return;

            m_CurrentHP = Mathf.Min(m_MaxHP, m_CurrentHP + _amount);
            RaiseHPChangeEvent();
        }

        public void KillInstantly()
        {
            if (m_IsDead) return;
            m_CurrentHP = 0;
            RaiseHPChangeEvent();
            Die();
        }

        #endregion

        #region Private Methods

        private void RaiseHPChangeEvent()
        {
            m_OnHPChangedChannel?.RaiseEvent(new HPData(m_CurrentHP, m_MaxHP));
        }

        private void Die()
        {
            m_IsDead = true;
            OnDeathLocal?.Invoke();
            m_OnDeathChannel?.RaiseEvent();
        }

        #endregion
    }
}
