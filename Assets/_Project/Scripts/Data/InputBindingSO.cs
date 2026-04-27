using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SpellStrike.Data
{
    [CreateAssetMenu(fileName = "InputBindingData", menuName = "SpellStrike/Input Binding Data")]
    public class InputBindingSO : ScriptableObject
    {
        [Header("Legacy Input Settings")]
        public string HorizontalAxis = "Horizontal";
        public string VerticalAxis = "Vertical";
        public KeyCode DashKeyPrimary = KeyCode.Space;
        public KeyCode DashKeySecondary = KeyCode.LeftShift;
        public int DashMouseButton = 1; // 1 = Right Click
        public int CastMouseButton = 0; // 0 = Left Click

#if ENABLE_INPUT_SYSTEM
        [Header("New Input System Settings")]
        public Key MoveUpKey = Key.W;
        public Key MoveDownKey = Key.S;
        public Key MoveLeftKey = Key.A;
        public Key MoveRightKey = Key.D;
        
        public Key DashKey1 = Key.Space;
        public Key DashKey2 = Key.LeftShift;
#endif
    }
}
