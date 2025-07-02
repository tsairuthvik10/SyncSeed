using UnityEngine;
using UnityEngine.Events;

namespace SyncSeed
{
    /// <summary>
    /// Provides utility methods for common animation effects and transitions.
    /// Centralizes animation functionality using LeanTween for smooth animations.
    /// </summary>
    public static class AnimationController
    {
        #region Events
        
        public static event System.Action<Transform> OnAnimationStarted;
        public static event System.Action<Transform> OnAnimationCompleted;
        
        #endregion

        #region Configuration
        
        private static float _defaultDuration = 0.1f;
        private static float _defaultScale = 1.2f;
        private static LeanTweenType _defaultEaseType = LeanTweenType.easeOutBack;
        
        #endregion

        #region Properties
        
        public static float DefaultDuration 
        { 
            get => _defaultDuration; 
            set => _defaultDuration = Mathf.Max(0.01f, value); 
        }
        
        public static float DefaultScale 
        { 
            get => _defaultScale; 
            set => _defaultScale = Mathf.Max(0.1f, value); 
        }
        
        public static LeanTweenType DefaultEaseType 
        { 
            get => _defaultEaseType; 
            set => _defaultEaseType = value; 
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Creates a ping-pong scale animation on the target transform.
        /// </summary>
        /// <param name="target">The transform to animate</param>
        public static void Ping(Transform target)
        {
            Ping(target, _defaultScale, _defaultDuration);
        }
        
        /// <summary>
        /// Creates a ping-pong scale animation with custom parameters.
        /// </summary>
        /// <param name="target">The transform to animate</param>
        /// <param name="scale">The scale multiplier for the animation</param>
        /// <param name="duration">The duration of the animation</param>
        public static void Ping(Transform target, float scale, float duration)
        {
            if (target == null)
            {
                Debug.LogWarning("Target transform is null. Cannot perform ping animation.");
                return;
            }
            
            if (scale <= 0f)
            {
                Debug.LogWarning("Scale must be positive. Cannot perform ping animation.");
                return;
            }
            
            if (duration <= 0f)
            {
                Debug.LogWarning("Duration must be positive. Cannot perform ping animation.");
                return;
            }
            
            try
            {
                Vector3 originalScale = target.localScale;
                Vector3 targetScale = originalScale * scale;
                
                OnAnimationStarted?.Invoke(target);
                
                LeanTween.scale(target.gameObject, targetScale, duration)
                    .setEase(_defaultEaseType)
                    .setLoopPingPong(1)
                    .setOnComplete(() => {
                        OnAnimationCompleted?.Invoke(target);
                    });
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error performing ping animation: {e.Message}");
            }
        }
        
        /// <summary>
        /// Creates a bounce animation on the target transform.
        /// </summary>
        /// <param name="target">The transform to animate</param>
        public static void Bounce(Transform target)
        {
            Bounce(target, _defaultScale, _defaultDuration);
        }
        
        /// <summary>
        /// Creates a bounce animation with custom parameters.
        /// </summary>
        /// <param name="target">The transform to animate</param>
        /// <param name="scale">The scale multiplier for the bounce</param>
        /// <param name="duration">The duration of the animation</param>
        public static void Bounce(Transform target, float scale, float duration)
        {
            if (target == null)
            {
                Debug.LogWarning("Target transform is null. Cannot perform bounce animation.");
                return;
            }
            
            try
            {
                Vector3 originalScale = target.localScale;
                Vector3 targetScale = originalScale * scale;
                
                OnAnimationStarted?.Invoke(target);
                
                LeanTween.scale(target.gameObject, targetScale, duration)
                    .setEase(LeanTweenType.easeOutBounce)
                    .setOnComplete(() => {
                        LeanTween.scale(target.gameObject, originalScale, duration * 0.5f)
                            .setEase(LeanTweenType.easeInOutQuad)
                            .setOnComplete(() => {
                                OnAnimationCompleted?.Invoke(target);
                            });
                    });
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error performing bounce animation: {e.Message}");
            }
        }
        
        /// <summary>
        /// Creates a fade in animation on the target GameObject.
        /// </summary>
        /// <param name="target">The GameObject to animate</param>
        /// <param name="duration">The duration of the fade</param>
        public static void FadeIn(GameObject target, float duration = 0.5f)
        {
            if (target == null)
            {
                Debug.LogWarning("Target GameObject is null. Cannot perform fade in animation.");
                return;
            }
            
            try
            {
                CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = target.AddComponent<CanvasGroup>();
                }
                
                canvasGroup.alpha = 0f;
                target.SetActive(true);
                
                OnAnimationStarted?.Invoke(target.transform);
                
                LeanTween.alphaCanvas(canvasGroup, 1f, duration)
                    .setEase(LeanTweenType.easeInOutQuad)
                    .setOnComplete(() => {
                        OnAnimationCompleted?.Invoke(target.transform);
                    });
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error performing fade in animation: {e.Message}");
            }
        }
        
