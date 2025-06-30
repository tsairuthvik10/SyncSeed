using UnityEngine;

public class RhythmManager : MonoBehaviour
{
    public static RhythmManager Instance;

    private float beatInterval = 2.0f;
    public float BeatInterval => beatInterval;

    public event System.Action OnPulse;

    public void Pulse()
    {
        HapticsManager.TriggerPulse();
        OnPulse?.Invoke(); // notify listeners like RhythmPulse
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetBeatInterval(float interval)
    {
        beatInterval = interval;
    }
}