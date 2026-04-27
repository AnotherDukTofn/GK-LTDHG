using UnityEngine;

namespace SpellStrike.Core.EventChannels
{
    [CreateAssetMenu(menuName = "Events/String Event Channel")]
    public class StringEventChannelSO : ScriptableObject
    {
        #region Private Fields

        private System.Action<string> m_OnEventRaised;

        #endregion

        #region Public Methods

        public void RaiseEvent(string _value) => m_OnEventRaised?.Invoke(_value);
        public void Subscribe(System.Action<string> _listener) => m_OnEventRaised += _listener;
        public void Unsubscribe(System.Action<string> _listener) => m_OnEventRaised -= _listener;

        #endregion
    }
}
