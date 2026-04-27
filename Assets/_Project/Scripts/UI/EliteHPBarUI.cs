using UnityEngine;
using UnityEngine.UI;
using SpellStrike.Core.EventChannels;

namespace SpellStrike.UI
{
    public class EliteHPBarUI : MonoBehaviour
    {
        [SerializeField] private HPEventChannelSO m_BossHPChannel;
        [SerializeField] private StringEventChannelSO m_BossNameChannel;
        [SerializeField] private Image m_HPFillImage;
        [SerializeField] private Text m_EliteNameText;
        [SerializeField] private CanvasGroup m_CanvasGroup;

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
            if (m_CanvasGroup != null) m_CanvasGroup.alpha = 1f; // Hiện ra khi đánh boss

            if (m_HPFillImage != null)
            {
                m_HPFillImage.fillAmount = (float)_data.Current / _data.Max;
            }

            if (_data.Current <= 0f)
            {
                if (m_CanvasGroup != null) m_CanvasGroup.alpha = 0f;
            }
        }
    }
}
