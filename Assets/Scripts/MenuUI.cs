using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SyncSeed
{
    /// <summary>
    /// Manages menu UI interactions and navigation.
    /// Handles start menu, player name input, and level progression buttons.
    /// </summary>
    public class MenuUI : MonoBehaviour
    {
        #region Events
        
        [Header("Menu Events")]
        public UnityEvent<string> OnPlayerNameSubmitted;
        public UnityEvent OnStartMenuShown;
        public UnityEvent OnLeaderboardShown;
        public UnityEvent OnNextLevelRequested;
        public UnityEvent OnRestartLevelRequested;
        
        #endregion

        #region Serialized Fields
        
        [Header("Menu Panels")]
        [SerializeField] private GameObject _startPanel;
        [SerializeField] private GameObject _leaderboardPanel;
        
        [Header("Input Fields")]
        [SerializeField] private TMP_InputField _displayNameInput;
        
        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _restartLevelButton;
        
        [Header("Configuration")]
        [SerializeField] private int _minPlayerNameLength = 1;
        [SerializeField] private int _maxPlayerNameLength = 20;
        [SerializeField] private string _defaultPlayerName = "Player";
        
        #endregion

        #region Properties
        
        public GameObject StartPanel 
        { 
            get => _startPanel; 
            set => _startPanel = value; 
        }
        
        public GameObject LeaderboardPanel 
        { 
            get => _leaderboardPanel; 
            set => _leaderboardPanel = value; 
        }
        
        public TMP_InputField DisplayNameInput 
        { 
            get => _displayNameInput; 
            set => _displayNameInput = value; 
        }
        
        public string CurrentPlayerName => _displayNameInput?.text ?? string.Empty;
        
        public bool IsStartMenuVisible => _startPanel != null && _startPanel.activeInHierarchy;
        public bool IsLeaderboardVisible => _leaderboardPanel != null && _leaderboardPanel.activeInHierarchy;
        
        #endregion

        #region Unity Lifecycle
        
        private void Start()
        {
            InitializeMenuUI();
            SetupButtonListeners();
            ShowStartMenu();
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Shows the start menu and hides other panels.
        /// </summary>
        public void ShowStartMenu()
        {
            SetPanelVisibility(_startPanel, true);
            SetPanelVisibility(_leaderboardPanel, false);
            OnStartMenuShown?.Invoke();
        }
        
        /// <summary>
        /// Shows the leaderboard panel and hides other panels.
        /// </summary>
        public void ShowLeaderboard()
        {
            SetPanelVisibility(_startPanel, false);
            SetPanelVisibility(_leaderboardPanel, true);
            OnLeaderboardShown?.Invoke();
        }
        
        /// <summary>
        /// Handles the play button click event.
        /// </summary>
        public void OnPlayButtonClicked()
        {
            string playerName = GetValidatedPlayerName();
            if (string.IsNullOrEmpty(playerName))
            {
                Debug.LogWarning("Invalid player name. Cannot start game.");
                return;
            }
            
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.SetPlayerName(playerName))
                {
                    SetPanelVisibility(_startPanel, false);
                    OnPlayerNameSubmitted?.Invoke(playerName);
                    GameManager.Instance.StartLevel();
                }
                else
                {
                    Debug.LogWarning("Failed to set player name. Cannot start game.");
                }
            }
            else
            {
                Debug.LogError("GameManager instance not found. Cannot start game.");
            }
        }
        
        /// <summary>
        /// Handles the next level button click event.
        /// </summary>
        public void OnNextLevelButtonClicked()
        {
            SetPanelVisibility(_leaderboardPanel, false);
            OnNextLevelRequested?.Invoke();
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AdvanceToNextLevel();
            }
            else
            {
                Debug.LogError("GameManager instance not found. Cannot advance to next level.");
            }
        }
        
        /// <summary>
        /// Handles the restart level button click event.
        /// </summary>
        public void OnRestartLevelButtonClicked()
        {
            SetPanelVisibility(_leaderboardPanel, false);
            OnRestartLevelRequested?.Invoke();
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartLevel();
            }
            else
            {
                Debug.LogError("GameManager instance not found. Cannot restart level.");
            }
        }
        
        /// <summary>
        /// Sets the player name input field value.
        /// </summary>
        /// <param name="playerName">The player name to set</param>
        public void SetPlayerName(string playerName)
        {
            if (_displayNameInput != null)
            {
                _displayNameInput.text = playerName ?? string.Empty;
            }
        }
        
        /// <summary>
        /// Clears the player name input field.
        /// </summary>
        public void ClearPlayerName()
        {
            if (_displayNameInput != null)
            {
                _displayNameInput.text = string.Empty;
            }
        }
        
        /// <summary>
        /// Sets the minimum player name length requirement.
        /// </summary>
        /// <param name="minLength">Minimum length required</param>
        public void SetMinPlayerNameLength(int minLength)
        {
            _minPlayerNameLength = Mathf.Max(1, minLength);
        }
        
        /// <summary>
        /// Sets the maximum player name length limit.
        /// </summary>
        /// <param name="maxLength">Maximum length allowed</param>
        public void SetMaxPlayerNameLength(int maxLength)
        {
            _maxPlayerNameLength = Mathf.Max(_minPlayerNameLength, maxLength);
        }
        
        #endregion

        #region Private Methods
        
        /// <summary>
        /// Initializes the menu UI with default values and validation.
        /// </summary>
        private void InitializeMenuUI()
        {
            // Validate configuration
            if (_minPlayerNameLength < 1)
            {
                Debug.LogWarning("Minimum player name length must be at least 1. Setting to 1.");
                _minPlayerNameLength = 1;
            }
            
            if (_maxPlayerNameLength < _minPlayerNameLength)
            {
                Debug.LogWarning("Maximum player name length must be greater than minimum. Adjusting.");
                _maxPlayerNameLength = _minPlayerNameLength + 10;
            }
            
            // Set default player name if input field is empty
            if (_displayNameInput != null && string.IsNullOrEmpty(_displayNameInput.text))
            {
                _displayNameInput.text = _defaultPlayerName;
            }
            
            Debug.Log("MenuUI initialized successfully.");
        }
        
        /// <summary>
        /// Sets up button click listeners.
        /// </summary>
        private void SetupButtonListeners()
        {
            if (_playButton != null)
            {
                _playButton.onClick.RemoveAllListeners();
                _playButton.onClick.AddListener(OnPlayButtonClicked);
            }
            else
            {
                Debug.LogError("Play button reference is missing in MenuUI.");
            }
            
            if (_nextLevelButton != null)
            {
                _nextLevelButton.onClick.RemoveAllListeners();
                _nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
            }
            else
            {
                Debug.LogError("Next level button reference is missing in MenuUI.");
            }
            
            if (_restartLevelButton != null)
            {
                _restartLevelButton.onClick.RemoveAllListeners();
                _restartLevelButton.onClick.AddListener(OnRestartLevelButtonClicked);
            }
            else
            {
                Debug.LogError("Restart level button reference is missing in MenuUI.");
            }
        }
        
        /// <summary>
        /// Sets the visibility of a panel GameObject.
        /// </summary>
        /// <param name="panel">The panel to set visibility for</param>
        /// <param name="visible">Whether the panel should be visible</param>
        private void SetPanelVisibility(GameObject panel, bool visible)
        {
            if (panel != null)
            {
                panel.SetActive(visible);
            }
            else
            {
                Debug.LogWarning($"Panel reference is null. Cannot set visibility to {visible}.");
            }
        }
        
        /// <summary>
        /// Gets and validates the player name from the input field.
        /// </summary>
        /// <returns>Validated player name or empty string if invalid</returns>
        private string GetValidatedPlayerName()
        {
            if (_displayNameInput == null)
            {
                Debug.LogWarning("Display name input field is null.");
                return string.Empty;
            }
            
            string playerName = _displayNameInput.text?.Trim() ?? string.Empty;
            
            if (string.IsNullOrEmpty(playerName))
            {
                Debug.LogWarning("Player name cannot be empty.");
                return string.Empty;
            }
            
            if (playerName.Length < _minPlayerNameLength)
            {
                Debug.LogWarning($"Player name must be at least {_minPlayerNameLength} characters long.");
                return string.Empty;
            }
            
            if (playerName.Length > _maxPlayerNameLength)
            {
                Debug.LogWarning($"Player name cannot exceed {_maxPlayerNameLength} characters.");
                return string.Empty;
            }
            
            return playerName;
        }
        
        #endregion

        #region Validation
        
        private void OnValidate()
        {
            // Ensure minimum length is at least 1
            _minPlayerNameLength = Mathf.Max(1, _minPlayerNameLength);
            
            // Ensure maximum length is greater than minimum
            _maxPlayerNameLength = Mathf.Max(_minPlayerNameLength, _maxPlayerNameLength);
            
            // Ensure default player name is not null
            if (string.IsNullOrEmpty(_defaultPlayerName))
            {
                _defaultPlayerName = "Player";
            }
        }
        
        #endregion
    }
}