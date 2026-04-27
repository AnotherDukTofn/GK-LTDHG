using UnityEngine;
using SpellStrike.Combat;

namespace SpellStrike.Enemy
{
    [RequireComponent(typeof(DamageDealer), typeof(Rigidbody))]
    public class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private DamageDealer m_DamageDealer;
        [SerializeField] private float m_Speed = 10f;
        
        private float m_RangeLeft;
        private Vector3 m_Direction;

        private void Awake()
        {
            if (m_DamageDealer == null) m_DamageDealer = GetComponent<DamageDealer>();
        }

        public void Fire(Vector3 direction, int damage, float range)
        {
            m_Direction = direction.normalized;
            m_DamageDealer.SetDamage(damage);
            m_RangeLeft = range;
            
            // DamageDealer Collider sẽ tự xử lý OnCollisionEnter nhờ logic DamageDealer
            // Dọn Object khi đạn đập trúng qua một Event nhỏ, hoặc đạn biến mất qua Coroutine
        }

        private void Update()
        {
            if (m_RangeLeft <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            float step = m_Speed * Time.deltaTime;
            transform.position += m_Direction * step;
            m_RangeLeft -= step;
        }

        // Nếu bắn trúng, DamageDealer sẽ lo liệu phần xử lý sát thương
        // Nhưng cần tự hủy đạn ở lớp này
        private void OnTriggerEnter(Collider other)
        {
            // Bỏ qua nếu là enemy layer
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy")) return;

            Destroy(gameObject); // Thay bằng Pool
        }
    }
}
