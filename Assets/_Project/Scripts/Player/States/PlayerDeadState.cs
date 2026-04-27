namespace SpellStrike.Player
{
    public class PlayerDeadState : PlayerBaseState
    {
        public PlayerDeadState(PlayerController _player) : base(_player) { }

        public override void Enter()
        {
            if (m_Player.Animator != null)
            {
                // m_Player.Animator.SetTrigger("Die");
            }

            // Tắt va chạm để quái không bị kẹt khi bước qua
            if (m_Player.CharController != null)
            {
                m_Player.CharController.enabled = false;
            }
        }
    }
}
