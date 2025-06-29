using UnityEngine;

public class RhythmMiniGame : MonoBehaviour
{
    private Plant currentPlant;
    private int successClicks = 0;
    private int requiredClicks = 0;

    public UIManager uiManager;

    public void StartMiniGame(Plant plant, int difficulty)
    {
        currentPlant = plant;
        successClicks = 0;
        requiredClicks = difficulty;

        uiManager.ShowRhythmMiniGame();
        if (currentPlant.waterEffect != null)
            currentPlant.waterEffect.Play();
    }

    public void OnMiniGameClick()
    {
        successClicks++;
        uiManager.AnimateScale(uiManager.rhythmButton.transform);

        HapticFeedback.VibrateLight();

        if (successClicks >= requiredClicks)
        {
            OnMiniGameSuccess();
        }
    }

    private void OnMiniGameSuccess()
    {
        if (currentPlant.waterEffect != null)
            currentPlant.waterEffect.Stop();

        currentPlant.Grow();
        uiManager.HideRhythmMiniGame();
    }
}