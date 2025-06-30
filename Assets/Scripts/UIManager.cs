using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject leaderboardContainer;
    public TextMeshProUGUI scoreText;

    void Start()
    {
        if (leaderboardContainer != null)
            leaderboardContainer.SetActive(false);

        UpdateScore(0); // Initialize score display
    }

    public void UpdateScore(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = $"{newScore}";
        }
    }

    public void ShowLeaderboard()
    {
        if (leaderboardContainer != null)
        {
            leaderboardContainer.SetActive(true);
            // You could trigger an animation here if needed
            // e.g. Animator.SetTrigger("Show")
        }
    }

    public void ToggleLeaderboard()
    {
        if (leaderboardContainer != null)
        {
            leaderboardContainer.SetActive(!leaderboardContainer.activeSelf);
        }
    }
}
