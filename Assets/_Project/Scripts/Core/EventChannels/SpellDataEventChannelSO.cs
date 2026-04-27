using UnityEngine;

namespace SpellStrike.Core.EventChannels
{
    /// <summary>
    /// SO Event Channel truyền SpellDataSO — dùng cho OnSpellEquipped, OnSpellUnlocked.
    /// Cần forward-declare vì SpellDataSO nằm ở namespace khác.
    /// </summary>
    [CreateAssetMenu(menuName = "Events/SpellData Event Channel")]
    public class SpellDataEventChannelSO : ScriptableObject
    {
        #region Private Fields

        private System.Action<SpellStrike.Data.SpellDataSO> m_OnEventRaised;

        #endregion

        #region Public Methods

        public void RaiseEvent(SpellStrike.Data.SpellDataSO _value) => m_OnEventRaised?.Invoke(_value);
        public void Subscribe(System.Action<SpellStrike.Data.SpellDataSO> _listener) => m_OnEventRaised += _listener;
        public void Unsubscribe(System.Action<SpellStrike.Data.SpellDataSO> _listener) => m_OnEventRaised -= _listener;

        #endregion
    }
}
