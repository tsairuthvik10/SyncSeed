using UnityEngine;
using UnityEngine.Events;

namespace SyncSeed
{
    /// <summary>
    /// Represents a rhythm node that players must hit in sync with the beat.
    /// Handles timing, collision detection, scoring, and visual feedback.
    /// </summary>
    public class RhythmNode : MonoBehaviour
    {
        #region Events
        
        [Header("Rhythm Node Events")]
        public UnityEvent OnNodeHit;
        public UnityEvent OnNodePulse;
        public UnityEvent OnNodeDestroyed;
        public UnityEvent<int> OnScoreAdded;
        
        #endregion

        #region Serialized Fields
        
        [Header("Timing Configuration")]
        [SerializeField] private float _beatInterval = 1.0f;
        [SerializeField] private bool _autoStart = true;
        [SerializeField] private bool _loopPulses = true;
        
        [Header("Scoring")]
        [SerializeField] private int _scoreValue = 10;
        [SerializeField] private bool _canBeHitMultipleTimes = false;
        
        [Header("Visual Effects")]
        [SerializeField] private bool _enableVisualFeedback = true;
        [SerializeField] private float _hitScaleMultiplier = 1.2f;
        [SerializeField] private float _hitAnimationDuration = 0.1f;
        [SerializeField] private Color _hitColor = Color.yellow;
        
        [Header("Audio & Haptics")]
        [SerializeField] private bool _enableAudioFeedback = true;
        [SerializeField] private bool _enableHapticFeedback = true;
        [SerializeField] private bool _enablePlatformSpecificHaptics = true;
        
        [Header("Destruction")]
        [SerializeField] private float _destroyDelay = 0.3f;
        [SerializeField] private bool _destroyOnHit = true;
        
        #endregion

        #region Properties
        
        public float BeatInterval 
        { 
            get => _beatInterval; 
            set => _beatInterval = Mathf.Max(0.01f, value); 
        }
        
        public int ScoreValue 
        { 
            get => _scoreValue; 
            set => _scoreValue = Mathf.Max(0, value); 
        }
        
        public bool IsHit { get; private set; }
        public bool IsActive { get; private set; }
        public float CurrentTimer { get; private set; }
        
        #endregion

        #region Private Fields
        
        private float _timer = 0f;
        private Renderer _renderer;
        private Color _originalColor;
        private Vector3 _originalScale;
        private bool _isInitialized = false;
        
        #endregion

        #region Unity Lifecycle
        
        private void Start()
        {
            InitializeRhythmNode();
        }
        
        private void Update()
        {
            if (IsActive && !IsHit)
            {
                UpdateTimer();
            }
        }
        
        private void OnDestroy()
        {
            OnNodeDestroyed?.Invoke();
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Starts the rhythm node's pulse cycle.
        /// </summary>
        public void StartNode()
        {
            if (IsActive)
            {
                Debug.LogWarning("Rhythm node is already active.");
                return;
            }
            
            IsActive = true;
            _timer = 0f;
            Debug.Log("Rhythm node started.");
        }
        
        /// <summary>
        /// Stops the rhythm node's pulse cycle.
        /// </summary>
        public void StopNode()
        {
            if (!IsActive)
            {
                Debug.LogWarning("Rhythm node is not active.");
                return;
            }
            
            IsActive = false;
            Debug.Log("Rhythm node stopped.");
        }
        
        /// <summary>
        /// Triggers a pulse effect for the rhythm node.
        /// </summary>
        public void Pulse()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("Rhythm node not initialized. Cannot pulse.");
                return;
            }
            
            // Visual animation pulse
            if (_enableVisualFeedback)
            {
                AnimationController.Ping(transform);
            }
            
            // Haptic feedback for rhythm beat
            if (_enableHapticFeedback)
            {
                TriggerHapticFeedback();
            }
            
            OnNodePulse?.Invoke();
        }
        
        /// <summary>
        /// Manually triggers a hit on the rhythm node.
        /// </summary>
        public void TriggerHit()
        {
            if (IsHit && !_canBeHitMultipleTimes)
            {
                Debug.LogWarning("Rhythm node already hit and cannot be hit multiple times.");
                return;
            }
            
            ProcessHit();
        }
        
        /// <summary>
        /// Sets the beat interval for the rhythm node.
        /// </summary>
        /// <param name="interval">The new beat interval in seconds</param>
        public void SetBeatInterval(float interval)
        {
            BeatInterval = interval;
        }
        
        /// <summary>
        /// Sets the score value for hitting this node.
        /// </summary>
        /// <param name="score">The score value to award</param>
        public void SetScoreValue(int score)
        {
            ScoreValue = score;
        }
        
