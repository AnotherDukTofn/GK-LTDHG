using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;
using SpellStrike.Data;

namespace SpellStrike.Combat.Spells
{
    /// <summary>
    /// Bắn ra một quả cầu lửa theo quỹ đạo Parabol (đường vòng cung).
    /// Target = vị trí chuột trên mặt đất (truyền qua SetTargetPoint).
    /// Travel time KHÔNG ĐỔI bất kể khoảng cách (constant lob time).
    /// Gây sát thương AoE khi chạm đất. Trả về Pool sau khi nổ.
    /// </summary>
    [RequireComponent(typeof(DamageDealer))]
    public class FireballSpell : SpellBase
    {
        [Header("References")]
        [SerializeField] private DamageDealer m_DamageDealer;
        [SerializeField] private TrailRenderer m_TrailRenderer;

        #region Private Fields

        private Vector3 m_TargetPoint;
        private bool m_HasTarget;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gọi trước Launch() để chỉ định điểm rơi (vị trí chuột trên mặt đất).
        /// </summary>
        public void SetTargetPoint(Vector3 _worldPoint)
        {
            m_TargetPoint = _worldPoint;
            m_HasTarget = true;
        }

        #endregion

        public override void Setup(SpellDataSO _data, Transform _caster, float _damageMult = 1f)
        {
            base.Setup(_data, _caster, _damageMult);
            if (m_DamageDealer == null) m_DamageDealer = GetComponent<DamageDealer>();
            m_HasTarget = false;
        }

        public override void Launch()
        {
            // Reset Trail để tránh hiện tượng "kéo dây" từ vị trí cũ khi rút ra khỏi Pool
            if (m_TrailRenderer != null)
            {
                m_TrailRenderer.Clear();
            }

            // Thiết lập damage từ data
            m_DamageDealer.SetDamage((int)Mathf.Round(m_Data.Damage), m_DamageMult);
            // Disable collider lúc đang bay, chỉ bật khi nổ
            m_DamageDealer.GetComponent<Collider>().enabled = false;

            // Tính điểm rơi
            Vector3 startPos = transform.position;
            Vector3 targetPos;

            if (m_HasTarget)
            {
                targetPos = m_TargetPoint;
            }
            else
            {
                // Fallback: bắn về phía trước theo Range (từ data)
                targetPos = startPos + transform.forward * m_Data.Range;
            }

            LobAsync(startPos, targetPos, m_SpellCts.Token).Forget();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (m_DamageDealer != null) m_DamageDealer.GetComponent<Collider>().enabled = false;
            // Trails are kept a bit buggy if disabled midway, but clear is a good start
            if (m_TrailRenderer != null) m_TrailRenderer.Clear();
        }

        private async UniTaskVoid LobAsync(Vector3 _start, Vector3 _target, CancellationToken _token)
        {
            float duration = m_Data.BehaviorParams.LobTravelTime > 0
                ? m_Data.BehaviorParams.LobTravelTime
                : (m_Data.BehaviorParams.Duration > 0 ? m_Data.BehaviorParams.Duration : 1f);

            float height = m_Data.BehaviorParams.AoeRadius; // Chiều cao vòng cung từ data

            // DOTween DOJump tự động xử lý nội suy Parabola
            bool isCanceled = await transform.DOJump(_target, height, 1, duration)
                                             .SetEase(Ease.Linear)
                                             .ToUniTask(cancellationToken: _token)
                                             .SuppressCancellationThrow();

            if (isCanceled) return;

            ExplodeAsync(_token).Forget();
        }

        private async UniTaskVoid ExplodeAsync(CancellationToken _token)
        {
            // Bật hiệu ứng nổ từ SpellDataSO
            if (m_Data.VfxPrefab != null)
            {
                var vfx = Instantiate(m_Data.VfxPrefab, transform.position, Quaternion.identity);
                Destroy(vfx.gameObject, 2f);
            }

            // Bật collider DamageDealer, lấy bán kính Aoe từ data
            Collider col = m_DamageDealer.GetComponent<Collider>();
            if (col is SphereCollider sphere)
            {
                sphere.radius = m_Data.BehaviorParams.AoeRadius;
            }
            col.enabled = true;
            Debug.Log($"[FireballSpell] Explosion activated at {transform.position}");

            // Đợi một khoảng ngắn (0.1s) thay vì chỉ 1 frame vật lý để đảm bảo engine kịp scan trúng quái
            bool isCanceled = await UniTask.Delay(System.TimeSpan.FromSeconds(0.1f), cancellationToken: _token).SuppressCancellationThrow();
            
            if (isCanceled) return;

            SpellPoolService.Instance.ReturnSpell(m_Data.Id, this);
        }
    }
}
