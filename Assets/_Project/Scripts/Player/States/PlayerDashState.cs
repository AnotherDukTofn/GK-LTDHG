using UnityEngine;

namespace SpellStrike.Player
{
    public class PlayerDashState : PlayerBaseState
    {
        public PlayerDashState(PlayerController _player) : base(_player) { }

        public override void Enter()
        {
            // Trừ stamina
            float cost = m_Player.Config != null ? m_Player.Config.DashStaminaCost : 25f;
            if (m_Player.Stamina != null)
            {
                m_Player.Stamina.TryConsumeStamina(cost);
            }

            // Hướng Dash: Ưu tiên hướng di chuyển. Nếu đứng im thì theo hướng nhìn hiện tại.
            Vector3 dir = m_Player.GetCameraRelativeMoveDirection();
            if (dir.sqrMagnitude < 0.01f)
            {
                dir = m_Player.transform.forward;
            }
            dir.Normalize();

            // Xoay mặt về hướng dash ngay lập tức
            m_Player.transform.rotation = Quaternion.LookRotation(dir);

            // Giao cho DashController chạy Coroutine
            // (State Machine sẽ đánh giá điều kiện thoát dựa trên DashController.IsDashing được check trong SetupStateMachine)
            m_Player.Dash.StartDash(dir);

            // TODO: Bật Trail Renderer / VFX / Audio
        }

        public override void Action()
        {
            // Dash logic di chuyển giật cục đã được Coroutine trong DashController xử lý qua CharacterController.Move()
            // Do đó ở đây không cần làm gì cả.
        }
    }
}
