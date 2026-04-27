using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using SpellStrike.Data;

namespace SpellStrike.Combat.Spells
{
    /// <summary>
    /// Phép bay thẳng xuyên nhiều mục tiêu. Gây sát thương mỗi khi xuyên qua một mục tiêu.
    /// Tự hủy sau khi bay hết tầm.
    /// Kèm DamageDealer luôn bật Trigger.
    /// </summary>
    [RequireComponent(typeof(DamageDealer), typeof(Rigidbody))]
    public class SplashSpell : SpellBase
    {
        [Header("References")]
        [SerializeField] private DamageDealer m_DamageDealer;

        public override void Setup(SpellDataSO _data, Transform _caster, float _damageMult = 1f)
        {
            base.Setup(_data, _caster, _damageMult);
            if (m_DamageDealer == null) m_DamageDealer = GetComponent<DamageDealer>();
            
            // Đảm bảo Rigidbody không bị vật lý tác động làm lệch quỹ đạo
            var rb = GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
        }

        public override void Launch()
        {
            m_DamageDealer.SetDamage((int)Mathf.Round(m_Data.Damage), m_DamageMult);
            m_DamageDealer.GetComponent<Collider>().enabled = true;

            FlyAsync(m_SpellCts.Token).Forget();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (m_DamageDealer != null) 
            {
                var col = m_DamageDealer.GetComponent<Collider>();
                if (col != null) col.enabled = false;
            }
        }

        private async UniTaskVoid FlyAsync(CancellationToken _token)
        {
            float traveledDistance = 0f;
            float range = m_Data.Range;
            float speed = m_Data.BehaviorParams.ProjectileSpeed;
            if (speed <= 0) speed = 20f;

            Vector3 direction = transform.forward;
            
            // Debug log để tra lỗi pool
            // Debug.Log($"[SplashSpell] Launching. Range: {range}, Speed: {speed}, ID: {m_Data.Id}");

            try 
            {
                while (traveledDistance < range)
                {
                    float step = speed * Time.deltaTime;
                    
                    // Di chuyển tịnh tiến
                    transform.position += direction * step;
                    traveledDistance += step;

                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: _token);
                }
            }
            catch (System.OperationCanceledException)
            {
                // Task bị hủy do Object quay về pool/bị disable -> thoát im lặng
                return;
            }

            // Hết tầm -> Return Pool
            if (gameObject.activeInHierarchy)
            {
                SpellPoolService.Instance.ReturnSpell(m_Data.Id, this);
            }
        }
    }
}
