using UnityEngine;

namespace SpellStrike.Data
{
    [CreateAssetMenu(menuName = "Data/LootBox Data")]
    public class LootBoxDataSO : ScriptableObject
    {
        #region Serialized Fields

        [SerializeField] private int m_HP = 30;
        [SerializeField] private DropTableSO m_DropTable;
        [SerializeField] private bool m_BlocksPathfinding;

        #endregion

        #region Public Properties

        public int HP => m_HP;
        public DropTableSO DropTable => m_DropTable;
        public bool BlocksPathfinding => m_BlocksPathfinding;

        #endregion
    }
}
