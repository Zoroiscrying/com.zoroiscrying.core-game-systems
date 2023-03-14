using System;

namespace Zoroiscrying.CoreGameSystems.CoreSystemUtility
{
    /// <summary>
    /// A simple timer implemented for code reduction.
    /// The timer's NormalizedValue returns the t, 0 for start, 1 for end.
    /// Use of the Timer:
    /// - 
    /// </summary>
    public class CustomTimer
    {
        public CustomTimer(float initialTime)
        {
            _initialTime = initialTime;
            InitializeTimer();
        }
        public CustomTimer(float initialTime, Action onTimerEnds, bool autoResetTimer = true)
        {
            _initialTime = initialTime;
            OnTimerEnds += onTimerEnds;
            _autoResetTimer = autoResetTimer;
            InitializeTimer();
        }

        private bool _autoResetTimer = true;
        
        public event Action OnTimerEnds = delegate { };
        
        private float _initialTime = 1f;
        private float _time = 0f;

        public bool TimerEnds => _timerEnds;
        
        private bool _timerEnds = false;

        private bool _invokedTimerEndEvent = false;

        public void ResetInitialTime(float initialTime)
        {
            _initialTime = initialTime;
            InitializeTimer();
        }

        public void InitializeTimer()
        {
            _invokedTimerEndEvent = false;
            _time = _initialTime;
            _timerEnds = false;
        }

        /// <summary>
        /// Return the real time value of the timer.
        /// </summary>
        /// <returns></returns>
        public float RealTimeValue()
        {
            return _time;
        }

        /// <summary>
        /// Return the normalized value of the timer.
        /// </summary>
        /// <returns>0 for start, 1 for end.</returns>
        public float NormalizedValue()
        {
            return (_initialTime - _time) / _initialTime;
        }

        public void FinishTimer()
        {
            _time = -0f;
            CheckTimer();
        }

        public void Tick(float deltaTime)
        {
            CheckTimer();

            if (!_timerEnds)
            {
                _time -= deltaTime;
            }
        }

        private void CheckTimer()
        {
            if (_time <= 0f && !_invokedTimerEndEvent)
            {
                _time = 0f;
                _timerEnds = true;
                OnTimerEnds.Invoke();
                _invokedTimerEndEvent = true;
                if (_autoResetTimer)
                {
                    InitializeTimer();   
                }
            }
        }

    }
}