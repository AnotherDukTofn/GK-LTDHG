using UnityEngine;
using SpellStrike.Data;

namespace SpellStrike.Combat.Spells
{
    /// <summary>
    /// Phép dạng nội tại (Passive Aura). Tự động theo m_Caster và giật điện mỗi Tick.
    /// </summary>
    public class StaticFieldSpell : SpellBase
    {
        private float m_TickTimer;

        public override void Launch()
        {
            m_TickTimer = m_Data.BehaviorParams.TickRate;
            // TODO: Bật particle vòng điện vòng quanh
        }

        private void Update()
        {
            // StaticField tự động tick mỗi frame nó đang bật
            if (m_Caster == null) return; 

            m_TickTimer -= Time.deltaTime;

            if (m_TickTimer <= 0f)
            {
                ApplyZapTick();
                m_TickTimer = m_Data.BehaviorParams.TickRate;
            }
        }

        private void ApplyZapTick()
        {
            float radius = m_Data.BehaviorParams.AuraRadius;
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);

            foreach (var hit in hits)
            {
                // Chỉ zap kẻ địch, bỏ qua người chơi
                if (hit.gameObject != m_Caster.gameObject && hit.TryGetComponent<HealthComponent>(out var health))
                {
                    int dmg = Mathf.RoundToInt(m_Data.BehaviorParams.DamagePerTick * m_DamageMult);
                    health.TakeDamage(dmg);
                }
            }
        }
    }
}
