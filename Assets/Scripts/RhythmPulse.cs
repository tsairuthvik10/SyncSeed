// RhythmPulse.cs (attach to PlayerRhythmPulse GameObject)
using UnityEngine;
using UnityEngine.Events;

namespace SyncSeed
{
    /// <summary>
    /// Handles rhythm pulse visual and collision effects for the player.
    /// Creates expanding pulse waves that can interact with rhythm nodes.
    /// </summary>
    public class RhythmPulse : MonoBehaviour
    {
        #region Events
        
        [Header("Pulse Events")]
        public UnityEvent OnPulseStarted;
        public UnityEvent OnPulseEnded;
        public UnityEvent<float> OnPulseRadiusChanged;
        
        #endregion

        #region Serialized Fields
        
        [Header("Pulse Configuration")]
        [SerializeField] private float _pulseRadius = 2f;
        [SerializeField] private float _pulseDuration = 0.2f;
        [SerializeField] private bool _autoSubscribeToRhythmManager = true;
        [SerializeField] private bool _enableVisualEffects = true;
        [SerializeField] private bool _enableCollisionEffects = true;
        
        [Header("Visual Effects")]
        [SerializeField] private Material _pulseMaterial;
        [SerializeField] private Color _pulseColor = Color.white;
        [SerializeField] private float _pulseAlpha = 0.5f;
        
        #endregion

        #region Properties
        
        public float PulseRadius 
        { 
            get => _pulseRadius; 
            set 
            {
                _pulseRadius = Mathf.Max(0.1f, value);
                if (_sphereCollider != null)
                {
                    _sphereCollider.radius = _pulseRadius;
                }
                OnPulseRadiusChanged?.Invoke(_pulseRadius);
            }
        }
        
        public float PulseDuration 
        { 
            get => _pulseDuration; 
            set => _pulseDuration = Mathf.Max(0.01f, value); 
        }
        
        public bool IsPulseActive { get; private set; }
        public bool EnableVisualEffects 
        { 
            get => _enableVisualEffects; 
            set => _enableVisualEffects = value; 
        }
        
        public bool EnableCollisionEffects 
        { 
            get => _enableCollisionEffects; 
            set => _enableCollisionEffects = value; 
        }
        
        #endregion

        #region Private Fields
        
        private SphereCollider _sphereCollider;
        private Renderer _renderer;
        private Material _originalMaterial;
        private Color _originalColor;
        private bool _isInitialized = false;
        
        #endregion

        #region Unity Lifecycle
        
