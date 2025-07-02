using UnityEngine;
using TMPro;
using UnityEngine.Events;

namespace SyncSeed
{
    /// <summary>
    /// Manages UI elements and interactions for the game interface.
    /// Implements singleton pattern with proper lifecycle management.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region Singleton Implementation
        
        public static UIManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeUIManager();
            }
            else if (Instance != this)
            {
                Debug.LogWarning("Multiple UIManager instances detected. Destroying duplicate.");
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
        
        [Header("UI Events")]
        public UnityEvent<int> OnScoreUpdated;
        public UnityEvent OnLeaderboardShown;
        public UnityEvent OnLeaderboardHidden;
        
        #endregion

        #region Serialized Fields
        
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private GameObject _leaderboardPanel;
        [SerializeField] private LeaderboardManager _leaderboardManager;
        
        [Header("UI Configuration")]
        [SerializeField] private string _scoreFormat = "Score: {0}";
        [SerializeField] private bool _autoUpdateScore = true;
        
        #endregion

        #region Properties
        
        public TextMeshProUGUI ScoreText 
        { 
            get => _scoreText; 
            set => _scoreText = value; 
        }
        
        public GameObject LeaderboardPanel 
        { 
            get => _leaderboardPanel; 
            set => _leaderboardPanel = value; 
        }
        
        public bool IsLeaderboardVisible => _leaderboardPanel != null && _leaderboardPanel.activeInHierarchy;
        
        #endregion

        #region Unity Lifecycle
        
        private void Start()
        {
            if (Instance == this)
            {
                ValidateReferences();
            }
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Updates the score display text.
        /// </summary>
        /// <param name="score">The score to display</param>
        public void UpdateScore(int score)
        {
            if (_scoreText == null)
            {
                Debug.LogWarning("Score text reference is null. Cannot update score display.");
                return;
            }
            
            _scoreText.text = string.Format(_scoreFormat, score);
            OnScoreUpdated?.Invoke(score);
        }
        
        /// <summary>
        /// Shows the leaderboard panel and fetches leaderboard data.
        /// </summary>
        public void ShowLeaderboard()
        {
            if (_leaderboardPanel == null)
            {
                Debug.LogWarning("Leaderboard panel reference is null. Cannot show leaderboard.");
                return;
            }
            
            _leaderboardPanel.SetActive(true);
            OnLeaderboardShown?.Invoke();
            
            if (_leaderboardManager != null)
            {
                _leaderboardManager.FetchLeaderboard();
            }
            else
            {
                Debug.LogWarning("LeaderboardManager reference is null. Cannot fetch leaderboard data.");
            }
        }
        
        /// <summary>
        /// Hides the leaderboard panel.
        /// </summary>
        public void HideLeaderboard()
        {
            if (_leaderboardPanel == null)
            {
                Debug.LogWarning("Leaderboard panel reference is null. Cannot hide leaderboard.");
                return;
            }
            
            _leaderboardPanel.SetActive(false);
            OnLeaderboardHidden?.Invoke();
        }
        
        /// <summary>
        /// Toggles the leaderboard panel visibility.
        /// </summary>
        public void ToggleLeaderboard()
        {
            if (IsLeaderboardVisible)
            {
                HideLeaderboard();
            }
            else
            {
                ShowLeaderboard();
            }
        }
        
        /// <summary>
        /// Sets the score format string.
        /// </summary>
        /// <param name="format">The format string (use {0} for score placeholder)</param>
        public void SetScoreFormat(string format)
        {
            if (string.IsNullOrEmpty(format))
            {
                Debug.LogWarning("Score format cannot be null or empty.");
                return;
            }
            
            _scoreFormat = format;
        }
        
        /// <summary>
        /// Enables or disables automatic score updates.
        /// </summary>
        /// <param name="enabled">Whether to enable automatic score updates</param>
        public void SetAutoUpdateScore(bool enabled)
        {
            _autoUpdateScore = enabled;
        }
        
        #endregion

        #region Private Methods
        
        /// <summary>
        /// Initializes the UI manager with default values.
        /// </summary>
        private void InitializeUIManager()
        {
            if (string.IsNullOrEmpty(_scoreFormat))
            {
                _scoreFormat = "Score: {0}";
            }
            
            Debug.Log("UIManager initialized successfully.");
        }
        
        /// <summary>
        /// Validates that all required references are set.
        /// </summary>
        private void ValidateReferences()
        {
            if (_scoreText == null)
            {
                Debug.LogError("ScoreText reference is missing in UIManager.");
            }
            
            if (_leaderboardPanel == null)
            {
                Debug.LogError("LeaderboardPanel reference is missing in UIManager.");
            }
            
            if (_leaderboardManager == null)
            {
                Debug.LogWarning("LeaderboardManager reference is missing in UIManager. Leaderboard functionality will be limited.");
            }
        }
        
        #endregion

        #region Validation
        
        private void OnValidate()
        {
            // Ensure score format is not null
            if (string.IsNullOrEmpty(_scoreFormat))
            {
                _scoreFormat = "Score: {0}";
            }
        }
        
        #endregion
    }
}