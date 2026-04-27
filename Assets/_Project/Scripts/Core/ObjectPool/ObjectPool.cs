using System.Collections.Generic;
using UnityEngine;

namespace SpellStrike.Core
{
    /// <summary>
    /// Generic Object Pool — tái sử dụng GameObject thay vì Instantiate/Destroy liên tục.
    /// Bắt buộc dùng cho: Enemy, Projectile, VFX, Drop Item.
    /// </summary>
    public class ObjectPool<T> where T : MonoBehaviour
    {
        #region Private Fields

        private readonly Queue<T> m_Pool = new Queue<T>();
        private readonly T m_Prefab;
        private readonly Transform m_Parent;

        #endregion

        #region Constructor

        public ObjectPool(T _prefab, int _initialSize, Transform _parent = null)
        {
            m_Prefab = _prefab;
            m_Parent = _parent;

            for (int i = 0; i < _initialSize; i++)
            {
                T obj = CreateNewInstance();
                obj.gameObject.SetActive(false);
                m_Pool.Enqueue(obj);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Lấy một object từ pool. Tạo mới nếu pool rỗng.
        /// </summary>
        public T Get()
        {
            T obj;

            if (m_Pool.Count > 0)
            {
                obj = m_Pool.Dequeue();
            }
            else
            {
                obj = CreateNewInstance();
            }

            obj.gameObject.SetActive(true);
            return obj;
        }

        /// <summary>
        /// Trả object về pool. Tự động deactivate.
        /// </summary>
        public void Return(T _obj)
        {
            _obj.gameObject.SetActive(false);
            m_Pool.Enqueue(_obj);
        }

        /// <summary>
        /// Số lượng object đang inactive trong pool.
        /// </summary>
        public int CountInactive => m_Pool.Count;

        #endregion

        #region Private Methods

        private T CreateNewInstance()
        {
            T obj = Object.Instantiate(m_Prefab, m_Parent);
            return obj;
        }

        #endregion
    }
}
