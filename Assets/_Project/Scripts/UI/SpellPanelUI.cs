using UnityEngine;
using UnityEngine.UI;
using SpellStrike.Data;
using SpellStrike.Core.EventChannels;

namespace SpellStrike.UI
{
    /// <summary>
    /// Hiển thị Icon của spell và overlay bóng mờ cooldown của nó ở góc màn hình.
    /// Script tạm thời, yêu cầu TextMeshPro.
    /// </summary>
    public class SpellPanelUI : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Channels")]
        [SerializeField] private SpellDataEventChannelSO m_OnSpellEquippedChannel;
        [SerializeField] private FloatEventChannelSO m_OnCooldownTickChannel;

        [Header("UI Elements")]
        [SerializeField] private Image m_IconImage;
        [Tooltip("Image type phải là Filled (360 radial hoặc Vertical/Horizontal)")]
        [SerializeField] private Image m_CooldownOverlay;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            if (m_OnSpellEquippedChannel != null) m_OnSpellEquippedChannel.Subscribe(OnEquip);
            if (m_OnCooldownTickChannel != null) m_OnCooldownTickChannel.Subscribe(OnCooldownTick);
        }

        private void OnDisable()
        {
            if (m_OnSpellEquippedChannel != null) m_OnSpellEquippedChannel.Unsubscribe(OnEquip);
            if (m_OnCooldownTickChannel != null) m_OnCooldownTickChannel.Unsubscribe(OnCooldownTick);
        }

        private void Start()
        {
            if (m_IconImage != null) m_IconImage.enabled = false;
            if (m_CooldownOverlay != null) m_CooldownOverlay.fillAmount = 0f;
        }

        #endregion

        #region Event Callbacks

        private void OnEquip(SpellDataSO _data)
        {
            if (_data != null && m_IconImage != null)
            {
                m_IconImage.enabled = true;
                m_IconImage.sprite = _data.Icon;
            }
        }

        private void OnCooldownTick(float _ratio)
        {
            if (m_CooldownOverlay != null)
            {
                m_CooldownOverlay.fillAmount = _ratio;
            }
        }

        #endregion
    }
}
