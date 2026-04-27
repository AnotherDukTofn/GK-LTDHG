using System.Threading;
using UnityEngine;
using SpellStrike.Data;

namespace SpellStrike.Combat.Spells
{
    /// <summary>
    /// Lớp nền tảng cho mọi loại phép thuật.
    /// Không giới hạn bằng hình thức cụ thể, do các class phái sinh tự override logic Launch.
    /// Do phép có thời gian cooldown (Track bởi PlayerSpellController), SpellBase chỉ là cục GameObject được clone ra khi bắn.
    /// </summary>
    public abstract class SpellBase : MonoBehaviour
    {
        #region Protected Fields

        protected SpellDataSO m_Data;
        protected float m_DamageMult = 1f;
        // Đối tượng gọi phép (Player/Enemy)
        protected Transform m_Caster;
        
        // Token dùng để tắt các UniTask chạy ngầm khi Object bị quăng về Pool
        protected CancellationTokenSource m_SpellCts;

        #endregion

        #region Unity Lifecycle

        protected virtual void OnEnable()
        {
            if (m_SpellCts != null)
            {
                m_SpellCts.Cancel();
                m_SpellCts.Dispose();
            }
            m_SpellCts = new CancellationTokenSource();
        }

        protected virtual void OnDisable()
        {
            if (m_SpellCts != null)
            {
                m_SpellCts.Cancel();
                m_SpellCts.Dispose();
                m_SpellCts = null;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Khởi tạo data ban đầu.
        /// </summary>
        public virtual void Setup(SpellDataSO _data, Transform _caster, float _damageMult = 1f)
        {
            m_Data = _data;
            m_Caster = _caster;
            m_DamageMult = _damageMult;
        }

        /// <summary>
        /// Logic thi triển chính (bắn ra, đánh AOE, v.v.)
        /// </summary>
        public abstract void Launch();

        /// <summary>
        /// Dành cho loại phép Hold liên tục. (VD: Inferno Breathe)
        /// Các class không dùng thì override rỗng.
        /// </summary>
        public virtual void HoldTick() { }

        /// <summary>
        /// Hủy phép Hold
        /// </summary>
        public virtual void EndHold() { }

        #endregion
    }
}
