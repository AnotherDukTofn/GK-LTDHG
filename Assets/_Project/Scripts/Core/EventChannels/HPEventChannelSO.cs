using UnityEngine;

namespace SpellStrike.Core.EventChannels
{
    /// <summary>
    /// Dữ liệu HP truyền qua event — chứa cả current và max.
    /// </summary>
    [System.Serializable]
    public struct HPData
    {
        public int Current;
        public int Max;

        public HPData(int _current, int _max)
        {
            Current = _current;
            Max = _max;
        }

        public float Ratio => Max > 0 ? (float)Current / Max : 0f;
    }

    [CreateAssetMenu(menuName = "Events/HP Event Channel")]
    public class HPEventChannelSO : ScriptableObject
    {
        #region Private Fields

        private System.Action<HPData> m_OnEventRaised;

        #endregion

        #region Public Methods

        public void RaiseEvent(HPData _value) => m_OnEventRaised?.Invoke(_value);
        public void Subscribe(System.Action<HPData> _listener) => m_OnEventRaised += _listener;
        public void Unsubscribe(System.Action<HPData> _listener) => m_OnEventRaised -= _listener;

        #endregion
    }
}
