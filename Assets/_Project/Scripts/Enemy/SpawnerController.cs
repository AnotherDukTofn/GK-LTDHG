using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using UnityEngine;
using SpellStrike.Data;

namespace SpellStrike.Enemy
{
    /// <summary>
    /// Sinh quái liên tục theo cấu hình SpawnerDataSO.
    /// Có thể bị phá hủy nến gắn HealthComponent.
    /// </summary>
    public class SpawnerController : MonoBehaviour
    {
        [SerializeField] private SpawnerDataSO m_Data;
        [SerializeField] private Combat.HealthComponent m_Health;

        private bool m_IsActive = false;
        private CancellationTokenSource m_SpawnCts;

        private void OnEnable()
        {
            StartSpawn();
        }

        private void OnDisable()
        {
            StopSpawn();
        }

        public void StartSpawn()
        {
            if (m_IsActive) return;
            m_IsActive = true;
            
            if (m_SpawnCts != null)
            {
                m_SpawnCts.Cancel();
                m_SpawnCts.Dispose();
            }
            m_SpawnCts = new CancellationTokenSource();
            
            SpawnAsync(m_SpawnCts.Token).Forget();
        }

        public void StopSpawn()
        {
            m_IsActive = false;
            if (m_SpawnCts != null)
            {
                m_SpawnCts.Cancel();
                m_SpawnCts.Dispose();
                m_SpawnCts = null;
            }
        }

        private async UniTaskVoid SpawnAsync(CancellationToken _token)
        {
            // Initial delay
            await UniTask.Delay(TimeSpan.FromSeconds(2f), cancellationToken: _token);

            while (m_IsActive)
            {
                int currentSpawnCount = UnityEngine.Random.Range(m_Data.SpawnCountMin, m_Data.SpawnCountMax + 1);
                for (int i = 0; i < currentSpawnCount; i++)
                {
                    if (m_Data.EnemyPool != null && m_Data.EnemyPool.Count > 0)
                    {
                        SpawnEnemy(m_Data.EnemyPool[UnityEngine.Random.Range(0, m_Data.EnemyPool.Count)].Prefab);
                    }
                    await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: _token); // Giãn cách spawn giữa các con trong 1 wave
                }

                await UniTask.Delay(TimeSpan.FromSeconds(m_Data.SpawnInterval), cancellationToken: _token);
            }
        }

        private void SpawnEnemy(GameObject prefab)
        {
            // Tạm thời Instantiate. Cần Pool sau.
            // Lấy vị trí ngẫu nhiên xung quanh spawner
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * m_Data.SpawnRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

            Instantiate(prefab, spawnPos, Quaternion.identity);
        }

        // Logic phá hủy Spawner
        public void Die()
        {
            StopSpawn();
            // Báo cho LevelManager (Phase 5)
            // Thả loot: Chắc chắn drop phép (Spell) theo yêu cầu
            if (m_Data.DropTable != null)
            {
                // Tìm trong bảng drop món nào là Spell để drop chắc chắn
                var spellEntry = m_Data.DropTable.LootEntries.Find(e => e.ItemType == DropItemType.SpellRandom);
                if (spellEntry != null && spellEntry.Prefab != null)
                {
                    Combat.LootDropperService.Instance?.DropPrefab(transform.position, spellEntry.Prefab);
                }
                else
                {
                    // Nếu không tìm thấy Spell cụ thể trong bàn, dùng logic Roll bình thường (nhưng spawner nên có spell)
                    Combat.LootDropperService.Instance?.TryDropLoot(transform.position, m_Data.DropTable);
                }
            }

            Destroy(gameObject);
        }
    }
}
