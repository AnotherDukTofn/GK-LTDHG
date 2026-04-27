using Cysharp.Threading.Tasks;
using System.Threading;
using SpellStrike.Data;
using SpellStrike.Combat;
using UnityEngine;

namespace SpellStrike.Player
{
    /// <summary>
    /// Component rời xử lý logic di chuyển giật cục khi Dash.
    /// Được gọi bởi PlayerDashState.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class DashController : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private GameConfigSO m_Config;
        
        #endregion

        #region Private Fields

        private CharacterController m_CharController;
        private StatusEffectController m_StatusEffect;
        private CancellationTokenSource m_DashCts;
        private bool m_IsDashing;

        #endregion

        #region Public Properties

        public bool IsDashing => m_IsDashing;
        public float DashSpeed => m_Config != null ? m_Config.DashDistance / m_Config.DashDuration : 20f;
        public float DashDuration => m_Config != null ? m_Config.DashDuration : 0.2f;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            m_CharController = GetComponent<CharacterController>();
            m_StatusEffect = GetComponent<StatusEffectController>();
        }

        private void OnDestroy()
        {
            if (m_DashCts != null)
            {
                m_DashCts.Cancel();
                m_DashCts.Dispose();
            }
        }

        #endregion

        #region Public Methods

        public void StartDash(Vector3 _direction)
        {
            if (m_IsDashing) return;
            
            if (m_DashCts != null)
            {
                m_DashCts.Cancel();
                m_DashCts.Dispose();
            }
            m_DashCts = new CancellationTokenSource();
            
            DashAsync(_direction, m_DashCts.Token).Forget();
        }

        #endregion

        #region Private Methods

        private async UniTaskVoid DashAsync(Vector3 _direction, CancellationToken _token)
        {
            m_IsDashing = true;
            
            // Bật i-frame
            if (m_StatusEffect != null && m_Config != null)
            {
                m_StatusEffect.ApplyInvincible(m_Config.DashIframeDuration);
            }

            float timer = 0f;
            float duration = DashDuration;
            float speed = DashSpeed;

            while (timer < duration)
            {
                // Dùng CharacterController.Move để có collision checking (không xuyên tường)
                m_CharController.Move(_direction * (speed * Time.deltaTime));
                timer += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: _token);
            }

            m_IsDashing = false;
        }

        #endregion
    }
}
