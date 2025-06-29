using UnityEngine;

public static class AnimationController
{
    public static void Ping(Transform target)
    {
        LeanTween.scale(target.gameObject, Vector3.one * 1.2f, 0.1f).setLoopPingPong(1);
    }
}