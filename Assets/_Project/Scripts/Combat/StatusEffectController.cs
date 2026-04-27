using UnityEngine;

namespace SpellStrike.Combat
{
    /// <summary>
    /// Component quản lý Shield, Invincible, Haste, Slow.
    /// Gắn lên Player và Enemy.
    /// </summary>
    public class StatusEffectController : MonoBehaviour
    {
        #region Private Fields

        private bool m_IsShielded;
        private float m_InvincibleTimer;
        private float m_HasteTimer;
        private float m_HasteAmount;
        private float m_SlowTimer;
        private float m_SlowAmount;

        #endregion

        #region Public Properties

        public bool IsInvincible => m_InvincibleTimer > 0f;
        public bool IsShielded => m_IsShielded;

        /// <summary>
        /// Trả về hệ số tốc độ (Ví dụ: 1.2 x 0.8 = 0.96).
        /// </summary>
        public float SpeedMultiplier
        {
            get
            {
                float haste = m_HasteTimer > 0f ? m_HasteAmount : 0f;
                float slow = m_SlowTimer > 0f ? m_SlowAmount : 0f;
                return (1f + haste) * (1f - slow);
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Update()
        {
            float dt = Time.deltaTime;

            if (m_InvincibleTimer > 0f) m_InvincibleTimer -= dt;
            if (m_HasteTimer > 0f) m_HasteTimer -= dt;
            if (m_SlowTimer > 0f) m_SlowTimer -= dt;
        }

        #endregion

        #region Public Methods

        public void ApplyShield()
        {
            m_IsShielded = true;
            // TODO: Bật VFX mảng chắn
        }

        public void ConsumeShield()
        {
            m_IsShielded = false;
            // TODO: Tắt VFX mảng chắn + phá vỡ
        }

        public void ApplyInvincible(float _duration)
        {
            m_InvincibleTimer = Mathf.Max(m_InvincibleTimer, _duration);
        }

        public void ApplyHaste(float _amount, float _duration)
        {
            m_HasteAmount = _amount;
            m_HasteTimer = Mathf.Max(m_HasteTimer, _duration);
        }

        public void ApplySlow(float _amount, float _duration)
        {
            // Chỉ lấy hiệu ứng slow mạnh nhất, không cộng dồn percent
            if (_amount > m_SlowAmount || m_SlowTimer <= 0f)
            {
                m_SlowAmount = _amount;
            }
            m_SlowTimer = Mathf.Max(m_SlowTimer, _duration);
        }

        public void ClearAllEffects()
        {
            m_IsShielded = false;
            m_InvincibleTimer = 0f;
            m_HasteTimer = 0f;
            m_SlowTimer = 0f;
        }

        #endregion
    }
}
