namespace SpellStrike.Core.StateMachine
{
    /// <summary>
    /// Interface cho mỗi state trong FSM.
    /// Implement bởi PlayerIdleState, EnemyChaseState, v.v.
    /// </summary>
    public interface IState
    {
        void Enter();
        void Action();
        void FixedAction();
        void Exit();
    }
}
