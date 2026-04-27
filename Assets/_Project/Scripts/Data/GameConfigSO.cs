using UnityEngine;

namespace SpellStrike.Data
{
    /// <summary>
    /// Cấu hình chung toàn game — 1 instance duy nhất.
    /// Exception: dùng public fields để đơn giản hóa readonly access (xem TDD §6.7).
    /// </summary>
    [CreateAssetMenu(menuName = "Data/Game Config")]
    public class GameConfigSO : ScriptableObject
    {
        [Header("Player")]
        public int PlayerMaxHP = 100;
        public int PlayerMaxStamina = 100;
        public float PlayerMoveSpeed = 5f;
        public float PlayerRotateSpeed = 720f;

        [Header("Dash")]
        public float DashDistance = 4f;
        public float DashDuration = 0.2f;
        public int DashStaminaCost = 25;
        public float DashIframeDuration = 0.15f;
        public float StaminaRegenDelay = 0.5f;
        public float StaminaRegenRate = 20f;

        [Header("Drop Lifetime")]
        public float SpellDropLifetime = 15f;
        public float PowerUpLifetime = 10f;
        public float DropWarningTime = 3f;

        [Header("Power-Up")]
        public int HealAmount = 25;
        public float HasteAmount = 0.6f;
        public float HasteDuration = 8f;
        public float InvincibleDuration = 5f;

        [Header("Camera")]
        public float CameraLerpSpeed = 5f;
    }
}
