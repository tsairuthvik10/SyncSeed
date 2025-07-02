using UnityEngine;
using UnityEngine.Events;

namespace SyncSeed
{
    /// <summary>
    /// Manages haptic feedback across different platforms and devices.
    /// Provides centralized haptic control with intensity and duration options.
    /// </summary>
    public static class HapticsManager
    {
        #region Events
        
        public static event System.Action OnHapticTriggered;
        public static event System.Action<float> OnHapticIntensityChanged;
        
        #endregion

        #region Configuration
        
        private static bool _isEnabled = true;
        private static float _intensity = 1.0f;
        private static float _duration = 0.1f;
        private static bool _usePlatformSpecificHaptics = true;
        
        #endregion

        #region Properties
        
        public static bool IsEnabled 
        { 
            get => _isEnabled; 
            set => _isEnabled = value; 
        }
        
        public static float Intensity 
        { 
            get => _intensity; 
            set => _intensity = Mathf.Clamp01(value); 
        }
        
        public static float Duration 
        { 
            get => _duration; 
            set => _duration = Mathf.Max(0.01f, value); 
        }
        
        public static bool UsePlatformSpecificHaptics 
        { 
            get => _usePlatformSpecificHaptics; 
            set => _usePlatformSpecificHaptics = value; 
        }
        
        public static bool IsSupported => IsHapticSupported();
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Triggers a haptic pulse with default settings.
        /// </summary>
        public static void TriggerPulse()
        {
            if (!_isEnabled)
            {
                Debug.Log("Haptics are disabled. Skipping haptic pulse.");
                return;
            }
            
            if (!IsSupported)
            {
                Debug.LogWarning("Haptic feedback is not supported on this platform.");
                return;
            }
            
            TriggerHapticFeedback(_intensity, _duration);
            OnHapticTriggered?.Invoke();
        }
        
        /// <summary>
        /// Triggers a haptic pulse with custom intensity and duration.
        /// </summary>
        /// <param name="intensity">Haptic intensity (0-1)</param>
        /// <param name="duration">Haptic duration in seconds</param>
        public static void TriggerPulse(float intensity, float duration)
        {
            if (!_isEnabled)
            {
                Debug.Log("Haptics are disabled. Skipping haptic pulse.");
                return;
            }
            
            if (!IsSupported)
            {
                Debug.LogWarning("Haptic feedback is not supported on this platform.");
                return;
            }
            
            float clampedIntensity = Mathf.Clamp01(intensity);
            float clampedDuration = Mathf.Max(0.01f, duration);
            
            TriggerHapticFeedback(clampedIntensity, clampedDuration);
            OnHapticTriggered?.Invoke();
            OnHapticIntensityChanged?.Invoke(clampedIntensity);
        }
        
        /// <summary>
        /// Triggers a light haptic feedback.
        /// </summary>
        public static void TriggerLightPulse()
        {
            TriggerPulse(0.3f, 0.05f);
        }
        
        /// <summary>
        /// Triggers a medium haptic feedback.
        /// </summary>
        public static void TriggerMediumPulse()
        {
            TriggerPulse(0.6f, 0.1f);
        }
        
        /// <summary>
        /// Triggers a heavy haptic feedback.
        /// </summary>
        public static void TriggerHeavyPulse()
        {
            TriggerPulse(1.0f, 0.2f);
        }
        
        /// <summary>
        /// Triggers a haptic feedback for rhythm hits.
        /// </summary>
        public static void TriggerRhythmPulse()
        {
            TriggerPulse(0.7f, 0.08f);
        }
        
        /// <summary>
        /// Triggers a haptic feedback for level completion.
        /// </summary>
        public static void TriggerLevelCompletePulse()
        {
            TriggerPulse(0.8f, 0.15f);
        }
        
        /// <summary>
        /// Triggers a haptic feedback for game over.
        /// </summary>
        public static void TriggerGameOverPulse()
        {
            TriggerPulse(1.0f, 0.3f);
        }
        
        /// <summary>
        /// Enables or disables haptic feedback.
        /// </summary>
        /// <param name="enabled">Whether to enable haptics</param>
        public static void SetEnabled(bool enabled)
        {
            IsEnabled = enabled;
        }
        
