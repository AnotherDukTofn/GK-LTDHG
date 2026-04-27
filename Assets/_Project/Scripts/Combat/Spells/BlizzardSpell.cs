using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using SpellStrike.Data;

namespace SpellStrike.Combat.Spells
{
    /// <summary>
    /// Phép hóa bão tuyết (Transform). Cho phép người chơi di chuyển xuyên quái và gây sát thương.
    /// Gọi Player biến đổi State.
    /// </summary>
    public class BlizzardSpell : SpellBase
    {
        public override void Launch()
        {
            // Tính năng làm player IgnoreCollision với Enemy
            int playerLayer = LayerMask.NameToLayer("Player");
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            
            Physics.IgnoreLayerCollision(playerLayer, enemyLayer, true);

            // Bật State đặc biệt cho Player (hoặc áp dụng hiệu ứng trực tiếp)
            BlizzardDurationAsync(playerLayer, enemyLayer, m_SpellCts.Token).Forget();
        }

        private async UniTaskVoid BlizzardDurationAsync(int _playerLayer, int _enemyLayer, CancellationToken _token)
        {
            float timer = 0f;
            float duration = m_Data.BehaviorParams.Duration > 0f ? m_Data.BehaviorParams.Duration : 3f;
            float tickRate = m_Data.BehaviorParams.TickRate > 0f ? m_Data.BehaviorParams.TickRate : 0.5f;
            float nextTick = tickRate;

            // Optional: Player invincibility
            if (m_Caster != null && m_Caster.TryGetComponent<StatusEffectController>(out var status))
            {
                status.ApplyInvincible(duration);
            }

            while (timer < duration)
            {
                if (m_Caster == null) 
                {
                    // Caster đã bị hủy, dừng phép thuật lại ngay lập tức
                    break;
                }

                timer += Time.deltaTime;

                // Áp dụng sát thương theo khu vực đang đứng mỗi chu kỳ
                if (timer >= nextTick)
                {
                    ApplyStormDamage();
                    nextTick += tickRate;
                }

                transform.position = m_Caster.position; // Bám theo Caster

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: _token);
            }

            // Tắt hiệu ứng xuyên thấu
            Physics.IgnoreLayerCollision(_playerLayer, _enemyLayer, false);

            SpellPoolService.Instance.ReturnSpell(m_Data.Id, this);
        }

        private void ApplyStormDamage()
        {
            float radius = m_Data.BehaviorParams.AoeRadius;
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);

            foreach (var hit in hits)
            {
                if (hit.gameObject != m_Caster.gameObject && hit.TryGetComponent<HealthComponent>(out var health))
                {
                    int dmg = Mathf.RoundToInt(m_Data.BehaviorParams.DamagePerTick * m_DamageMult);
                    health.TakeDamage(dmg);
                }

                if (hit.TryGetComponent<StatusEffectController>(out var effect))
                {
                    effect.ApplySlow(m_Data.BehaviorParams.SlowAmount, m_Data.BehaviorParams.SlowDuration);
                }
            }
        }
    }
}
