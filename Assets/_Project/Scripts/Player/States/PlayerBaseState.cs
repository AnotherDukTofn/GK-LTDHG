using SpellStrike.Core.StateMachine;

namespace SpellStrike.Player
{
    public abstract class PlayerBaseState : IState
    {
        protected readonly PlayerController m_Player;

        public PlayerBaseState(PlayerController _player)
        {
            m_Player = _player;
        }

        public virtual void Enter() { }
        public virtual void Action() { }
        public virtual void FixedAction() { }
        public virtual void Exit() { }
    }
}
