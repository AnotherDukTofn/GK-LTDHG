using UnityEngine;
using UnityEngine.UI;
using SpellStrike.Core.EventChannels;
using DG.Tweening;

namespace SpellStrike.UI
{
    /// <summary>
    /// Update HP bar khi nhận event từ HPEventChannelSO
    /// Script tạm thời, yêu cầu cài package TextMeshPro sau.
    /// </summary>
    public class HPBarUI : MonoBehaviour
    {
        [SerializeField] private HPEventChannelSO m_HPEventChannel;
        [SerializeField] private Image m_HPFillImage;
        [SerializeField] private Image m_HPBackgroundImage;
        [SerializeField] private float m_TweenDuration = 0.2f;

        private Tween m_HBTween;

        private void OnEnable()
        {
            if (m_HPEventChannel != null)
                m_HPEventChannel.Subscribe(OnHPChanged);
        }

        private void OnDisable()
        {
            if (m_HPEventChannel != null)
                m_HPEventChannel.Unsubscribe(OnHPChanged);
        }

        private void OnHPChanged(HPData _data)
        {
            Debug.Log($"[HPBarUI] Received HP Update: {_data.Current}/{_data.Max}");
            if (m_HPFillImage != null)
            {
                float targetFill = (float)_data.Current / _data.Max;
                
                // Kill existing tween if any
                m_HBTween?.Kill();
                
                // Start a new tween for smooth fill
                m_HBTween = m_HPFillImage.DOFillAmount(targetFill, m_TweenDuration).SetEase(Ease.OutQuad);
            }
        }
    }
}
