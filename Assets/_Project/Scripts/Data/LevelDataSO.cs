using System.Collections.Generic;
using UnityEngine;

namespace SpellStrike.Data
{
    [CreateAssetMenu(menuName = "Data/Level Data")]
    public class LevelDataSO : ScriptableObject
    {
        #region Serialized Fields

        [SerializeField] private string m_LevelName;
        [SerializeField] private int m_StartingSpellPoolSize;
        [SerializeField] private List<SpellDataSO> m_SpellDropPool;
        [SerializeField] private LevelScalingConfig m_ScalingConfig;
        [SerializeField] private EliteBossDataSO m_EliteBossData;
        [SerializeField] private string m_SceneName;

        #endregion

        #region Public Properties

        public string LevelName => m_LevelName;
        public int StartingSpellPoolSize => m_StartingSpellPoolSize;
        public List<SpellDataSO> SpellDropPool => m_SpellDropPool;
        public LevelScalingConfig ScalingConfig => m_ScalingConfig;
        public EliteBossDataSO EliteBossData => m_EliteBossData;
        public string SceneName => m_SceneName;

        #endregion
    }

    /// <summary>
    /// Hệ số scaling cho level — nhân với chỉ số base từ EnemyDataSO/EliteBossDataSO.
    /// </summary>
    [System.Serializable]
    public class LevelScalingConfig
    {
        [Range(0.5f, 3f)] public float EnemyHPMult = 1f;
        [Range(0.5f, 3f)] public float EnemyDmgMult = 1f;
        [Range(0.5f, 3f)] public float EnemySpeedMult = 1f;
        [Range(0.5f, 3f)] public float SpawnerHPMult = 1f;
        [Range(0.1f, 2f)] public float SpawnRateMult = 1f;
        [Range(0.5f, 5f)] public float EliteHPMult = 1f;
        [Range(0.5f, 3f)] public float EliteDmgMult = 1f;
    }
}
