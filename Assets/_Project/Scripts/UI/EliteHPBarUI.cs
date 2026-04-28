using UnityEngine;
using UnityEngine.UI;
using SpellStrike.Core.EventChannels;
using DG.Tweening;

namespace SpellStrike.UI
{
    public class EliteHPBarUI : MonoBehaviour
    {
        [SerializeField] private HPEventChannelSO m_BossHPChannel;
        [SerializeField] private StringEventChannelSO m_BossNameChannel;
        [SerializeField] private Image m_HPFillImage;
        [SerializeField] private Text m_EliteNameText;
        [SerializeField] private CanvasGroup m_CanvasGroup;
        [SerializeField] private float m_TweenDuration = 0.25f;

        private Tween m_HBTween;

        private void OnEnable()
        {
            if (m_BossHPChannel != null)
                m_BossHPChannel.Subscribe(OnHPChanged);
            if (m_BossNameChannel != null)
                m_BossNameChannel.Subscribe(OnNameReceived);
        }

        private void OnDisable()
        {
            if (m_BossHPChannel != null)
                m_BossHPChannel.Unsubscribe(OnHPChanged);
            if (m_BossNameChannel != null)
                m_BossNameChannel.Unsubscribe(OnNameReceived);
        }

        private void Start()
        {
            if (m_CanvasGroup != null) m_CanvasGroup.alpha = 0f; // Ẩn lúc đầu
        }

        private void OnNameReceived(string _name)
        {
            if (m_EliteNameText != null)
                m_EliteNameText.text = _name;
        }

        private void OnHPChanged(HPData _data)
        {
            Debug.Log($"[EliteHPBarUI] Received Boss HP Update: {_data.Current}/{_data.Max}");
            if (m_CanvasGroup != null) m_CanvasGroup.alpha = 1f; // Hiện ra khi đánh boss

            if (m_HPFillImage != null)
            {
                float targetFill = (float)_data.Current / _data.Max;
                
                // Kill existing tween if any
                m_HBTween?.Kill();
                
                // Start a new tween for smooth fill
                m_HBTween = m_HPFillImage.DOFillAmount(targetFill, m_TweenDuration).SetEase(Ease.OutQuad);
            }

            if (_data.Current <= 0f)
            {
                if (m_CanvasGroup != null) m_CanvasGroup.alpha = 0f;
            }
        }
    }
}
