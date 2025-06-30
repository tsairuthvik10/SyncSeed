using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public UIManager uiManager;
    public int currentLevel = 1;
    public int playerScore = 0;
    public int pointsPerTarget = 10;
    public int targetsRemaining;
    public string playerName;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        FindObjectOfType<MenuUI>().ShowStartMenu();
    }

    public void SetPlayerName(string name)
    {
        playerName = name;
        Debug.Log($"Player name set to: {playerName}");
    }

    public void StartLevel()
    {
        playerScore = 0;
        targetsRemaining = GenerateTargetsForLevel(currentLevel);
        Debug.Log($"Level {currentLevel} started with {targetsRemaining} targets.");
        uiManager.UpdateScore(playerScore);
    }

    public void RhythmTargetHit()
    {
        playerScore += pointsPerTarget;
        targetsRemaining--;
        uiManager.UpdateScore(playerScore);

        if (targetsRemaining <= 0)
        {
            EndLevel();
        }
    }

    public void EndLevel()
    {
        Debug.Log($"Level {currentLevel} ended. Final Score: {playerScore}");

        if (uiManager != null)
            uiManager.ShowLeaderboard();

        FindObjectOfType<MenuUI>().ShowLeaderboard();
    }

    public void AdvanceToNextLevel()
    {
        currentLevel++;
        StartLevel();
    }

    public void RestartLevel()
    {
        StartLevel();
    }

    public void AddScore(int amount)
    {
        playerScore += amount;
        uiManager.UpdateScore(playerScore);
    }

    private int GenerateTargetsForLevel(int level)
    {
        return Mathf.Clamp(3 + level * 2, 3, 30);
    }
}
