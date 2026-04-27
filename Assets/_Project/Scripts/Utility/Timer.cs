using UnityEngine;

namespace SpellStrike.Utility
{
    /// <summary>
    /// Countdown timer — thay thế cho raw Update timer.
    /// Hỗ trợ auto-reset (loop) hoặc one-shot.
    /// </summary>
    public class Timer
    {
        #region Private Fields

        private float m_Duration;
        private float m_RemainingTime;
        private bool m_IsRunning;
        private bool m_IsLooping;

        #endregion

        #region Public Properties

        public bool IsRunning => m_IsRunning;
        public bool IsCompleted => !m_IsRunning && m_RemainingTime <= 0f;
        public float RemainingTime => m_RemainingTime;
        public float Ratio => m_Duration > 0f ? Mathf.Clamp01(1f - m_RemainingTime / m_Duration) : 1f;

        #endregion

        #region Events

        public System.Action OnCompleted;

        #endregion

        #region Constructor

        public Timer(float _duration, bool _isLooping = false)
        {
            m_Duration = _duration;
            m_IsLooping = _isLooping;
            m_RemainingTime = _duration;
            m_IsRunning = false;
        }

        #endregion

        #region Public Methods

        public void Start()
        {
            m_RemainingTime = m_Duration;
            m_IsRunning = true;
        }

        public void Start(float _duration)
        {
            m_Duration = _duration;
            Start();
        }

        public void Stop()
        {
            m_IsRunning = false;
        }

        public void Reset()
        {
            m_RemainingTime = m_Duration;
            m_IsRunning = false;
        }

        /// <summary>
        /// Gọi mỗi frame trong Update(). Trả về true nếu timer vừa hoàn thành.
        /// </summary>
        public bool Tick(float _deltaTime)
        {
            if (!m_IsRunning) return false;

            m_RemainingTime -= _deltaTime;

            if (m_RemainingTime <= 0f)
            {
                OnCompleted?.Invoke();

                if (m_IsLooping)
                {
                    m_RemainingTime += m_Duration;
                    return true;
                }

                m_IsRunning = false;
                m_RemainingTime = 0f;
                return true;
            }

            return false;
        }

        #endregion
    }
}