        /// <summary>
        /// Sets the default haptic intensity.
        /// </summary>
        /// <param name="intensity">Intensity level (0-1)</param>
        public static void SetIntensity(float intensity)
        {
            Intensity = intensity;
        }
        
        /// <summary>
        /// Sets the default haptic duration.
        /// </summary>
        /// <param name="duration">Duration in seconds</param>
        public static void SetDuration(float duration)
        {
            Duration = duration;
        }
        
        /// <summary>
        /// Enables or disables platform-specific haptic implementations.
        /// </summary>
        /// <param name="enabled">Whether to use platform-specific haptics</param>
        public static void SetPlatformSpecificHaptics(bool enabled)
        {
            UsePlatformSpecificHaptics = enabled;
        }
        
        /// <summary>
        /// Tests haptic feedback with current settings.
        /// </summary>
        public static void TestHaptics()
        {
            Debug.Log("Testing haptic feedback...");
            TriggerPulse();
        }
        
        /// <summary>
        /// Tests haptic feedback with custom settings.
        /// </summary>
        /// <param name="intensity">Test intensity</param>
        /// <param name="duration">Test duration</param>
        public static void TestHaptics(float intensity, float duration)
        {
            Debug.Log($"Testing haptic feedback with intensity: {intensity}, duration: {duration}");
            TriggerPulse(intensity, duration);
        }
        
        #endregion

        #region Private Methods
        
        /// <summary>
        /// Triggers the actual haptic feedback based on platform.
        /// </summary>
        /// <param name="intensity">Haptic intensity</param>
        /// <param name="duration">Haptic duration</param>
        private static void TriggerHapticFeedback(float intensity, float duration)
        {
            if (_usePlatformSpecificHaptics)
            {
                TriggerPlatformSpecificHaptics(intensity, duration);
            }
            else
            {
                TriggerGenericHaptics(intensity, duration);
            }
        }
        
        /// <summary>
        /// Triggers platform-specific haptic feedback.
        /// </summary>
        /// <param name="intensity">Haptic intensity</param>
        /// <param name="duration">Haptic duration</param>
        private static void TriggerPlatformSpecificHaptics(float intensity, float duration)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // Android-specific haptic feedback
            if (intensity > 0.5f)
            {
                Handheld.Vibrate();
            }
            else
            {
                // For lighter haptics, we could use Android's Vibration API
                // This is a simplified implementation
                Handheld.Vibrate();
            }
#elif UNITY_IOS && !UNITY_EDITOR
            // iOS-specific haptic feedback using iOS Haptic Engine
            // This would require native iOS plugin integration
            Debug.Log("iOS haptic feedback not implemented. Using generic fallback.");
            TriggerGenericHaptics(intensity, duration);
#else
            // Fallback for other platforms or editor
            TriggerGenericHaptics(intensity, duration);
#endif
        }
        
        /// <summary>
        /// Triggers generic haptic feedback that works across platforms.
        /// </summary>
        /// <param name="intensity">Haptic intensity</param>
        /// <param name="duration">Haptic duration</param>
        private static void TriggerGenericHaptics(float intensity, float duration)
        {
            // Generic haptic feedback using Unity's built-in vibration
            if (intensity > 0.3f) // Only trigger for meaningful intensity
            {
                Handheld.Vibrate();
            }
        }
        
        /// <summary>
        /// Checks if haptic feedback is supported on the current platform.
        /// </summary>
        /// <returns>True if haptics are supported</returns>
        private static bool IsHapticSupported()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return true; // Android supports vibration
#elif UNITY_IOS && !UNITY_EDITOR
            return true; // iOS supports haptic feedback (with proper implementation)
#else
            return false; // Other platforms may not support haptics
#endif
        }
        
        #endregion

        #region Validation
        
        /// <summary>
        /// Validates haptic settings and ensures they are within acceptable ranges.
        /// </summary>
        public static void ValidateSettings()
        {
            _intensity = Mathf.Clamp01(_intensity);
            _duration = Mathf.Max(0.01f, _duration);
            
            Debug.Log($"HapticsManager settings validated - Enabled: {_isEnabled}, Intensity: {_intensity}, Duration: {_duration}");
        }
        
        #endregion
    }
}