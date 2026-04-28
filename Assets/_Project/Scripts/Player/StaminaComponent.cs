using UnityEngine;
using SpellStrike.Data;
using SpellStrike.Core.EventChannels;

namespace SpellStrike.Player
{
    /// <summary>
    /// Component quản lý Stamina (dành riêng cho Player).
    /// Hồi phục theo thời gian sau một khoảng delay.
    /// </summary>
    public class StaminaComponent : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private GameConfigSO m_Config;
        [SerializeField] private FloatEventChannelSO m_OnStaminaChangedChannel;

        #endregion

        #region Private Fields

        private float m_CurrentStamina;
        private float m_RegenTimer;

        #endregion

        #region Public Properties

        public float CurrentStamina => m_CurrentStamina;
        public float MaxStamina => m_Config != null ? m_Config.PlayerMaxStamina : 100f;
        public float Ratio => m_CurrentStamina / MaxStamina;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            m_CurrentStamina = MaxStamina;
            RaiseEvent();
        }

        private void Update()
        {
            if (m_CurrentStamina >= MaxStamina || m_Config == null) return;

            if (m_RegenTimer > 0f)
            {
                m_RegenTimer -= Time.deltaTime;
            }
            else
            {
                m_CurrentStamina += m_Config.StaminaRegenRate * Time.deltaTime;
                if (m_CurrentStamina > MaxStamina) m_CurrentStamina = MaxStamina;
                RaiseEvent();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                RaiseEvent();
            }
        }
#endif

        #endregion

        #region Public Methods

        public bool HasEnoughStamina(float _amount)
        {
            return m_CurrentStamina >= _amount;
        }

        public bool TryConsumeStamina(float _amount)
        {
            if (HasEnoughStamina(_amount))
            {
                m_CurrentStamina -= _amount;
                m_RegenTimer = m_Config.StaminaRegenDelay;
                RaiseEvent();
                return true;
            }
            return false;
        }

        #endregion

        #region Private Methods

        private void RaiseEvent()
        {
            m_OnStaminaChangedChannel?.RaiseEvent(Ratio);
        }

        #endregion
    }
}
