using UnityEngine;

public class RhythmNode : MonoBehaviour
{
    public float beatInterval = 1f;
    private float timer = 0f;

    void Update()
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
        HapticsManager.TriggerPulse();
        AnimationController.Ping(transform);
    }

    public void OnPlayerTap()
    {
        GameManager.Instance.AddScore(10);
        Destroy(gameObject);
    }
}