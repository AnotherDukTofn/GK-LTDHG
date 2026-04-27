using UnityEngine;

namespace SpellStrike.Core.EventChannels
{
    [CreateAssetMenu(menuName = "Events/Float Event Channel")]
    public class FloatEventChannelSO : ScriptableObject
    {
        #region Private Fields

        private System.Action<float> m_OnEventRaised;

        #endregion

        #region Public Methods

        public void RaiseEvent(float _value) => m_OnEventRaised?.Invoke(_value);
        public void Subscribe(System.Action<float> _listener) => m_OnEventRaised += _listener;
        public void Unsubscribe(System.Action<float> _listener) => m_OnEventRaised -= _listener;

        #endregion
    }
}
