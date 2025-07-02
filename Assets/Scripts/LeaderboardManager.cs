using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SyncSeed
{
    /// <summary>
    /// Manages leaderboard functionality including score submission and retrieval.
    /// Handles Firebase Firestore integration for persistent leaderboard data.
    /// </summary>
    public class LeaderboardManager : MonoBehaviour
    {
        #region Singleton Implementation
        
        public static LeaderboardManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeLeaderboardManager();
            }
            else if (Instance != this)
            {
                Debug.LogWarning("Multiple LeaderboardManager instances detected. Destroying duplicate.");
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
        
        [Header("Leaderboard Events")]
        public UnityEvent<List<LeaderboardEntry>> OnLeaderboardFetched;
        public UnityEvent<string, int> OnScoreSubmitted;
        public UnityEvent<string> OnLeaderboardError;
        
        #endregion

        #region Serialized Fields
        
        [Header("UI References")]
        [SerializeField] private Transform _leaderboardContainer;
        [SerializeField] private GameObject _leaderboardEntryPrefab;
        
        [Header("Configuration")]
        [SerializeField] private int _maxLeaderboardEntries = 10;
        [SerializeField] private string _collectionName = "leaderboard";
        [SerializeField] private bool _autoFetchOnStart = true;
        
        #endregion

        #region Properties
        
        public Transform LeaderboardContainer 
        { 
            get => _leaderboardContainer; 
            set => _leaderboardContainer = value; 
        }
        
        public GameObject LeaderboardEntryPrefab 
        { 
            get => _leaderboardEntryPrefab; 
            set => _leaderboardEntryPrefab = value; 
        }
        
        public int MaxLeaderboardEntries 
        { 
            get => _maxLeaderboardEntries; 
            set => _maxLeaderboardEntries = Mathf.Max(1, value); 
        }
        
        public bool IsConnected => _db != null;
        
        #endregion

        #region Private Fields
        
        private FirebaseFirestore _db;
        private List<LeaderboardEntry> _currentLeaderboard = new List<LeaderboardEntry>();
        private bool _isInitialized = false;
        
        #endregion

        #region Unity Lifecycle
        
        private void Start()
        {
            if (Instance == this && _autoFetchOnStart)
            {
                FetchLeaderboard();
            }
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Submits a player's score to the leaderboard.
        /// </summary>
        /// <param name="playerName">The player's name</param>
        /// <param name="score">The player's score</param>
        public void SubmitScore(string playerName, int score)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("LeaderboardManager not initialized. Cannot submit score.");
                OnLeaderboardError?.Invoke("LeaderboardManager not initialized");
                return;
            }
            
            if (string.IsNullOrWhiteSpace(playerName))
            {
                Debug.LogWarning("Player name cannot be empty. Cannot submit score.");
                OnLeaderboardError?.Invoke("Player name cannot be empty");
                return;
            }
            
            if (score < 0)
            {
                Debug.LogWarning("Score cannot be negative. Cannot submit score.");
                OnLeaderboardError?.Invoke("Score cannot be negative");
                return;
            }
            
            var entry = new Dictionary<string, object>
            {
                {"name", playerName.Trim()},
                {"score", score},
                {"timestamp", Timestamp.GetCurrentTimestamp()}
            };
            
            _db.Collection(_collectionName).AddAsync(entry).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    Debug.Log($"Score submitted to leaderboard: {playerName} - {score}");
                    OnScoreSubmitted?.Invoke(playerName, score);
                }
                else
                {
                    string errorMessage = task.Exception?.Message ?? "Unknown error occurred";
                    Debug.LogError($"Failed to submit score to leaderboard: {errorMessage}");
                    OnLeaderboardError?.Invoke($"Failed to submit score: {errorMessage}");
                }
            });
        }
        
        /// <summary>
        /// Fetches the leaderboard data from Firebase.
        /// </summary>
        public void FetchLeaderboard()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("LeaderboardManager not initialized. Cannot fetch leaderboard.");
                OnLeaderboardError?.Invoke("LeaderboardManager not initialized");
                return;
            }
            
            _db.Collection(_collectionName)
              .OrderBy("score")
              .Limit(_maxLeaderboardEntries)
              .GetSnapshotAsync().ContinueWithOnMainThread(task =>
              {
                  if (task.IsCompleted && !task.IsFaulted)
                  {
                      ProcessLeaderboardData(task.Result);
                  }
                  else
                  {
                      string errorMessage = task.Exception?.Message ?? "Unknown error occurred";
                      Debug.LogError($"Failed to fetch leaderboard: {errorMessage}");
                      OnLeaderboardError?.Invoke($"Failed to fetch leaderboard: {errorMessage}");
                  }
              });
        }
        
        /// <summary>
        /// Clears all leaderboard entries from the UI.
        /// </summary>
        public void ClearLeaderboardUI()
        {
            if (_leaderboardContainer == null)
            {
                Debug.LogWarning("Leaderboard container reference is null. Cannot clear UI.");
                return;
            }
            
            foreach (Transform child in _leaderboardContainer)
            {
                Destroy(child.gameObject);
            }
        }
        
        /// <summary>
        /// Gets the current leaderboard data.
        /// </summary>
        /// <returns>List of leaderboard entries</returns>
        public List<LeaderboardEntry> GetCurrentLeaderboard()
        {
            return new List<LeaderboardEntry>(_currentLeaderboard);
        }
        
        /// <summary>
        /// Sets the maximum number of leaderboard entries to display.
        /// </summary>
        /// <param name="maxEntries">Maximum number of entries</param>
        public void SetMaxLeaderboardEntries(int maxEntries)
        {
            MaxLeaderboardEntries = maxEntries;
        }
        
        /// <summary>
        /// Sets the Firebase collection name for the leaderboard.
        /// </summary>
        /// <param name="collectionName">The collection name</param>
        public void SetCollectionName(string collectionName)
        {
            if (string.IsNullOrWhiteSpace(collectionName))
            {
                Debug.LogWarning("Collection name cannot be empty.");
                return;
            }
            
            _collectionName = collectionName.Trim();
        }
        
        #endregion

        #region Private Methods
        
        /// <summary>
        /// Initializes the leaderboard manager with Firebase connection.
        /// </summary>
        private void InitializeLeaderboardManager()
        {
            try
            {
                _db = FirebaseFirestore.DefaultInstance;
                _isInitialized = true;
                Debug.Log("LeaderboardManager initialized successfully with Firebase connection.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize LeaderboardManager: {e.Message}");
                _isInitialized = false;
            }
        }
        
        /// <summary>
        /// Processes the fetched leaderboard data and updates the UI.
        /// </summary>
        /// <param name="snapshot">The Firestore snapshot containing leaderboard data</param>
        private void ProcessLeaderboardData(QuerySnapshot snapshot)
        {
            if (snapshot == null)
            {
                Debug.LogWarning("Received null snapshot from Firebase.");
                return;
            }
            
            _currentLeaderboard.Clear();
            ClearLeaderboardUI();
            
            List<DocumentSnapshot> documents = new List<DocumentSnapshot>(snapshot.Documents);
            documents.Reverse(); // Show highest scores first
            
            foreach (var doc in documents)
            {
                if (doc.TryGetValue("name", out string name) && doc.TryGetValue("score", out int score))
                {
                    var entry = new LeaderboardEntry
                    {
                        Name = name ?? "Unknown",
                        Score = score,
                        Timestamp = doc.TryGetValue("timestamp", out Timestamp timestamp) ? timestamp : null
                    };
                    
                    _currentLeaderboard.Add(entry);
                    CreateLeaderboardEntryUI(entry);
                }
            }
            
            OnLeaderboardFetched?.Invoke(_currentLeaderboard);
            Debug.Log($"Leaderboard fetched successfully. {_currentLeaderboard.Count} entries loaded.");
        }
        
        /// <summary>
        /// Creates a UI element for a leaderboard entry.
        /// </summary>
        /// <param name="entry">The leaderboard entry to create UI for</param>
        private void CreateLeaderboardEntryUI(LeaderboardEntry entry)
        {
            if (_leaderboardContainer == null || _leaderboardEntryPrefab == null)
            {
                Debug.LogWarning("Leaderboard container or entry prefab is null. Cannot create UI entry.");
                return;
            }
            
            GameObject entryObject = Instantiate(_leaderboardEntryPrefab, _leaderboardContainer);
            TMP_Text entryText = entryObject.GetComponentInChildren<TMP_Text>();
            
            if (entryText != null)
            {
                entryText.text = $"{entry.Name}: {entry.Score}";
            }
            else
            {
                Debug.LogWarning("Leaderboard entry prefab does not contain TMP_Text component.");
            }
        }
        
        #endregion

        #region Validation
        
        private void OnValidate()
        {
            // Ensure max entries is at least 1
            _maxLeaderboardEntries = Mathf.Max(1, _maxLeaderboardEntries);
            
            // Ensure collection name is not null
            if (string.IsNullOrEmpty(_collectionName))
            {
                _collectionName = "leaderboard";
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Represents a single leaderboard entry.
    /// </summary>
    [System.Serializable]
    public class LeaderboardEntry
    {
        public string Name;
        public int Score;
        public Timestamp Timestamp;
        
        public LeaderboardEntry()
        {
            Name = "Unknown";
            Score = 0;
            Timestamp = null;
        }
        
        public LeaderboardEntry(string name, int score, Timestamp timestamp = null)
        {
            Name = name ?? "Unknown";
            Score = score;
            Timestamp = timestamp;
        }
    }
}