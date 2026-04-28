using UnityEngine;
using SpellStrike.Enemy.States;
using SpellStrike.Core.StateMachine;

namespace SpellStrike.Enemy
{
    public class MeleeEnemy : EnemyBase
    {
        protected override void SetupStateMachine()
        {
            m_StateMachine = new StateMachine();

            // Setup các States cụ thể
            var chaseState = new EnemyChaseState(this);
            var attackState = new EnemyAttackState(this, m_Data.AttackRate, PerformSlamAttack);
            var deadState = new EnemyDeadState(this);

            // Khoảng cách theo Data
            float attackRange = m_Data.AttackRange;

            // FSM Transitions
            // Luôn ở ChaseState trừ khi vào tầm đánh
            m_StateMachine.AddTransition(chaseState, attackState, () => IsPlayerInRange(attackRange));
            
            // Từ Attack -> Chase nếu player lùi ra khỏi tầm đánh
            m_StateMachine.AddTransition(attackState, chaseState, () => !IsPlayerInRange(attackRange * 1.1f)); // 1.1 buffer

            // Any -> Dead
            if (m_Health != null)
            {
                m_StateMachine.AddAnyTransition(deadState, () => m_Health.IsDead);
            }

            m_StateMachine.SetState(chaseState);
        }

        private void PerformSlamAttack()
        {
            if (m_Animator != null) m_Animator.SetTrigger("Attack");

            // Logic hit đơn giản thay vì Animation Event, sử dụng OverlapSphere
            Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward, m_Data.AttackRange);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    // Tìm HealthComponent ở object đó hoặc cha/con của nó
                    var hp = hit.GetComponentInParent<Combat.HealthComponent>() ?? hit.GetComponentInChildren<Combat.HealthComponent>();
                    if (hp != null)
                    {
                        hp.TakeDamage((int)m_Data.Damage);
                    }
                }
            }
        }
    }
}
