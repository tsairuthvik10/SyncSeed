using UnityEngine;
using UnityEngine.Events;

namespace SyncSeed
{
    /// <summary>
    /// Manages rhythm timing and pulse events for the rhythm-based gameplay.
    /// Provides centralized rhythm control and haptic feedback coordination.
    /// </summary>
    public class RhythmManager : MonoBehaviour
    {
        #region Singleton Implementation
        
        public static RhythmManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeRhythmManager();
            }
            else if (Instance != this)
            {
                Debug.LogWarning("Multiple RhythmManager instances detected. Destroying duplicate.");
                Destroy(gameObject);
            }
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        #endregion

        #region Events
        
        [Header("Rhythm Events")]
        public UnityEvent OnPulseTriggered;
        public UnityEvent<float> OnBeatIntervalChanged;
        public UnityEvent OnRhythmStarted;
        public UnityEvent OnRhythmStopped;
        
        #endregion

        #region Serialized Fields
        
        [Header("Rhythm Configuration")]
        [SerializeField] private float _beatInterval = 2.0f;
        [SerializeField] private bool _autoStart = true;
        [SerializeField] private bool _enableHaptics = true;
        [SerializeField] private float _minBeatInterval = 0.1f;
        [SerializeField] private float _maxBeatInterval = 10.0f;
        
        #endregion

        #region Properties
        
        public float BeatInterval 
        { 
            get => _beatInterval; 
            set 
            {
                float newInterval = Mathf.Clamp(value, _minBeatInterval, _maxBeatInterval);
                if (_beatInterval != newInterval)
                {
                    _beatInterval = newInterval;
                    OnBeatIntervalChanged?.Invoke(_beatInterval);
                }
            }
        }
        
        public bool IsRhythmActive { get; private set; }
        public bool EnableHaptics 
        { 
            get => _enableHaptics; 
            set => _enableHaptics = value; 
        }
        
        public float MinBeatInterval 
        { 
            get => _minBeatInterval; 
            set => _minBeatInterval = Mathf.Max(0.01f, value); 
        }
        
        public float MaxBeatInterval 
        { 
            get => _maxBeatInterval; 
            set => _maxBeatInterval = Mathf.Max(_minBeatInterval, value); 
        }
        
        #endregion

        #region Unity Lifecycle
        
        private void Start()
        {
            if (Instance == this && _autoStart)
            {
                StartRhythm();
            }
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Triggers a rhythm pulse with haptic feedback.
        /// </summary>
        public void Pulse()
        {
            if (_enableHaptics)
            {
                HapticsManager.TriggerPulse();
            }
            
            OnPulseTriggered?.Invoke();
            Debug.Log($"Rhythm pulse triggered at interval: {_beatInterval}s");
        }
        
        /// <summary>
        /// Sets the beat interval for rhythm timing.
        /// </summary>
        /// <param name="interval">The new beat interval in seconds</param>
        public void SetBeatInterval(float interval)
        {
            BeatInterval = interval;
        }
        
        /// <summary>
        /// Starts the rhythm system.
        /// </summary>
        public void StartRhythm()
        {
            if (IsRhythmActive)
            {
                Debug.LogWarning("Rhythm is already active.");
                return;
            }
            
            IsRhythmActive = true;
            OnRhythmStarted?.Invoke();
            Debug.Log("Rhythm system started.");
        }
        
        /// <summary>
        /// Stops the rhythm system.
        /// </summary>
        public void StopRhythm()
        {
            if (!IsRhythmActive)
            {
                Debug.LogWarning("Rhythm is not active.");
                return;
            }
            
            IsRhythmActive = false;
            OnRhythmStopped?.Invoke();
            Debug.Log("Rhythm system stopped.");
        }
        
        /// <summary>
        /// Toggles the rhythm system on/off.
        /// </summary>
        public void ToggleRhythm()
        {
            if (IsRhythmActive)
            {
                StopRhythm();
            }
            else
            {
                StartRhythm();
            }
        }
        
        /// <summary>
        /// Enables or disables haptic feedback.
        /// </summary>
        /// <param name="enabled">Whether to enable haptics</param>
        public void SetHapticsEnabled(bool enabled)
        {
            EnableHaptics = enabled;
        }
        
        /// <summary>
        /// Sets the minimum beat interval.
        /// </summary>
        /// <param name="minInterval">Minimum interval in seconds</param>
        public void SetMinBeatInterval(float minInterval)
        {
            MinBeatInterval = minInterval;
        }
        
        /// <summary>
        /// Sets the maximum beat interval.
        /// </summary>
        /// <param name="maxInterval">Maximum interval in seconds</param>
        public void SetMaxBeatInterval(float maxInterval)
        {
            MaxBeatInterval = maxInterval;
        }
        
        /// <summary>
        /// Adjusts the beat interval based on level difficulty.
        /// </summary>
        /// <param name="level">Current level number</param>
        /// <param name="baseInterval">Base interval for level 1</param>
        /// <param name="reductionPerLevel">Interval reduction per level</param>
        public void AdjustBeatIntervalForLevel(int level, float baseInterval, float reductionPerLevel)
        {
            float newInterval = baseInterval - (level - 1) * reductionPerLevel;
            BeatInterval = newInterval;
        }
        
        #endregion

        #region Private Methods
        
        /// <summary>
        /// Initializes the rhythm manager with default values.
        /// </summary>
        private void InitializeRhythmManager()
        {
            // Validate configuration
            if (_minBeatInterval < 0.01f)
            {
                Debug.LogWarning("Minimum beat interval must be at least 0.01 seconds. Setting to 0.01.");
                _minBeatInterval = 0.01f;
            }
            
            if (_maxBeatInterval < _minBeatInterval)
            {
                Debug.LogWarning("Maximum beat interval must be greater than minimum. Adjusting.");
                _maxBeatInterval = _minBeatInterval + 5.0f;
            }
            
            if (_beatInterval < _minBeatInterval || _beatInterval > _maxBeatInterval)
            {
                Debug.LogWarning("Beat interval is outside valid range. Clamping to valid range.");
                _beatInterval = Mathf.Clamp(_beatInterval, _minBeatInterval, _maxBeatInterval);
            }
            
            Debug.Log("RhythmManager initialized successfully.");
        }
        
        #endregion

        #region Validation
        
        private void OnValidate()
        {
            // Ensure minimum interval is at least 0.01 seconds
            _minBeatInterval = Mathf.Max(0.01f, _minBeatInterval);
            
            // Ensure maximum interval is greater than minimum
            _maxBeatInterval = Mathf.Max(_minBeatInterval, _maxBeatInterval);
            
            // Ensure beat interval is within bounds
            _beatInterval = Mathf.Clamp(_beatInterval, _minBeatInterval, _maxBeatInterval);
        }
        
        #endregion
    }
}