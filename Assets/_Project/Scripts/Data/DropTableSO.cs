using System.Collections.Generic;
using UnityEngine;

namespace SpellStrike.Data
{
    public enum DropItemType { Nothing, PowerUpRandom, SpellRandom, Heal, Boost, Shield, Invincible }

    [CreateAssetMenu(menuName = "Data/Drop Table")]
    public class DropTableSO : ScriptableObject
    {
        #region Serialized Fields

        [SerializeField] [Range(0f, 1f)] private float m_DropChance = 1f;
        [SerializeField] private List<WeightedDropEntry> m_Entries;

        #endregion

        #region Public Methods

        /// <summary>
        /// Tỷ lệ rơi đồ tổng thể.
        /// </summary>
        public float DropChance => m_DropChance;
        
        /// <summary>
        /// Danh sách vật phẩm có thể rơi
        /// </summary>
        public List<WeightedDropEntry> LootEntries => m_Entries;

        /// <summary>
        /// Roll ngẫu nhiên theo trọng số — trả về loại item drop.
        /// </summary>
        public DropItemType Roll()
        {
            if (m_Entries == null || m_Entries.Count == 0)
                return DropItemType.Nothing;

            int totalWeight = 0;
            for (int i = 0; i < m_Entries.Count; i++)
                totalWeight += m_Entries[i].Weight;

            if (totalWeight <= 0)
                return DropItemType.Nothing;

            int roll = Random.Range(0, totalWeight);
            int cumulative = 0;

            for (int i = 0; i < m_Entries.Count; i++)
            {
                cumulative += m_Entries[i].Weight;
                if (roll < cumulative)
                    return m_Entries[i].ItemType;
            }

            return m_Entries[m_Entries.Count - 1].ItemType;
        }

        #endregion
    }

    [System.Serializable]
    public class WeightedDropEntry
    {
        public DropItemType ItemType;
        public int Weight;
        public GameObject Prefab;
    }
}
