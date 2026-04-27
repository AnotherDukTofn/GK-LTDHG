using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using UnityEngine;

namespace SpellStrike.Combat
{
    [RequireComponent(typeof(Collider))]
    public abstract class DropItem : MonoBehaviour
    {
        [Header("Drop Settings")]
        [SerializeField] protected float m_Lifetime = 15f;
        [SerializeField] protected float m_BlinkWarningTime = 3f;

        private CancellationTokenSource m_Cts;

        protected virtual void OnEnable()
        {
            GetComponent<Collider>().isTrigger = true;

            if (m_Cts != null)
            {
                m_Cts.Cancel();
                m_Cts.Dispose();
            }
            m_Cts = new CancellationTokenSource();
            
            LifetimeAsync(m_Cts.Token).Forget();
        }

        protected virtual void OnDisable()
        {
            if (m_Cts != null)
            {
                m_Cts.Cancel();
                m_Cts.Dispose();
                m_Cts = null;
            }
        }

        private async UniTaskVoid LifetimeAsync(CancellationToken _token)
        {
            bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(m_Lifetime - m_BlinkWarningTime), cancellationToken: _token).SuppressCancellationThrow();
            if (canceled) return;

            // TODO: Nhấp nháy cảnh báo sắp mất
            
            canceled = await UniTask.Delay(TimeSpan.FromSeconds(m_BlinkWarningTime), cancellationToken: _token).SuppressCancellationThrow();
            if (canceled) return;
            
            Destroy(gameObject); // Return to pool instead if applying ObjectPool for DropItem
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && other.TryGetComponent<Player.PlayerController>(out var player))
            {
                OnPickup(player);
                Destroy(gameObject);
            }
        }

        protected abstract void OnPickup(Player.PlayerController _player);
    }
}
