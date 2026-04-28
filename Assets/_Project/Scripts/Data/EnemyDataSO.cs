using UnityEngine;

namespace SpellStrike.Data
{
    public enum EnemyType { Melee, Ranged }

    [CreateAssetMenu(menuName = "Data/Enemy Data")]
    public class EnemyDataSO : ScriptableObject
    {
        #region Serialized Fields

        [Header("Basic Info")]
        [SerializeField] private string m_Id;
        [SerializeField] private string m_DisplayName;
        [SerializeField] private EnemyType m_Type;

        [Header("Stats")]
        [SerializeField] private int m_HP;
        [SerializeField] private float m_Speed;
        [SerializeField] private float m_Damage;
        [SerializeField] private float m_AttackRange = 1.5f;
        [SerializeField] private float m_AttackRate;
        [SerializeField] private float m_DeathDestroyDelay = 1.5f;

        [Header("Ranged Only")]
        [SerializeField] private float m_PreferredDistanceMin;
        [SerializeField] private float m_PreferredDistanceMax;


        [Header("Loot")]
        [SerializeField] private DropTableSO m_DropTable;

        [Header("Prefab")]
        [SerializeField] private GameObject m_Prefab;

        #endregion

        #region Public Properties

        public string Id => m_Id;
        public string DisplayName => m_DisplayName;
        public EnemyType Type => m_Type;
        public int HP => m_HP;
        public float Speed => m_Speed;
        public float Damage => m_Damage;
        public float AttackRange => m_AttackRange;
        public float AttackRate => m_AttackRate;
        public float DeathDestroyDelay => m_DeathDestroyDelay;
        public float PreferredDistanceMin => m_PreferredDistanceMin;
        public float PreferredDistanceMax => m_PreferredDistanceMax;

        public DropTableSO DropTable => m_DropTable;
        public GameObject Prefab => m_Prefab;

        #endregion
    }
}
