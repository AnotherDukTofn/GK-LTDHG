using UnityEngine;

namespace SpellStrike.Combat
{
    /// <summary>
    /// Gắn lên bất cứ trigger hitbox nào gây sát thương (Enemy Attack, Spell Projectile).
    /// </summary>
    public class DamageDealer : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private int m_BaseDamage;
        [Tooltip("Mask nhận damage. Spell: Enemy layer. Enemy: Player layer.")]
        [SerializeField] private LayerMask m_TargetLayers;

        #endregion

        #region Private Fields

        private float m_ScalingMult = 1f;

        #endregion

        #region Public Methods

        public void SetDamage(int _base, float _scaling = 1f)
        {
            m_BaseDamage = _base;
            m_ScalingMult = _scaling;
        }

        #endregion

        #region Unity Lifecycle

        private void OnTriggerEnter(Collider _other)
        {
            // Kiểm tra layer match
            if (((1 << _other.gameObject.layer) & m_TargetLayers) != 0)
            {
                // Tìm HealthComponent ở object đó hoặc cha/con của nó để tránh sót
                var health = _other.GetComponentInParent<HealthComponent>() ?? _other.GetComponentInChildren<HealthComponent>();
                if (health != null)
                {
                    int finalDamage = Mathf.RoundToInt(m_BaseDamage * m_ScalingMult);
                    Debug.Log($"[DamageDealer] {gameObject.name} hit {_other.name}, dealing {finalDamage} damage.");
                    health.TakeDamage(finalDamage);
                }
                else
                {
                    Debug.LogWarning($"[DamageDealer] {gameObject.name} hit {_other.name} but no HealthComponent found.");
                }
            }
        }

        #endregion
    }
}
