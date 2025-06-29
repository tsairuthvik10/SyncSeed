using UnityEngine;

public static class HapticsManager
{
    public static void TriggerPulse()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Handheld.Vibrate();
#endif
    }
}