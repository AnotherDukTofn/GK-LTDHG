using UnityEngine;
using SpellStrike.Enemy;
using SpellStrike.Combat;

namespace SpellStrike.Utility
{
    public class EnemySystemTester : MonoBehaviour
    {
        [Header("References")]
        public SpawnerController spawner;
        public EnemyBase enemyPrefab;
        
        [ContextMenu("Test Spawner Death")]
        public void TestSpawnerDeath()
        {
            if (spawner != null)
            {
                Debug.Log("Testing Spawner Death - Should drop guaranteed Spell.");
                spawner.Die();
            }
        }

        [ContextMenu("Test Enemy Death")]
        public void TestEnemyDeath()
        {
            if (enemyPrefab != null)
            {
                Debug.Log("Testing Enemy Death - Rolling 100 times to check probabilities...");
                for (int i = 0; i < 100; i++)
                {
                    LootDropperService.Instance.TryDropEnemyLoot(transform.position + Random.insideUnitSphere * 2f, enemyPrefab.Data.DropTable);
                }
            }
        }
    }
}
