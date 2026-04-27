using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SpellStrike.Player
{
    /// <summary>
    /// Component đọc input từ New Input System và public ra property cho Player FSM.
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField, Tooltip("Data asset chứa cấu hình binding phím")] 
        private Data.InputBindingSO m_InputBinding;

        #endregion

        #region Public Properties

        public Vector2 MoveInput { get; private set; }
        public Vector2 MousePosition { get; private set; }
        public bool IsCasting { get; private set; }
        public bool IsDashing { get; private set; }

        // Event bắn một lần khi nhấn nút
        public Action OnCastStarted;
        public Action OnCastCanceled;
        public Action OnDashPressed;

        #endregion

        #region Unity Lifecycle

        private void Update()
        {
            #if ENABLE_INPUT_SYSTEM
                        ReadInputSystem();
            #else
                        ReadLegacyInput();
            #endif
        }

        #endregion

        #region Input Reading

        private void ReadLegacyInput()
        {
            if (m_InputBinding != null)
            {
                MoveInput = new Vector2(Input.GetAxisRaw(m_InputBinding.HorizontalAxis), Input.GetAxisRaw(m_InputBinding.VerticalAxis)).normalized;
                MousePosition = Input.mousePosition;

                bool wasCasting = IsCasting;
                IsCasting = Input.GetMouseButton(m_InputBinding.CastMouseButton);

                if (IsCasting && !wasCasting) 
                {
                    Debug.Log("[PlayerInputHandler] Cast Started (Legacy)");
                    OnCastStarted?.Invoke();
                }
                if (!IsCasting && wasCasting) 
                {
                    Debug.Log("[PlayerInputHandler] Cast Canceled (Legacy)");
                    OnCastCanceled?.Invoke();
                }

                IsDashing = Input.GetKeyDown(m_InputBinding.DashKeyPrimary) || Input.GetKeyDown(m_InputBinding.DashKeySecondary) || Input.GetMouseButtonDown(m_InputBinding.DashMouseButton);
                if (IsDashing) OnDashPressed?.Invoke();
            }
            else
            {
                // Fallback nếu không gán SO
                MoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
                MousePosition = Input.mousePosition;

                bool wasCasting = IsCasting;
                IsCasting = Input.GetMouseButton(0); // Left click

                if (IsCasting && !wasCasting) OnCastStarted?.Invoke();
                if (!IsCasting && wasCasting) OnCastCanceled?.Invoke();

                IsDashing = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(1); // Space, Shift or Right Click
                if (IsDashing) OnDashPressed?.Invoke();
            }
        }

        #if ENABLE_INPUT_SYSTEM
                private void ReadInputSystem()
                {
                    if (Keyboard.current != null)
                    {
                        if (m_InputBinding != null)
                        {
                            var x = Keyboard.current[m_InputBinding.MoveRightKey].ReadValue() - Keyboard.current[m_InputBinding.MoveLeftKey].ReadValue();
                            var y = Keyboard.current[m_InputBinding.MoveUpKey].ReadValue() - Keyboard.current[m_InputBinding.MoveDownKey].ReadValue();
                            MoveInput = new Vector2(x, y).normalized;

                            bool didDash = Keyboard.current[m_InputBinding.DashKey1].wasPressedThisFrame || Keyboard.current[m_InputBinding.DashKey2].wasPressedThisFrame;
                            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) didDash = true;
                            
                            IsDashing = didDash;
                            if (IsDashing) OnDashPressed?.Invoke();
                        }
                        else
                        {
                            var x = Keyboard.current.dKey.ReadValue() - Keyboard.current.aKey.ReadValue();
                            var y = Keyboard.current.wKey.ReadValue() - Keyboard.current.sKey.ReadValue();
                            MoveInput = new Vector2(x, y).normalized;

                            bool didDash = Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.shiftKey.wasPressedThisFrame;
                            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) didDash = true;
                            
                            IsDashing = didDash;
                            if (IsDashing) OnDashPressed?.Invoke();
                        }
                    }

                    if (Mouse.current != null)
                    {
                        MousePosition = Mouse.current.position.ReadValue();
                        
                        bool wasCasting = IsCasting;
                        IsCasting = Mouse.current.leftButton.isPressed;

                        if (IsCasting && !wasCasting) OnCastStarted?.Invoke();
                        if (!IsCasting && wasCasting) OnCastCanceled?.Invoke();
                    }
                }
        #endif

        #endregion
    }
}
