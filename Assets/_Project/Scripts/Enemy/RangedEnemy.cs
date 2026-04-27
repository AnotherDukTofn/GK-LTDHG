using UnityEngine;
using SpellStrike.Enemy.States;
using SpellStrike.Core.StateMachine;

namespace SpellStrike.Enemy
{
    public class RangedEnemy : EnemyBase
    {
        [Header("Ranged Setup")]
        [SerializeField] private GameObject m_ProjectilePrefab;
        [SerializeField] private Transform m_FirePoint;

        protected override void SetupStateMachine()
        {
            m_StateMachine = new StateMachine();

            // Khởi tạo states
            var chaseState = new EnemyChaseState(this);
            var attackState = new EnemyAttackState(this, m_Data.AttackRate, PerformShootAttack);
            var deadState = new EnemyDeadState(this);

            float attackRange = m_Data.AttackRange; 

            // Chase -> Attack
            m_StateMachine.AddTransition(chaseState, attackState, () => IsPlayerInRange(attackRange));
            
            // Attack -> Chase (Player lùi ra quá tầm đánh)
            m_StateMachine.AddTransition(attackState, chaseState, () => !IsPlayerInRange(attackRange * 1.1f));

            // Any -> Dead
            if (m_Health != null)
            {
                m_StateMachine.AddAnyTransition(deadState, () => m_Health.IsDead);
            }

            m_StateMachine.SetState(chaseState);
        }

        private void PerformShootAttack()
        {
            if (m_Animator != null) m_Animator.SetTrigger("Shoot");

            if (m_ProjectilePrefab != null && m_FirePoint != null)
            {
                // TODO: Chuyển sang dùng ObjectPool thay vì Instantiate
                GameObject projectile = Instantiate(m_ProjectilePrefab, m_FirePoint.position, m_FirePoint.rotation);
                
                if (projectile.TryGetComponent<EnemyProjectile>(out var p))
                {
                    p.Fire(transform.forward, (int)m_Data.Damage, m_Data.AttackRange);
                }
            }
        }
    }
}
