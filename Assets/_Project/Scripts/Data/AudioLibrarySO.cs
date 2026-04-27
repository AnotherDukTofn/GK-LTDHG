using System.Collections.Generic;
using UnityEngine;

namespace SpellStrike.Data
{
    /// <summary>
    /// Thư viện audio — map string ID → AudioClip.
    /// Unity không serialize Dictionary, dùng List + runtime lookup.
    /// </summary>
    [CreateAssetMenu(menuName = "Data/Audio Library")]
    public class AudioLibrarySO : ScriptableObject
    {
        #region Serialized Fields

        [SerializeField] private List<AudioEntry> m_Entries;

        #endregion

        #region Private Fields

        private Dictionary<string, AudioClip> m_Lookup;

        #endregion

        #region Public Methods

        /// <summary>
        /// Lấy AudioClip theo ID. Trả về null nếu không tìm thấy.
        /// </summary>
        public AudioClip GetClip(string _id)
        {
            if (m_Lookup == null) BuildLookup();
            return m_Lookup.TryGetValue(_id, out var clip) ? clip : null;
        }

        #endregion

        #region Private Methods

        private void BuildLookup()
        {
            m_Lookup = new Dictionary<string, AudioClip>();
            if (m_Entries == null) return;

            foreach (var entry in m_Entries)
            {
                if (!string.IsNullOrEmpty(entry.Id) && entry.Clip != null)
                {
                    m_Lookup[entry.Id] = entry.Clip;
                }
            }
        }

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            m_Lookup = null; // Force rebuild on next access
        }

        #endregion
    }

    [System.Serializable]
    public struct AudioEntry
    {
        [Tooltip("ID dùng để gọi clip, ví dụ: sfx_dash, bgm_elite")]
        public string Id;
        public AudioClip Clip;
    }
}
