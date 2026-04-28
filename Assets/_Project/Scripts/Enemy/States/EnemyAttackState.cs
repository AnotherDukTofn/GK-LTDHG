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
            // Bắt đầu hồi chiêu từ 0 để có thể đánh ngay lập tức khi vừa chạm vào tầm đánh
            m_AttackTimer = 0f; 
            
            // Không nên set isStopped cứng ở đây nếu muốn quái vẫn nhích theo player
            // Ta sẽ xử lý dừng ở phần Action hoặc qua stoppingDistance
        }

        public override void Action()
        {
            if (m_Enemy.PlayerTarget == null) return;

            // Vẫn cho phép NavAgent bám đuổi nhẹ nếu ở trong AttackState nhưng chưa quá sát
            // hoặc đơn giản là để stoppingDistance của NavMeshAgent tự lo.
            if (m_Enemy.NavAgent.isOnNavMesh)
            {
                m_Enemy.NavAgent.SetDestination(m_Enemy.PlayerTarget.position);
            }

            // Xoay mặt về hướng player
            Vector3 dir = (m_Enemy.PlayerTarget.position - m_Enemy.transform.position).normalized;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
            {
                m_Enemy.transform.rotation = Quaternion.Slerp(m_Enemy.transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 7f);
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
