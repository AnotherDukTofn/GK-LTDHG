namespace SpellStrike.Player
{
    public class PlayerIdleState : PlayerBaseState
    {
        public PlayerIdleState(PlayerController _player) : base(_player) { }

        public override void Enter()
        {
            if (m_Player.Animator != null)
            {
                // Thay bằng Trigger/Bool anim phù hợp
                // m_Player.Animator.SetBool("IsMoving", false);
            }
        }

        public override void Action()
        {
            // Trong trạng thái Idle, hướng xoay tùy thuộc đang dùng phép hay không
            // Tạm thời nếu cast phép -> xoay theo hướng chuột
            if (m_Player.Input.IsCasting)
            {
                m_Player.RotateTowardsMouse();
            }
            
            // Lực kéo xuống ground đơn giản (Gravity)
            m_Player.CharController.Move(UnityEngine.Vector3.down * 9.81f * UnityEngine.Time.deltaTime);
        }
    }
}
