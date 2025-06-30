using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    public GameObject startPanel;
    public TMP_InputField displayNameInput;
    public Button playButton;

    public GameObject leaderboardPanel;
    public Button nextLevelButton;
    public Button restartLevelButton;

    private void Start()
    {
        FindObjectOfType<MenuUI>().ShowStartMenu();
        playButton.onClick.AddListener(() => {
            GameManager.Instance.SetPlayerName(displayNameInput.text);
            startPanel.SetActive(false);
            GameManager.Instance.StartLevel();
        });

        nextLevelButton.onClick.AddListener(() => {
            leaderboardPanel.SetActive(false);
            GameManager.Instance.AdvanceToNextLevel();
        });

        restartLevelButton.onClick.AddListener(() => {
            leaderboardPanel.SetActive(false);
            GameManager.Instance.RestartLevel();
        });
        
    }

    public void ShowStartMenu()
    {
        startPanel.SetActive(true);
        leaderboardPanel.SetActive(false);
    }

    public void ShowLeaderboard()
    {
        leaderboardPanel.SetActive(true);
    }
}