using System.Collections.Generic;
using UnityEngine;

namespace SpellStrike.Utility
{
    /// <summary>
    /// Weighted random selection — dùng cho DropTable, spell pool, v.v.
    /// </summary>
    public static class WeightedRandom
    {
        /// <summary>
        /// Chọn 1 phần tử ngẫu nhiên từ danh sách có trọng số.
        /// </summary>
        /// <param name="_entries">Danh sách (item, weight).</param>
        /// <returns>Item được chọn, hoặc default nếu danh sách rỗng.</returns>
        public static T Select<T>(IList<(T item, int weight)> _entries)
        {
            if (_entries == null || _entries.Count == 0)
                return default;

            int totalWeight = 0;
            for (int i = 0; i < _entries.Count; i++)
            {
                totalWeight += _entries[i].weight;
            }

            if (totalWeight <= 0)
                return _entries[0].item;

            int roll = Random.Range(0, totalWeight);
            int cumulative = 0;

            for (int i = 0; i < _entries.Count; i++)
            {
                cumulative += _entries[i].weight;
                if (roll < cumulative)
                {
                    return _entries[i].item;
                }
            }

            return _entries[_entries.Count - 1].item;
        }
    }
}