        private void Start()
        {
            InitializeRhythmPulse();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromRhythmManager();
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Triggers a pulse effect with visual and collision components.
        /// </summary>
        public void TriggerPulse()
        {
            if (IsPulseActive)
            {
                Debug.LogWarning("Pulse is already active. Cannot trigger new pulse.");
                return;
            }
            
            StartCoroutine(PulseRoutine());
        }
        
        /// <summary>
        /// Sets the pulse radius.
        /// </summary>
        /// <param name="radius">The new pulse radius</param>
        public void SetPulseRadius(float radius)
        {
            PulseRadius = radius;
        }
        
        /// <summary>
        /// Sets the pulse duration.
        /// </summary>
        /// <param name="duration">The new pulse duration in seconds</param>
        public void SetPulseDuration(float duration)
        {
            PulseDuration = duration;
        }
        
        /// <summary>
        /// Enables or disables visual effects.
        /// </summary>
        /// <param name="enabled">Whether to enable visual effects</param>
        public void SetVisualEffectsEnabled(bool enabled)
        {
            EnableVisualEffects = enabled;
        }
        
        /// <summary>
        /// Enables or disables collision effects.
        /// </summary>
        /// <param name="enabled">Whether to enable collision effects</param>
        public void SetCollisionEffectsEnabled(bool enabled)
        {
            EnableCollisionEffects = enabled;
        }
        
        /// <summary>
        /// Sets the pulse color.
        /// </summary>
        /// <param name="color">The new pulse color</param>
        public void SetPulseColor(Color color)
        {
            _pulseColor = color;
        }
        
        /// <summary>
        /// Sets the pulse alpha transparency.
        /// </summary>
        /// <param name="alpha">The new alpha value (0-1)</param>
        public void SetPulseAlpha(float alpha)
        {
            _pulseAlpha = Mathf.Clamp01(alpha);
        }
        
        /// <summary>
        /// Manually subscribes to the RhythmManager events.
        /// </summary>
        public void SubscribeToRhythmManager()
        {
            if (RhythmManager.Instance != null)
            {
                RhythmManager.Instance.OnPulseTriggered.AddListener(TriggerPulse);
                Debug.Log("RhythmPulse subscribed to RhythmManager events.");
            }
            else
            {
                Debug.LogWarning("RhythmManager instance not found. Cannot subscribe to events.");
            }
        }
        
        /// <summary>
        /// Manually unsubscribes from the RhythmManager events.
        /// </summary>
        public void UnsubscribeFromRhythmManager()
        {
            if (RhythmManager.Instance != null)
            {
                RhythmManager.Instance.OnPulseTriggered.RemoveListener(TriggerPulse);
                Debug.Log("RhythmPulse unsubscribed from RhythmManager events.");
            }
        }
        
        #endregion

        #region Private Methods
        
        /// <summary>
        /// Initializes the rhythm pulse component.
        /// </summary>
        private void InitializeRhythmPulse()
        {
            // Get required components
            _sphereCollider = GetComponent<SphereCollider>();
            if (_sphereCollider == null)
            {
                Debug.LogError("SphereCollider component not found on RhythmPulse GameObject.");
                return;
            }
            
            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
            {
                _originalMaterial = _renderer.material;
                _originalColor = _renderer.material.color;
            }
            
            // Set initial state
            _sphereCollider.enabled = false;
            _sphereCollider.radius = _pulseRadius;
            
            // Subscribe to rhythm manager if auto-subscribe is enabled
            if (_autoSubscribeToRhythmManager)
            {
                SubscribeToRhythmManager();
            }
            
            _isInitialized = true;
            Debug.Log("RhythmPulse initialized successfully.");
        }
        
        /// <summary>
        /// Coroutine that handles the pulse effect.
        /// </summary>
        private System.Collections.IEnumerator PulseRoutine()
        {
            if (!_isInitialized)
            {
                Debug.LogError("RhythmPulse not initialized. Cannot start pulse routine.");
                yield break;
            }
            
            IsPulseActive = true;
            OnPulseStarted?.Invoke();
            
            // Enable collision effects
            if (_enableCollisionEffects && _sphereCollider != null)
            {
                _sphereCollider.radius = _pulseRadius;
                _sphereCollider.enabled = true;
            }
            
            // Apply visual effects
            if (_enableVisualEffects && _renderer != null)
            {
                ApplyPulseVisualEffect();
            }
            
            // Wait for pulse duration
            yield return new WaitForSeconds(_pulseDuration);
            
            // Disable collision effects
            if (_sphereCollider != null)
            {
                _sphereCollider.enabled = false;
            }
            
            // Restore visual effects
            if (_renderer != null)
            {
                RestoreOriginalVisualEffect();
            }
            
            IsPulseActive = false;
            OnPulseEnded?.Invoke();
        }
        
        /// <summary>
        /// Applies visual effects for the pulse.
        /// </summary>
        private void ApplyPulseVisualEffect()
        {
            if (_renderer == null) return;
            
            if (_pulseMaterial != null)
            {
                _renderer.material = _pulseMaterial;
            }
            
            Color pulseColor = _pulseColor;
            pulseColor.a = _pulseAlpha;
            _renderer.material.color = pulseColor;
        }
        
        /// <summary>
        /// Restores the original visual appearance.
        /// </summary>
        private void RestoreOriginalVisualEffect()
        {
            if (_renderer == null) return;
            
            if (_originalMaterial != null)
            {
                _renderer.material = _originalMaterial;
            }
            else
            {
                _renderer.material.color = _originalColor;
            }
        }
        
        #endregion

        #region Validation
        
        private void OnValidate()
        {
            // Ensure pulse radius is positive
            _pulseRadius = Mathf.Max(0.1f, _pulseRadius);
            
            // Ensure pulse duration is positive
            _pulseDuration = Mathf.Max(0.01f, _pulseDuration);
            
            // Ensure alpha is in valid range
            _pulseAlpha = Mathf.Clamp01(_pulseAlpha);
        }
        
        #endregion
    }
}