        /// <summary>
        /// Creates a fade out animation on the target GameObject.
        /// </summary>
        /// <param name="target">The GameObject to animate</param>
        /// <param name="duration">The duration of the fade</param>
        /// <param name="deactivateOnComplete">Whether to deactivate the GameObject when complete</param>
        public static void FadeOut(GameObject target, float duration = 0.5f, bool deactivateOnComplete = true)
        {
            if (target == null)
            {
                Debug.LogWarning("Target GameObject is null. Cannot perform fade out animation.");
                return;
            }
            
            try
            {
                CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = target.AddComponent<CanvasGroup>();
                }
                
                OnAnimationStarted?.Invoke(target.transform);
                
                LeanTween.alphaCanvas(canvasGroup, 0f, duration)
                    .setEase(LeanTweenType.easeInOutQuad)
                    .setOnComplete(() => {
                        if (deactivateOnComplete)
                        {
                            target.SetActive(false);
                        }
                        OnAnimationCompleted?.Invoke(target.transform);
                    });
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error performing fade out animation: {e.Message}");
            }
        }
        
        /// <summary>
        /// Creates a rotation animation on the target transform.
        /// </summary>
        /// <param name="target">The transform to animate</param>
        /// <param name="rotation">The target rotation</param>
        /// <param name="duration">The duration of the rotation</param>
        public static void Rotate(Transform target, Vector3 rotation, float duration = 1f)
        {
            if (target == null)
            {
                Debug.LogWarning("Target transform is null. Cannot perform rotation animation.");
                return;
            }
            
            try
            {
                OnAnimationStarted?.Invoke(target);
                
                LeanTween.rotate(target.gameObject, rotation, duration)
                    .setEase(LeanTweenType.easeInOutQuad)
                    .setOnComplete(() => {
                        OnAnimationCompleted?.Invoke(target);
                    });
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error performing rotation animation: {e.Message}");
            }
        }
        
        /// <summary>
        /// Creates a move animation on the target transform.
        /// </summary>
        /// <param name="target">The transform to animate</param>
        /// <param name="position">The target position</param>
        /// <param name="duration">The duration of the movement</param>
        public static void Move(Transform target, Vector3 position, float duration = 1f)
        {
            if (target == null)
            {
                Debug.LogWarning("Target transform is null. Cannot perform move animation.");
                return;
            }
            
            try
            {
                OnAnimationStarted?.Invoke(target);
                
                LeanTween.move(target.gameObject, position, duration)
                    .setEase(LeanTweenType.easeInOutQuad)
                    .setOnComplete(() => {
                        OnAnimationCompleted?.Invoke(target);
                    });
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error performing move animation: {e.Message}");
            }
        }
        
        /// <summary>
        /// Stops all animations on the target GameObject.
        /// </summary>
        /// <param name="target">The GameObject to stop animations on</param>
        public static void StopAnimations(GameObject target)
        {
            if (target == null)
            {
                Debug.LogWarning("Target GameObject is null. Cannot stop animations.");
                return;
            }
            
            try
            {
                LeanTween.cancel(target);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error stopping animations: {e.Message}");
            }
        }
        
        /// <summary>
        /// Sets the default animation duration.
        /// </summary>
        /// <param name="duration">Default duration in seconds</param>
        public static void SetDefaultDuration(float duration)
        {
            DefaultDuration = duration;
        }
        
        /// <summary>
        /// Sets the default animation scale.
        /// </summary>
        /// <param name="scale">Default scale multiplier</param>
        public static void SetDefaultScale(float scale)
        {
            DefaultScale = scale;
        }
        
        /// <summary>
        /// Sets the default ease type for animations.
        /// </summary>
        /// <param name="easeType">Default ease type</param>
        public static void SetDefaultEaseType(LeanTweenType easeType)
        {
            DefaultEaseType = easeType;
        }
        
        /// <summary>
        /// Tests animation functionality with a sample animation.
        /// </summary>
        /// <param name="target">The target to test with</param>
        public static void TestAnimation(Transform target)
        {
            if (target == null)
            {
                Debug.LogWarning("Target transform is null. Cannot test animation.");
                return;
            }
            
            Debug.Log("Testing animation functionality...");
            Ping(target);
        }
        
        #endregion

        #region Validation
        
        /// <summary>
        /// Validates animation settings and ensures they are within acceptable ranges.
        /// </summary>
        public static void ValidateSettings()
        {
            _defaultDuration = Mathf.Max(0.01f, _defaultDuration);
            _defaultScale = Mathf.Max(0.1f, _defaultScale);
            
            Debug.Log($"AnimationController settings validated - Duration: {_defaultDuration}, Scale: {_defaultScale}, Ease: {_defaultEaseType}");
        }
        
        #endregion
    }
}