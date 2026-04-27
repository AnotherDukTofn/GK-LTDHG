using UnityEngine;

namespace SpellStrike.Core
{
    /// <summary>
    /// Game state tổng thể — quản lý bởi GameManager.
    /// </summary>
    public enum GameState
    {
        MainMenu,
        SpellSelection,
        Gameplay,
        Win,
        Lose
    }

    /// <summary>
    /// Singleton quản lý game state, scene transitions.
    /// Tồn tại xuyên suốt các scene (DontDestroyOnLoad).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Singleton

        private static GameManager s_Instance;
        public static GameManager Instance => s_Instance;

        #endregion

        #region Serialized Fields

        [SerializeField] private EventChannels.VoidEventChannelSO m_OnGameWin;
        [SerializeField] private EventChannels.VoidEventChannelSO m_OnGameLose;
        [SerializeField] private EventChannels.VoidEventChannelSO m_OnPlayerDeath;

        #endregion

        #region Private Fields

        private GameState m_CurrentState;

        #endregion

        #region Public Properties

        public GameState CurrentState => m_CurrentState;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_Instance = this;
            DontDestroyOnLoad(gameObject);
            m_CurrentState = GameState.MainMenu;
        }

        private void OnEnable()
        {
            if (m_OnPlayerDeath != null) m_OnPlayerDeath.Subscribe(HandlePlayerDeath);
        }

        private void OnDisable()
        {
            if (m_OnPlayerDeath != null) m_OnPlayerDeath.Unsubscribe(HandlePlayerDeath);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Chuyển trạng thái game. Gọi event tương ứng nếu Win/Lose.
        /// </summary>
        public void ChangeState(GameState _newState)
        {
            m_CurrentState = _newState;

            switch (_newState)
            {
                case GameState.Win:
                    m_OnGameWin?.RaiseEvent();
                    break;
                case GameState.Lose:
                    m_OnGameLose?.RaiseEvent();
                    break;
            }
        }

        #endregion

        #region Private Methods

        private void HandlePlayerDeath()
        {
            ChangeState(GameState.Lose);
        }

        #endregion
    }
}
