using System.Collections.Generic;
using UnityEngine;

namespace SpellStrike.Data
{
    [CreateAssetMenu(menuName = "Data/Spawner Data")]
    public class SpawnerDataSO : ScriptableObject
    {
        #region Serialized Fields

        [SerializeField] private string m_Id;
        [SerializeField] private int m_HP;
        [SerializeField] private float m_SpawnInterval;
        [SerializeField] private int m_SpawnCountMin;
        [SerializeField] private int m_SpawnCountMax;
        [SerializeField] private List<EnemyDataSO> m_EnemyPool;
        [SerializeField] private DropTableSO m_DropTable;

        #endregion

        #region Public Properties

        public string Id => m_Id;
        public int HP => m_HP;
        public float SpawnInterval => m_SpawnInterval;
        public int SpawnCountMin => m_SpawnCountMin;
        public int SpawnCountMax => m_SpawnCountMax;
        public List<EnemyDataSO> EnemyPool => m_EnemyPool;
        public DropTableSO DropTable => m_DropTable;

        #endregion
    }
}
