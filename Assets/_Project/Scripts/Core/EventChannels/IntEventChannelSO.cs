using UnityEngine;

namespace SpellStrike.Core.EventChannels
{
    [CreateAssetMenu(menuName = "Events/Int Event Channel")]
    public class IntEventChannelSO : ScriptableObject
    {
        #region Private Fields

        private System.Action<int> m_OnEventRaised;

        #endregion

        #region Public Methods

        public void RaiseEvent(int _value) => m_OnEventRaised?.Invoke(_value);
        public void Subscribe(System.Action<int> _listener) => m_OnEventRaised += _listener;
        public void Unsubscribe(System.Action<int> _listener) => m_OnEventRaised -= _listener;

        #endregion
    }
}
