using UnityEngine;

namespace SpellStrike.Enemy.States
{
    public class EnemyAttackState : EnemyBaseState
    {
        private float m_AttackTimer;
        private readonly float m_AttackCooldown;
        private readonly System.Action m_OnAttackAction;

        public EnemyAttackState(EnemyBase _enemy, float _cooldown, System.Action _onAttackAction) : base(_enemy)
        {
            m_AttackCooldown = _cooldown;
            m_OnAttackAction = _onAttackAction;
        }

        public override void Enter()
        {
            // Reset timer để đánh ngay đòn đầu nếu muốn, hoặc chờ
            m_AttackTimer = m_AttackCooldown; // Tạm thiết kế đánh ngay đòn đầu
            
            if (m_Enemy.NavAgent.isOnNavMesh)
            {
                m_Enemy.NavAgent.isStopped = true;
            }
        }

        public override void Action()
        {
            // Xoay mặt về hướng player
            if (m_Enemy.PlayerTarget != null)
            {
                Vector3 dir = (m_Enemy.PlayerTarget.position - m_Enemy.transform.position).normalized;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.01f)
                {
                    m_Enemy.transform.rotation = Quaternion.Slerp(m_Enemy.transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
                }
            }

            m_AttackTimer -= Time.deltaTime;
            if (m_AttackTimer <= 0f)
            {
                m_OnAttackAction?.Invoke();
                m_AttackTimer = m_AttackCooldown;
            }
        }
    }
}
