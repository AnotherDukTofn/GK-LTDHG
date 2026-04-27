using UnityEngine;

namespace SpellStrike.Combat
{
    public enum PowerUpType { Heal, MaxHPMultiplier, StaminaRegen, Invincible, Shield, Boost }

    public class PowerUpDropItem : DropItem
    {
        [SerializeField] private PowerUpType m_Type;
        [SerializeField] private float m_Value;
        [SerializeField] private float m_Duration;

        protected override void OnPickup(Player.PlayerController _player)
        {
            switch (m_Type)
            {
                case PowerUpType.Heal:
                    _player.Health?.Heal((int)m_Value);
                    break;
                case PowerUpType.MaxHPMultiplier:
                    // MaxHP implementation pending.
                    break;
                case PowerUpType.StaminaRegen:
                    // Tạm thời nạp thêm stamina tức thì
                    _player.Stamina?.TryConsumeStamina(-m_Value);
                    break;
                case PowerUpType.Invincible:
                    _player.StatusEffect?.ApplyInvincible(m_Duration);
                    break;
                case PowerUpType.Shield:
                    _player.StatusEffect?.ApplyShield();
                    break;
                case PowerUpType.Boost:
                    _player.StatusEffect?.ApplyHaste(m_Value, m_Duration);
                    break;
            }
        }
    }
}
