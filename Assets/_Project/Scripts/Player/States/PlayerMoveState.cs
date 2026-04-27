using UnityEngine;

namespace SpellStrike.Player
{
    public class PlayerMoveState : PlayerBaseState
    {
        public PlayerMoveState(PlayerController _player) : base(_player) { }

        public override void Enter()
        {
            if (m_Player.Animator != null)
            {
                // m_Player.Animator.SetBool("IsMoving", true);
            }
        }

        public override void Action()
        {
            Vector3 moveDir = m_Player.GetCameraRelativeMoveDirection();

            // Tính Speed thật
            float speed = m_Player.Config != null ? m_Player.Config.PlayerMoveSpeed : 5f;
            if (m_Player.StatusEffect != null)
            {
                speed *= m_Player.StatusEffect.SpeedMultiplier;
            }

            // Xoay: Nếu đang cast thì xoay theo chuột, nếu không xoay theo hướng đi
            if (m_Player.Input.IsCasting)
            {
                m_Player.RotateTowardsMouse();
            }
            else
            {
                m_Player.RotateTowardsMoveDirection();
            }

            // Di chuyển bằng CharacterController
            Vector3 finalMove = moveDir * speed + Vector3.down * 9.81f; // Gravity
            m_Player.CharController.Move(finalMove * Time.deltaTime);
        }
    }
}
