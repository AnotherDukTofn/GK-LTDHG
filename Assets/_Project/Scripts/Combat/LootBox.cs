using UnityEngine;
using SpellStrike.Data;

namespace SpellStrike.Combat
{
    /// <summary>
    /// Rương đồ có thể phá vỡ bằng đòn đánh (sát thương/phép).
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public class LootBox : MonoBehaviour
    {
        [SerializeField] private LootBoxDataSO m_Data;
        private HealthComponent m_Health;

        private void Awake()
        {
            m_Health = GetComponent<HealthComponent>();
            if (m_Data != null)
            {
                m_Health.Initialize(m_Data.HP);
            }
        }

        public void Die()
        {
            if (m_Data != null && m_Data.DropTable != null)
            {
                LootDropperService.Instance?.TryDropLootBoxLoot(transform.position, m_Data.DropTable);
            }

            // Gọi anim Break (nếu có)
            Destroy(gameObject);
        }
    }
}
