using UnityEngine;

namespace SpellStrike.Enemy.States
{
    public class EnemyDeadState : EnemyBaseState
    {
        public EnemyDeadState(EnemyBase _enemy) : base(_enemy) { }

        public override void Enter()
        {
            if (m_Enemy.NavAgent.isOnNavMesh)
            {
                m_Enemy.NavAgent.isStopped = true;
                m_Enemy.NavAgent.enabled = false;
            }

            // Gọi hàm Die để lo liệu xóa object/spawn loot
            m_Enemy.Die();
        }
    }
}
