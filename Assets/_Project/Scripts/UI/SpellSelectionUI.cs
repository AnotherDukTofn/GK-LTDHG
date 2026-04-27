using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SpellStrike.Data;
using SpellStrike.Combat.Spells;

namespace SpellStrike.UI
{
    /// <summary>
    /// Giao diện chọn Phép mở đầu level.
    /// Kéo thả prefabs Panel này vào màn hình lúc đầu (hoặc do LevelManager bật lên).
    /// </summary>
    public class SpellSelectionUI : MonoBehaviour
    {
        [SerializeField] private GameObject m_SelectionPanel;
        [SerializeField] private Button m_Option1Btn;
        [SerializeField] private Button m_Option2Btn;
        [SerializeField] private Button m_Option3Btn;
        
        [SerializeField] private Player.PlayerSpellController m_PlayerController;

        private List<SpellDataSO> m_AvailableOptions;

        private void Start()
        {
            // Tạm thời tự mở khi start game.
            // Level Manager có thể điều khiển logic đóng/mở này tốt hơn.
            ShowOptions();
        }

        public void ShowOptions()
        {
            if (SpellPoolService.Instance == null) return;

            m_SelectionPanel.SetActive(true);
            Time.timeScale = 0f; // Pause game

            m_AvailableOptions = SpellPoolService.Instance.GetRandomStartingSpells(3);

            SetupButton(m_Option1Btn, 0);
            SetupButton(m_Option2Btn, 1);
            SetupButton(m_Option3Btn, 2);
        }

        private void SetupButton(Button _btn, int _index)
        {
            _btn.onClick.RemoveAllListeners();
            if (_index < m_AvailableOptions.Count)
            {
                _btn.gameObject.SetActive(true);
                var spell = m_AvailableOptions[_index];
                
                // TODO: Update Text/Icon UI con ở trong button
                // _btn.GetComponentInChildren<Text>().text = spell.SpellName;
                
                _btn.onClick.AddListener(() => OnSpellSelected(spell));
            }
            else
            {
                _btn.gameObject.SetActive(false);
            }
        }

        private void OnSpellSelected(SpellDataSO _spell)
        {
            if (m_PlayerController != null)
            {
                m_PlayerController.EquipSpell(_spell);
            }

            m_SelectionPanel.SetActive(false);
            Time.timeScale = 1f; // Resume
        }
    }
}
