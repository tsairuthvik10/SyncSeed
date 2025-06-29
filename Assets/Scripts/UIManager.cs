using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject rhythmMiniGamePanel;
    public Button rhythmButton;

    void Start()
    {
        rhythmMiniGamePanel.SetActive(false);
    }

    public void ShowRhythmMiniGame()
    {
        rhythmMiniGamePanel.SetActive(true);
        AnimateScale(rhythmMiniGamePanel.transform);
    }

    public void HideRhythmMiniGame()
    {
        rhythmMiniGamePanel.SetActive(false);
    }

    public void AnimateScale(Transform target)
    {
        LeanTween.scale(target.gameObject, Vector3.one * 1.2f, 0.3f).setEasePunch();
    }
}