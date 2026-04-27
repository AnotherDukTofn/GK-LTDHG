using UnityEngine;
using SpellStrike.Data;

namespace SpellStrike.Combat
{
    /// <summary>
    /// Service toàn cục chuyên quản lý thả đồ.
    /// Dựa vào bảng DropTableSO, tính tỷ lệ và instantiate vật phẩm.
    /// </summary>
    public class LootDropperService : MonoBehaviour
    {
        public static LootDropperService Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) Destroy(gameObject);
            else Instance = this;
        }

        public void TryDropLoot(Vector3 _position, DropTableSO _table)
        {
            if (_table == null) return;

            // Xác định xem có drop đồ không
            if (Random.value > _table.DropChance) return;

            // Roll món đồ dựa vào tỷ lệ Weight
            int totalWeight = 0;
            foreach (var entry in _table.LootEntries)
            {
                totalWeight += entry.Weight;
            }

            if (totalWeight <= 0) return;

            int randomWeight = Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (var entry in _table.LootEntries)
            {
                currentWeight += entry.Weight;
                if (randomWeight < currentWeight)
                {
                    if (entry.Prefab != null)
                    {
                        DropPrefab(_position, entry.Prefab);
                    }
                    break;
                }
            }
        }

        public void DropPrefab(Vector3 _position, GameObject _prefab)
        {
            if (_prefab == null) return;
            // Thả món đồ này ra
            // Làm hiệu ứng nảy nhẹ ra xung quanh
            Vector2 offset = Random.insideUnitCircle * 1.5f;
            Vector3 dropPos = _position + new Vector3(offset.x, 0.5f, offset.y);
            Instantiate(_prefab, dropPos, Quaternion.identity);
        }

        /// <summary>
        /// Logic riêng cho Enemy: 60% Nothing, 10% Spell, 7.5% mỗi loại Powerup khác.
        /// </summary>
        public void TryDropEnemyLoot(Vector3 _position, DropTableSO _table)
        {
            if (_table == null) return;

            float roll = Random.Range(0f, 100f);

            // 60% tỉ lệ không rơi gì
            if (roll < 60f) return;

            DropItemType targetType = DropItemType.Nothing;

            // 10% rơi phép (60 -> 70)
            if (roll < 70f) targetType = DropItemType.SpellRandom;
            // 7.5% rơi Heal (70 -> 77.5)
            else if (roll < 77.5f) targetType = DropItemType.Heal;
            // 7.5% rơi Boost (77.5 -> 85)
            else if (roll < 85f) targetType = DropItemType.Boost;
            // 7.5% rơi Shield (85 -> 92.5)
            else if (roll < 92.5f) targetType = DropItemType.Shield;
            // 7.5% rơi Invincible (92.5 -> 100)
            else targetType = DropItemType.Invincible;

            if (targetType != DropItemType.Nothing)
            {
                var entry = _table.LootEntries.Find(e => e.ItemType == targetType);
                if (entry != null && entry.Prefab != null)
                {
                    DropPrefab(_position, entry.Prefab);
                }
            }
        }

        /// <summary>
        /// Logic riêng cho Lootbox: 50% Phép, 50% Powerup. Trong mỗi loại chia đều.
        /// </summary>
        public void TryDropLootBoxLoot(Vector3 _position, DropTableSO _table)
        {
            if (_table == null || _table.LootEntries == null || _table.LootEntries.Count == 0) return;

            bool dropSpell = Random.value < 0.5f;

            if (dropSpell)
            {
                // Lấy tất cả Spell trong bảng
                var spellEntries = _table.LootEntries.FindAll(e => e.ItemType == DropItemType.SpellRandom);
                if (spellEntries.Count > 0)
                {
                    var entry = spellEntries[Random.Range(0, spellEntries.Count)];
                    DropPrefab(_position, entry.Prefab);
                    return;
                }
            }

            // Nếu không drop spell hoặc không có spell nào, drop Powerup
            var powerupEntries = _table.LootEntries.FindAll(e => e.ItemType != DropItemType.SpellRandom && e.ItemType != DropItemType.Nothing);
            if (powerupEntries.Count > 0)
            {
                var entry = powerupEntries[Random.Range(0, powerupEntries.Count)];
                DropPrefab(_position, entry.Prefab);
            }
            else if (!dropSpell) // Nếu muốn drop powerup mà ko có, thử lại drop spell nếu có
            {
                var spellEntries = _table.LootEntries.FindAll(e => e.ItemType == DropItemType.SpellRandom);
                if (spellEntries.Count > 0)
                {
                    var entry = spellEntries[Random.Range(0, spellEntries.Count)];
                    DropPrefab(_position, entry.Prefab);
                }
            }
        }
    }
}
