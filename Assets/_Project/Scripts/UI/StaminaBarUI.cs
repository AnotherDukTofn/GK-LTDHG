using UnityEngine;
using UnityEngine.UI;
using SpellStrike.Core.EventChannels;

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

        [Tooltip("Kéo thả các Image block vào đây theo thứ tự trái → phải.")]
        [SerializeField] private Image[] m_Segments;

        [Header("Colors")]
        [SerializeField] private Color m_FilledColor = new Color(1f, 0.6f, 0.2f, 1f);  // Orange
        [SerializeField] private Color m_EmptyColor  = new Color(0.4f, 0.4f, 0.4f, 0.4f); // Grey/dim

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
            if (m_Segments == null || m_Segments.Length == 0) return;

            int totalSegments = m_Segments.Length;
            int filledCount = Mathf.FloorToInt(_ratio * totalSegments);

            // Nếu ratio == 1 thì tất cả đều sáng
            if (_ratio >= 1f) filledCount = totalSegments;

            for (int i = 0; i < totalSegments; i++)
            {
                if (m_Segments[i] != null)
                {
                    m_Segments[i].color = (i < filledCount) ? m_FilledColor : m_EmptyColor;
                }
            }
        }

        #endregion
    }
}
