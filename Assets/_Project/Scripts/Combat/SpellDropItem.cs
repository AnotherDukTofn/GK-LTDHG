using UnityEngine;
using SpellStrike.Data;

namespace SpellStrike.Combat
{
    public class SpellDropItem : DropItem
    {
        [SerializeField] private SpellDataSO m_SpellReward;

        protected override void OnPickup(Player.PlayerController _player)
        {
            if (m_SpellReward != null)
            {
                // Gọi Event trang bị luôn lúc lụm 
                // (Chờ Unlock logic phase sau nếu muốn chọn lọc)
                _player.GetComponent<Player.PlayerSpellController>()?.EquipSpell(m_SpellReward);
            }
        }
    }
}
