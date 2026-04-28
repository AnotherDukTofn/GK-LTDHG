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

        private void Awake()
        {
            if (m_Health == null) m_Health = GetComponent<Combat.HealthComponent>() ?? GetComponentInChildren<Combat.HealthComponent>();
        }

        private void OnEnable()
        {
            if (m_Health != null)
                m_Health.OnDeathLocal += Die;
            
            StartSpawn();
        }

        private void OnDisable()
        {
            if (m_Health != null)
                m_Health.OnDeathLocal -= Die;
            
            StopSpawn();
        }

        private void Start()
        {
            if (m_Health != null && m_Data != null)
            {
                m_Health.Initialize(m_Data.HP);
            }

            // Thiết lập bỏ qua va chạm giữa Spawner và Enemy
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            int spawnerLayer = gameObject.layer;
            if (enemyLayer != -1 && spawnerLayer != -1)
            {
                Physics.IgnoreLayerCollision(spawnerLayer, enemyLayer, true);
            }
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
                // Mỗi lần chỉ spawn 1 enemy duy nhất
                if (m_Data.EnemyPool != null && m_Data.EnemyPool.Count > 0)
                {
                    SpawnEnemy(m_Data.EnemyPool[UnityEngine.Random.Range(0, m_Data.EnemyPool.Count)].Prefab);
                }

                // Đợi đến đợt spawn tiếp theo
                await UniTask.Delay(TimeSpan.FromSeconds(m_Data.SpawnInterval), cancellationToken: _token);
            }
        }

        private void SpawnEnemy(GameObject prefab)
        {
            // Spawn trực tiếp tại vị trí Spawner (không dùng radius offset)
            Vector3 spawnPos = transform.position;
            Instantiate(prefab, spawnPos, Quaternion.identity);
        }

        // Logic phá hủy Spawner
        public void Die()
        {
            Debug.Log($"[SpawnerController] Spawner {gameObject.name} destroyed!");
            StopSpawn();
            
            // Thả loot: Chắc chắn drop phép (Spell) hoặc vật phẩm theo data
            if (m_Data != null && m_Data.DropTable != null)
            {
                Combat.LootDropperService.Instance?.TryDropEnemyLoot(transform.position, m_Data.DropTable);
            }

            Destroy(gameObject);
        }
    }
}
