using UnityEngine;
using SpellStrike.Core;
using SpellStrike.Data;

namespace SpellStrike.Level
{
    /// <summary>
    /// Xử lý flow thắng/thua cho 1 màn chơi cụ thể.
    /// Theo dõi số lượng Spawner. Nếu tụi nó chết hết -> Kích hoạt Boss (nếu có) -> Thắng.
    /// Nếu Player chết -> Thua.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private LevelDataSO m_LevelData;
        [SerializeField] private Player.PlayerController m_Player;

        // Có thể reference UI win/lose tại đây hoặc dùng EventSystem
        [Header("UI References (Tạm thời)")]
        [SerializeField] private GameObject m_WinScreen;
        [SerializeField] private GameObject m_LoseScreen;

        // Điểm đánh dấu boss xuất hiện
        [SerializeField] private Transform m_BossSpawnPoint;

        private int m_SpawnersRemaining;
        private bool m_IsLevelEnded = false;

        private void Start()
        {
            if (m_WinScreen != null) m_WinScreen.SetActive(false);
            if (m_LoseScreen != null) m_LoseScreen.SetActive(false);

            // Tìm và đếm Spawners trong map
            var spawners = FindObjectsByType<Enemy.SpawnerController>(FindObjectsSortMode.None);
            m_SpawnersRemaining = spawners.Length;

            if (m_Player != null && m_Player.Health != null)
            {
                m_Player.Health.OnDeathLocal += HandlePlayerDeath;
            }
        }

        private void OnDestroy()
        {
            if (m_Player != null && m_Player.Health != null)
            {
                m_Player.Health.OnDeathLocal -= HandlePlayerDeath;
            }
        }

        // Spawner sẽ gọi hàm này khi nó chết (hoặc dùng EventChannelSO)
        public void OnSpawnerDestroyed()
        {
            if (m_IsLevelEnded) return;

            m_SpawnersRemaining--;
            if (m_SpawnersRemaining <= 0)
            {
                if (m_LevelData != null && m_LevelData.EliteBossData != null)
                {
                    SpawnEliteBoss();
                }
                else
                {
                    WinLevel();
                }
            }
        }

        // Elite Boss controller sẽ gọi hàm này khi nó chết
        public void OnBossDefeated()
        {
            WinLevel();
        }

        private void SpawnEliteBoss()
        {
            // Spawn boss
            if (m_LevelData.EliteBossData != null && m_BossSpawnPoint != null)
            {
                // Instantiate Elite Boss Object
                // Sau đó UI sẽ hook vào Boss HP
                Debug.Log("[LevelManager] Spawning Elite Boss: " + m_LevelData.EliteBossData.DisplayName);
            }
            else
            {
                WinLevel(); // Safety fallback
            }
        }

        private void WinLevel()
        {
            if (m_IsLevelEnded) return;
            m_IsLevelEnded = true;

            GameManager.Instance?.ChangeState(GameState.Win);
            
            if (m_WinScreen != null) m_WinScreen.SetActive(true);
            
            // Xử lý unlock spell, cộng điểm...
        }

        private void HandlePlayerDeath()
        {
            if (m_IsLevelEnded) return;
            m_IsLevelEnded = true;

            GameManager.Instance?.ChangeState(GameState.Lose);
            
            if (m_LoseScreen != null) m_LoseScreen.SetActive(true);
        }
    }
}