        /// <summary>
        /// Enables or disables visual feedback.
        /// </summary>
        /// <param name="enabled">Whether to enable visual feedback</param>
        public void SetVisualFeedbackEnabled(bool enabled)
        {
            _enableVisualFeedback = enabled;
        }
        
        /// <summary>
        /// Enables or disables audio feedback.
        /// </summary>
        /// <param name="enabled">Whether to enable audio feedback</param>
        public void SetAudioFeedbackEnabled(bool enabled)
        {
            _enableAudioFeedback = enabled;
        }
        
        /// <summary>
        /// Enables or disables haptic feedback.
        /// </summary>
        /// <param name="enabled">Whether to enable haptic feedback</param>
        public void SetHapticFeedbackEnabled(bool enabled)
        {
            _enableHapticFeedback = enabled;
        }
        
        /// <summary>
        /// Resets the rhythm node to its initial state.
        /// </summary>
        public void ResetNode()
        {
            IsHit = false;
            IsActive = false;
            _timer = 0f;
            
            if (_renderer != null)
            {
                _renderer.material.color = _originalColor;
            }
            
            transform.localScale = _originalScale;
        }
        
        #endregion

        #region Private Methods
        
        /// <summary>
        /// Initializes the rhythm node component.
        /// </summary>
        private void InitializeRhythmNode()
        {
            // Get required components
            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
            {
                _originalColor = _renderer.material.color;
            }
            
            _originalScale = transform.localScale;
            
            // Set initial state
            IsActive = _autoStart;
            IsHit = false;
            _timer = 0f;
            
            _isInitialized = true;
            Debug.Log("Rhythm node initialized successfully.");
        }
        
        /// <summary>
        /// Updates the timer and triggers pulses based on beat interval.
        /// </summary>
        private void UpdateTimer()
        {
            _timer += Time.deltaTime;
            CurrentTimer = _timer;
            
            if (_timer >= _beatInterval)
            {
                _timer = 0f;
                Pulse();
                
                if (!_loopPulses)
                {
                    StopNode();
                }
            }
        }
        
        /// <summary>
        /// Processes a hit on the rhythm node.
        /// </summary>
        private void ProcessHit()
        {
            IsHit = true;
            
            // Add score
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(_scoreValue);
                OnScoreAdded?.Invoke(_scoreValue);
            }
            else
            {
                Debug.LogWarning("GameManager instance not found. Cannot add score.");
            }
            
            // Play sound
            if (_enableAudioFeedback && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayRhythmHitSound();
            }
            
            // Haptic feedback on successful hit
            if (_enableHapticFeedback)
            {
                TriggerHapticFeedback();
            }
            
            // Visual feedback
            if (_enableVisualFeedback)
            {
                ApplyHitVisualEffect();
            }
            
            OnNodeHit?.Invoke();
            
            // Destroy after delay if configured
            if (_destroyOnHit)
            {
                Destroy(gameObject, _destroyDelay);
            }
        }
        
        /// <summary>
        /// Applies visual feedback when the node is hit.
        /// </summary>
        private void ApplyHitVisualEffect()
        {
            if (_renderer != null)
            {
                _renderer.material.color = _hitColor;
            }
            
            LeanTween.scale(gameObject, _originalScale * _hitScaleMultiplier, _hitAnimationDuration)
                .setEasePunch()
                .setOnComplete(() => {
                    if (_renderer != null)
                    {
                        _renderer.material.color = _originalColor;
                    }
                });
        }
        
        /// <summary>
        /// Triggers haptic feedback based on platform.
        /// </summary>
        private void TriggerHapticFeedback()
        {
            if (_enablePlatformSpecificHaptics)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                Handheld.Vibrate();
#endif
            }
            else
            {
                HapticsManager.TriggerPulse();
            }
        }
        
        #endregion

        #region Collision Detection
        
        private void OnTriggerEnter(Collider other)
        {
            if (IsHit && !_canBeHitMultipleTimes)
            {
                return;
            }
            
            if (other.CompareTag("Player"))
            {
                ProcessHit();
            }
        }
        
        #endregion

        #region Validation
        
        private void OnValidate()
        {
            // Ensure beat interval is positive
            _beatInterval = Mathf.Max(0.01f, _beatInterval);
            
            // Ensure score value is non-negative
            _scoreValue = Mathf.Max(0, _scoreValue);
            
            // Ensure hit scale multiplier is positive
            _hitScaleMultiplier = Mathf.Max(0.1f, _hitScaleMultiplier);
            
            // Ensure hit animation duration is positive
            _hitAnimationDuration = Mathf.Max(0.01f, _hitAnimationDuration);
            
            // Ensure destroy delay is non-negative
            _destroyDelay = Mathf.Max(0f, _destroyDelay);
        }
        
        #endregion
    }
}
