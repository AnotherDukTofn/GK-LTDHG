using System.Collections.Generic;
using UnityEngine;
using SpellStrike.Data;
using SpellStrike.Utility;

namespace SpellStrike.Combat.Spells
{
    /// <summary>
    /// Service quản lý ObjectPool cho từng loại Spell.
    /// Chuyển thành MonoBehaviour singleton để chạy Coroutine và Inject.
    /// </summary>
    public class SpellPoolService : MonoBehaviour
    {
        #region Singleton

        public static SpellPoolService Instance { get; private set; }

        #endregion

        #region Private Fields

        // Map ID của SpellData -> Pool tương ứng
        private Dictionary<string, Core.ObjectPool<SpellBase>> m_SpellPools = new Dictionary<string, Core.ObjectPool<SpellBase>>();

        // Reference tới LevelData để biết danh sách phép cơ bản sẽ random lúc đầu
        [SerializeField] private LevelDataSO m_CurrentLevelData;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Lấy một instance của Spell từ pool dựa trên Data.
        /// </summary>
        public SpellBase GetSpell(SpellDataSO _data)
        {
            if (_data == null || _data.SpellPrefab == null) return null;

            if (!m_SpellPools.ContainsKey(_data.Id))
            {
                // Instantiate a new pool if it doesn't exist
                SpellBase prefab = _data.SpellPrefab.GetComponent<SpellBase>();
                if (prefab == null)
                {
                    Debug.LogError($"[SpellPoolService] SpellPrefab của {_data.Id} không có component kế thừa từ SpellBase!");
                    return null;
                }

                GameObject poolContainer = new GameObject($"Pool_{_data.Id}");
                poolContainer.transform.SetParent(this.transform);

                var newPool = new Core.ObjectPool<SpellBase>(prefab, 10, poolContainer.transform);
                m_SpellPools.Add(_data.Id, newPool);
            }

            return m_SpellPools[_data.Id].Get();
        }

        /// <summary>
        /// Trả Spell về pool dựa trên thư viện ID (Cần truyền ID vào). 
        /// Thường thì SpellBase lúc bị Destroy/Despawn sẽ tự gọi hàm này.
        /// </summary>
        public void ReturnSpell(string _spellId, SpellBase _spellInstance)
        {
            if (m_SpellPools.TryGetValue(_spellId, out var pool))
            {
                pool.Return(_spellInstance);
            }
            else
            {
                // Fallback: nếu không map được pool, hủy bỏ luôn
                Destroy(_spellInstance.gameObject);
            }
        }

        /// <summary>
        /// Lấy random N spells mở khóa ở đầu level (bước 5.2)
        /// </summary>
        public List<SpellDataSO> GetRandomStartingSpells(int count)
        {
            List<SpellDataSO> result = new List<SpellDataSO>();
            if (m_CurrentLevelData == null || m_CurrentLevelData.SpellDropPool == null) return result;

            List<SpellDataSO> tempPool = new List<SpellDataSO>(m_CurrentLevelData.SpellDropPool);
            
            for (int i = 0; i < count && tempPool.Count > 0; i++)
            {
                int index = Random.Range(0, tempPool.Count);
                result.Add(tempPool[index]);
                tempPool.RemoveAt(index);
            }

            return result;
        }

        #endregion
    }
}
