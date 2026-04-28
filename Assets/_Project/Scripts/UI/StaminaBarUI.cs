using UnityEngine;
using UnityEngine.UI;
using SpellStrike.Core.EventChannels;
using DG.Tweening;

namespace SpellStrike.UI
{
    /// <summary>
    /// Stamina bar phân đoạn (segment) — mỗi block = 1 lần Dash.
    /// Nhận event FloatEventChannelSO (ratio 0-1) từ StaminaComponent.
    /// </summary>
    public class StaminaBarUI : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private FloatEventChannelSO m_StaminaEventChannel;

        [Header("Continuous Fill (Optional)")]
        [SerializeField] private Image m_MainFillImage;
        [SerializeField] private float m_TweenDuration = 0.15f;

        [Header("Segmented Mode")]
        [Tooltip("Kéo thả các Image block vào đây theo thứ tự trái → phải.")]
        [SerializeField] private Image[] m_Segments;

        [Header("Colors")]
        [SerializeField] private Color m_FilledColor = new Color(1f, 0.6f, 0.2f, 1f);  // Orange
        [SerializeField] private Color m_EmptyColor  = new Color(0.4f, 0.4f, 0.4f, 0.4f); // Grey/dim

        private DG.Tweening.Tween m_STween;
        private float m_LastRatio;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            if (m_StaminaEventChannel != null)
                m_StaminaEventChannel.Subscribe(OnStaminaChanged);
        }

        private void OnDisable()
        {
            if (m_StaminaEventChannel != null)
                m_StaminaEventChannel.Unsubscribe(OnStaminaChanged);
        }

        #endregion

        #region Event Callbacks

        private void OnStaminaChanged(float _ratio)
        {
            float delta = _ratio - m_LastRatio;
            m_LastRatio = _ratio;

            // 1. Chế độ thanh liền duy nhất
            if (m_MainFillImage != null)
            {
                m_STween?.Kill();
                if (delta < -0.01f) // Giảm nhanh -> Dùng Tween
                    m_STween = m_MainFillImage.DOFillAmount(_ratio, m_TweenDuration).SetEase(Ease.OutQuad);
                else // Hồi mượt -> Gán trực tiếp
                    m_MainFillImage.fillAmount = _ratio;
            }

            // 2. Chế độ chia đoạn (mỗi ô tự đầy/vơi mượt mà)
            if (m_Segments == null || m_Segments.Length == 0) return;

            int N = m_Segments.Length;
            for (int i = 0; i < N; i++)
            {
                if (m_Segments[i] == null) continue;

                // Tính toán target cho riêng segment này (thêm sai số nhỏ 0.001 để ổn định các thanh đã đầy)
                float segmentStart = (float)i / N;
                float targetFill = Mathf.Clamp01((_ratio - segmentStart) * N + 0.001f);

                if (delta < -0.01f) // Trường hợp mất Stamina (Dash) -> Dùng Tween để co lại mượt
                {
                    m_Segments[i].DOKill(); 
                    m_Segments[i].DOFillAmount(targetFill, m_TweenDuration).SetEase(Ease.OutQuad);
                }
                else // Trường hợp đang hồi -> Gán trực tiếp theo Frame để ko bị giật/trễ
                {
                    m_Segments[i].DOKill();
                    m_Segments[i].fillAmount = targetFill;
                }
                
                // Cập nhật màu (Có thể giữ nguyên màu sắc nếu bạn đã có background xám ở dưới)
                m_Segments[i].color = (targetFill > 0.01f) ? m_FilledColor : m_EmptyColor;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                OnStaminaChanged(m_LastRatio);
            }
        }
#endif

        #endregion
    }
}
