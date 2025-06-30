using UnityEngine;

public class RhythmNode : MonoBehaviour
{
    public float beatInterval = 1.0f; // Time between pulses
    private float timer = 0f;
    private bool isHit = false;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= beatInterval)
        {
            timer = 0f;
            Pulse();
        }
    }

    void Pulse()
    {
        // Optional: Visual animation pulse
        AnimationController.Ping(transform);

        // Haptic feedback for rhythm beat
#if UNITY_ANDROID && !UNITY_EDITOR
        HapticsManager.TriggerPulse();
#endif
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isHit) return;

        if (other.CompareTag("Player"))
        {
            isHit = true;

            // Add score
            GameManager.Instance.AddScore(10);

            // Play sound
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayRhythmHitSound();

            // Haptic on successful hit
#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif

            // Visual feedback
            LeanTween.scale(gameObject, Vector3.one * 1.2f, 0.1f).setEasePunch();

            // Destroy after short delay
            Destroy(gameObject, 0.3f);
        }
    }
}
