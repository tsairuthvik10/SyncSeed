using UnityEngine;

public class Plant : MonoBehaviour
{
    public PlantData plantData;  // Assigned during spawn

    private Vector3 initialPosition;
    private bool isGrown = false;
    private GameManager gameManager;
    private RhythmMiniGame rhythmMiniGame;

    public AudioSource plantAudio;
    public ParticleSystem growEffect;
    public ParticleSystem waterEffect;
    public Renderer plantRenderer;

    public void Initialize(GameManager manager, PlantData data)
    {
        gameManager = manager;
        plantData = data;

        initialPosition = transform.position;
        rhythmMiniGame = FindObjectOfType<RhythmMiniGame>();

        plantAudio.pitch = plantData.soundPitch;
        plantRenderer.material.color = plantData.plantColor;

        if (waterEffect != null)
            waterEffect.Stop();
    }

    void Update()
    {
        if (!isGrown && Vector3.Distance(Camera.main.transform.position, initialPosition) > plantData.requiredWalkDistance)
        {
            if (Vector3.Distance(Camera.main.transform.position, initialPosition) < 0.5f)
            {
                rhythmMiniGame.StartMiniGame(this, plantData.rhythmDifficulty);
                isGrown = true;
            }
        }
    }

    public void Grow()
    {
        transform.localScale *= plantData.growthMultiplier;
        if (growEffect != null)
            growEffect.Play();
        if (plantAudio != null)
            plantAudio.Play();
        HapticFeedback.VibrateSuccess();
    }

    public string GenerateEchoCode()
    {
        return EchoCodeManager.GenerateCode(transform.position, plantData.plantID, plantAudio.pitch);
    }
}