using UnityEngine;
using SpellStrike.Data;

namespace SpellStrike.Combat.Spells
{
    /// <summary>
    /// Phép dạng tia nón/thổi lửa. Yêu cầu nhấn giữ (Hold).
    /// Áp dụng sát thương theo định kỳ (TickRate) và hiệu ứng Slow.
    /// Chỉ gây sát thương cho Enemy layer.
    /// </summary>
    public class InfernoBreatheSpell : SpellBase
    {
        [Header("Targeting")]
        [SerializeField] private LayerMask m_EnemyLayer;

        [Header("Visuals")]
        [SerializeField] private ParticleSystem m_FlameVFX;

        private float m_TickTimer;

        public override void Launch()
        {
            if (m_FlameVFX != null) m_FlameVFX.Play();
            m_TickTimer = m_Data.BehaviorParams.TickRate;
        }

        public override void HoldTick()
        {
            m_TickTimer -= Time.deltaTime;

            if (m_TickTimer <= 0f)
            {
                ApplyBurnTick();
                m_TickTimer = m_Data.BehaviorParams.TickRate; // reset timer
            }
        }

        public override void EndHold()
        {
            if (m_FlameVFX != null) m_FlameVFX.Stop();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (m_FlameVFX != null) m_FlameVFX.Stop();
        }

        private void ApplyBurnTick()
        {
            // Quét các kẻ địch trong vùng hình nón phía trước mặt nhân vật
            float radius = m_Data.BehaviorParams.ConeRadius;
            float angle = m_Data.BehaviorParams.ConeAngle;

            Collider[] hits = Physics.OverlapSphere(m_Caster.position, radius, m_EnemyLayer);

            foreach (var hit in hits)
            {
                Vector3 directionToTarget = (hit.transform.position - m_Caster.position).normalized;
                if (Vector3.Angle(m_Caster.forward, directionToTarget) <= angle / 2f)
                {
                    if (hit.TryGetComponent<HealthComponent>(out var health))
                    {
                        int dmg = Mathf.RoundToInt(m_Data.BehaviorParams.DamagePerTick * m_DamageMult);
                        health.TakeDamage(dmg);
                    }

                    if (hit.TryGetComponent<StatusEffectController>(out var status))
                    {
                        status.ApplySlow(m_Data.BehaviorParams.SlowAmount, m_Data.BehaviorParams.SlowDuration);
                    }
                }
            }
        }
    }
}
