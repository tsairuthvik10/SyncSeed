// RhythmPulse.cs (attach to PlayerRhythmPulse GameObject)
using UnityEngine;

public class RhythmPulse : MonoBehaviour
{
    public float pulseRadius = 2f;
    public float pulseDuration = 0.2f;

    private SphereCollider sphereCollider;

    private void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.enabled = false;

        RhythmManager.Instance.OnPulse += TriggerPulse;
    }

    void TriggerPulse()
    {
        StartCoroutine(PulseRoutine());
    }

    System.Collections.IEnumerator PulseRoutine()
    {
        sphereCollider.radius = pulseRadius;
        sphereCollider.enabled = true;
        yield return new WaitForSeconds(pulseDuration);
        sphereCollider.enabled = false;
    }
}

