using UnityEngine;

namespace SpellStrike.Core.EventChannels
{
    /// <summary>
    /// SO Event Channel không tham số — dùng cho sự kiện đơn giản.
    /// Ví dụ: OnSpawnerDestroyed, OnPlayerDeath, OnGameWin, OnGameLose.
    /// </summary>
    [CreateAssetMenu(menuName = "Events/Void Event Channel")]
    public class VoidEventChannelSO : ScriptableObject
    {
        #region Private Fields

        private System.Action m_OnEventRaised;

        #endregion

        #region Public Methods

        public void RaiseEvent() => m_OnEventRaised?.Invoke();
        public void Subscribe(System.Action _listener) => m_OnEventRaised += _listener;
        public void Unsubscribe(System.Action _listener) => m_OnEventRaised -= _listener;

        #endregion
    }
}
