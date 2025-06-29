using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int currentScore = 0;
    public int currentLevel = 1;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        UIManager.Instance.UpdateScore(currentScore);
    }

    public void AdvanceLevel()
    {
        currentLevel++;
        ProceduralLevelGenerator.Instance.GenerateLevel(currentLevel);
    }
}