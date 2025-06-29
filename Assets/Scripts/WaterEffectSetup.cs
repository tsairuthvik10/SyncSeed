using UnityEngine;

public class WaterEffectSetup : MonoBehaviour
{
    void Start()
    {
        var ps = gameObject.AddComponent<ParticleSystem>();
        var main = ps.main;
        var emission = ps.emission;
        var shape = ps.shape;
        var colorOverLifetime = ps.colorOverLifetime;
        var sizeOverLifetime = ps.sizeOverLifetime;
        var renderer = ps.GetComponent<ParticleSystemRenderer>();

        // Main settings
        main.duration = 2f;
        main.loop = true;
        main.startLifetime = 0.8f;
        main.startSpeed = 0.2f;
        main.startSize = 0.08f;
        main.startColor = new Color(0.6f, 0.8f, 1f, 0.6f);
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        // Emission
        emission.rateOverTime = 40f;

        // Shape
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.1f;
        shape.rotation = new Vector3(-90, 0, 0); // Emit downwards

        // Color over lifetime: fade out
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(0.6f,0.8f,1f,0.6f), 0f), new GradientColorKey(new Color(0.6f,0.8f,1f,0f), 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.6f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = grad;

        // Size over lifetime: shrink
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0f, 1f);
        curve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

        // Renderer: soft particle sprite
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.SetColor("_Color", new Color(0.6f, 0.8f, 1f, 0.6f));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        ps.Stop();
    }
}