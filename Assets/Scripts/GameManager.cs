using UnityEngine;
using UnityEngine.Events;

namespace SyncSeed
{
    /// <summary>
    /// Central game controller managing game state, scoring, and level progression.
    /// Implements singleton pattern with proper lifecycle management.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Singleton Implementation
        
        public static GameManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGameManager();
            }
            else if (Instance != this)
            {
                Debug.LogWarning("Multiple GameManager instances detected. Destroying duplicate.");
                Destroy(gameObject);
            }
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        #endregion

        #region Events
        
        [Header("Game Events")]
        public UnityEvent<int> OnScoreChanged;
        public UnityEvent<int> OnLevelChanged;
        public UnityEvent<int> OnTargetsRemainingChanged;
        public UnityEvent OnLevelCompleted;
        public UnityEvent OnGameStarted;
        public UnityEvent<string> OnPlayerNameChanged;
        
        #endregion

        #region Serialized Fields
        
        [Header("Game Configuration")]
        [SerializeField] private int _currentLevel = 1;
        [SerializeField] private int _playerScore = 0;
        [SerializeField] private int _pointsPerTarget = 10;
        [SerializeField] private int _targetsRemaining = 0;
        [SerializeField] private string _playerName = string.Empty;
        
        [Header("Level Configuration")]
        [SerializeField] private int _minTargetsPerLevel = 3;
        [SerializeField] private int _maxTargetsPerLevel = 30;
        [SerializeField] private int _targetsIncrementPerLevel = 2;
        
        [Header("Dependencies")]
        [SerializeField] private UIManager _uiManager;
        [SerializeField] private MenuUI _menuUI;
        [SerializeField] private LeaderboardManager _leaderboardManager;
        
        #endregion

        #region Properties
        
        public int CurrentLevel 
        { 
            get => _currentLevel; 
            private set 
            {
                if (_currentLevel != value)
                {
                    _currentLevel = Mathf.Max(1, value);
                    OnLevelChanged?.Invoke(_currentLevel);
                }
            }
        }
        
        public int PlayerScore 
        { 
            get => _playerScore; 
            private set 
            {
                if (_playerScore != value)
                {
                    _playerScore = Mathf.Max(0, value);
                    OnScoreChanged?.Invoke(_playerScore);
                    _uiManager?.UpdateScore(_playerScore);
                }
            }
        }
        
        public int PointsPerTarget 
        { 
            get => _pointsPerTarget; 
            set => _pointsPerTarget = Mathf.Max(1, value);
        }
        
        public int TargetsRemaining 
        { 
            get => _targetsRemaining; 
            private set 
            {
                if (_targetsRemaining != value)
                {
                    _targetsRemaining = Mathf.Max(0, value);
                    OnTargetsRemainingChanged?.Invoke(_targetsRemaining);
                }
            }
        }
        
        public string PlayerName 
        { 
            get => _playerName; 
            private set 
            {
                if (_playerName != value)
                {
                    _playerName = value ?? string.Empty;
                    OnPlayerNameChanged?.Invoke(_playerName);
                }
            }
        }
        
        public bool IsLevelComplete => TargetsRemaining <= 0;
        public bool IsGameActive => !string.IsNullOrEmpty(PlayerName);
        
        #endregion

        #region Unity Lifecycle
        
        private void Start()
        {
            if (Instance == this)
            {
                ShowStartMenu();
            }
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Sets the player name and validates input.
        /// </summary>
        /// <param name="name">Player name to set</param>
        /// <returns>True if name was set successfully, false otherwise</returns>
        public bool SetPlayerName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Debug.LogWarning("Player name cannot be empty or whitespace.");
                return false;
            }
            
            PlayerName = name.Trim();
            Debug.Log($"Player name set to: {PlayerName}");
            return true;
        }
        
        /// <summary>
        /// Starts a new level, resetting score and generating targets.
        /// </summary>
        public void StartLevel()
        {
            if (!IsGameActive)
            {
                Debug.LogWarning("Cannot start level without setting player name.");
                return;
            }
            
            PlayerScore = 0;
            TargetsRemaining = GenerateTargetsForLevel(CurrentLevel);
            
            Debug.Log($"Level {CurrentLevel} started with {TargetsRemaining} targets.");
            OnGameStarted?.Invoke();
        }
        
        /// <summary>
        /// Called when player hits a rhythm target. Updates score and checks level completion.
        /// </summary>
        public void RhythmTargetHit()
        {
            if (!IsGameActive || TargetsRemaining <= 0)
            {
                Debug.LogWarning("Cannot hit target: game not active or level already complete.");
                return;
            }
            
            PlayerScore += PointsPerTarget;
            TargetsRemaining--;
            
            if (IsLevelComplete)
            {
                EndLevel();
            }
        }
        
        /// <summary>
        /// Adds additional score points (for bonuses, combos, etc.).
        /// </summary>
        /// <param name="amount">Amount of points to add</param>
        public void AddScore(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"Cannot add non-positive score: {amount}");
                return;
            }
            
            PlayerScore += amount;
        }
        
        /// <summary>
        /// Advances to the next level and starts it.
        /// </summary>
        public void AdvanceToNextLevel()
        {
            CurrentLevel++;
            StartLevel();
        }
        
        /// <summary>
        /// Restarts the current level.
        /// </summary>
        public void RestartLevel()
        {
            StartLevel();
        }
        
        /// <summary>
        /// Ends the current level and shows results.
        /// </summary>
        public void EndLevel()
        {
            if (!IsLevelComplete)
            {
                Debug.LogWarning("Cannot end level: targets still remaining.");
                return;
            }
            
            Debug.Log($"Level {CurrentLevel} ended. Final Score: {PlayerScore}");
            
            // Submit score to leaderboard if player name is set
            if (!string.IsNullOrEmpty(PlayerName))
            {
                _leaderboardManager?.SubmitScore(PlayerName, PlayerScore);
            }
            
            // Show leaderboard
            _uiManager?.ShowLeaderboard();
            _menuUI?.ShowLeaderboard();
            
            OnLevelCompleted?.Invoke();
        }
        
        /// <summary>
        /// Shows the start menu.
        /// </summary>
        public void ShowStartMenu()
        {
            _menuUI?.ShowStartMenu();
        }
        
        /// <summary>
        /// Resets the game to initial state.
        /// </summary>
        public void ResetGame()
        {
            CurrentLevel = 1;
            PlayerScore = 0;
            TargetsRemaining = 0;
            PlayerName = string.Empty;
            ShowStartMenu();
        }
        
        #endregion

        #region Private Methods
        
        /// <summary>
        /// Initializes the game manager with default values.
        /// </summary>
        private void InitializeGameManager()
        {
            // Validate configuration
            if (_minTargetsPerLevel < 1)
            {
                Debug.LogError("Minimum targets per level must be at least 1.");
                _minTargetsPerLevel = 1;
            }
            
            if (_maxTargetsPerLevel < _minTargetsPerLevel)
            {
                Debug.LogError("Maximum targets per level must be greater than minimum.");
                _maxTargetsPerLevel = _minTargetsPerLevel + 10;
            }
            
            if (_targetsIncrementPerLevel < 0)
            {
                Debug.LogError("Targets increment per level must be non-negative.");
                _targetsIncrementPerLevel = 0;
            }
            
            Debug.Log("GameManager initialized successfully.");
        }
        
        /// <summary>
        /// Generates the number of targets for a given level.
        /// </summary>
        /// <param name="level">Level number</param>
        /// <returns>Number of targets for the level</returns>
        private int GenerateTargetsForLevel(int level)
        {
            int targets = _minTargetsPerLevel + (level - 1) * _targetsIncrementPerLevel;
            return Mathf.Clamp(targets, _minTargetsPerLevel, _maxTargetsPerLevel);
        }
        
        #endregion

        #region Validation
        
        private void OnValidate()
        {
            // Ensure values are valid in the inspector
            _currentLevel = Mathf.Max(1, _currentLevel);
            _playerScore = Mathf.Max(0, _playerScore);
            _pointsPerTarget = Mathf.Max(1, _pointsPerTarget);
            _targetsRemaining = Mathf.Max(0, _targetsRemaining);
            _minTargetsPerLevel = Mathf.Max(1, _minTargetsPerLevel);
            _maxTargetsPerLevel = Mathf.Max(_minTargetsPerLevel, _maxTargetsPerLevel);
            _targetsIncrementPerLevel = Mathf.Max(0, _targetsIncrementPerLevel);
        }
        
        #endregion
    }
}