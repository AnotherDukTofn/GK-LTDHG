using UnityEngine;
using UnityEngine.UI;
using SpellStrike.Core.EventChannels;

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
            if (m_HPFillImage != null)
            {
                m_HPFillImage.fillAmount = (float)_data.Current / _data.Max;
            }
        }
    }
}
