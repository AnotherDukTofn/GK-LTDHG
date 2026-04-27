using UnityEngine;

namespace SpellStrike.Enemy.States
{
    public class EnemyChaseState : EnemyBaseState
    {
        public EnemyChaseState(EnemyBase _enemy) : base(_enemy) { }

        public override void Enter()
        {
            if (m_Enemy.Animator != null)
            {
                // m_Enemy.Animator.SetBool("IsMoving", true);
            }

            if (m_Enemy.NavAgent.isOnNavMesh)
            {
                m_Enemy.NavAgent.isStopped = false;
            }
        }

        public override void Action()
        {
            if (m_Enemy.PlayerTarget != null)
            {
                m_Enemy.NavAgent.SetDestination(m_Enemy.PlayerTarget.position);
            }
        }

        public override void Exit()
        {
            if (m_Enemy.NavAgent.isOnNavMesh)
            {
                m_Enemy.NavAgent.isStopped = true;
            }
        }
    }
}
