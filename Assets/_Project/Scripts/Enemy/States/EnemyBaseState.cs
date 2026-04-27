using SpellStrike.Core.StateMachine;

namespace SpellStrike.Enemy.States
{
    public abstract class EnemyBaseState : IState
    {
        protected readonly EnemyBase m_Enemy;

        public EnemyBaseState(EnemyBase _enemy)
        {
            m_Enemy = _enemy;
        }

        public virtual void Enter() { }
        public virtual void Action() { }
        public virtual void FixedAction() { }
        public virtual void Exit() { }
    }
}
