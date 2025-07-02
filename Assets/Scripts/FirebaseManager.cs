using Firebase;
using Firebase.Firestore;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace SyncSeed
{
    /// <summary>
    /// Manages Firebase integration and database operations.
    /// Handles Firebase initialization, authentication, and data persistence.
    /// </summary>
    public class FirebaseManager : MonoBehaviour
    {
        #region Events
        
        [Header("Firebase Events")]
        public UnityEvent OnFirebaseInitialized;
        public UnityEvent<string> OnFirebaseError;
        public UnityEvent<string> OnScoreSubmitted;
        public UnityEvent<string> OnDataRetrieved;
        
        #endregion

        #region Serialized Fields
        
        [Header("Firebase Configuration")]
        [SerializeField] private bool _autoInitialize = true;
        [SerializeField] private bool _enableDebugLogging = true;
        [SerializeField] private string _defaultCollection = "leaderboard";
        
        [Header("Authentication")]
        [SerializeField] private bool _enableAnonymousAuth = true;
        [SerializeField] private bool _autoSignIn = true;
        
        #endregion

        #region Properties
        
        public FirebaseFirestore Database { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsAuthenticated { get; private set; }
        public string UserId { get; private set; }
        public string DefaultCollection 
        { 
            get => _defaultCollection; 
            set => _defaultCollection = value ?? "leaderboard"; 
        }
        
        #endregion

        #region Private Fields
        
        private DependencyStatus _dependencyStatus = DependencyStatus.UnavailableOther;
        private bool _isInitializing = false;
        
        #endregion

        #region Unity Lifecycle
        
        private void Start()
        {
            if (_autoInitialize)
            {
                InitializeFirebase();
            }
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Initializes Firebase and checks dependencies.
        /// </summary>
        public void InitializeFirebase()
        {
            if (_isInitializing)
            {
                Debug.LogWarning("Firebase initialization already in progress.");
                return;
            }
            
            if (IsInitialized)
            {
                Debug.LogWarning("Firebase is already initialized.");
                return;
            }
            
            _isInitializing = true;
            
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
                _dependencyStatus = task.Result;
                
                if (_dependencyStatus == DependencyStatus.Available)
                {
                    OnFirebaseDependenciesResolved();
                }
                else
                {
                    string errorMessage = $"Could not resolve all Firebase dependencies: {_dependencyStatus}";
                    Debug.LogError(errorMessage);
                    OnFirebaseError?.Invoke(errorMessage);
                }
                
                _isInitializing = false;
            });
        }
        
        /// <summary>
        /// Submits a player's score to the Firebase database.
        /// </summary>
        /// <param name="playerId">The player's unique identifier</param>
        /// <param name="score">The player's score</param>
        public void SubmitScore(string playerId, int score)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("Firebase not initialized. Cannot submit score.");
                OnFirebaseError?.Invoke("Firebase not initialized");
                return;
            }
            
            if (string.IsNullOrWhiteSpace(playerId))
            {
                Debug.LogWarning("Player ID cannot be empty. Cannot submit score.");
                OnFirebaseError?.Invoke("Player ID cannot be empty");
                return;
            }
            
            if (score < 0)
            {
                Debug.LogWarning("Score cannot be negative. Cannot submit score.");
                OnFirebaseError?.Invoke("Score cannot be negative");
                return;
            }
            
            DocumentReference docRef = Database.Collection(_defaultCollection).Document(playerId);
            var scoreData = new Dictionary<string, object> {
                { "score", score },
                { "timestamp", Timestamp.GetCurrentTimestamp() },
                { "playerId", playerId }
            };
            
            docRef.SetAsync(scoreData).ContinueWithOnMainThread(task => {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    Debug.Log($"Score submitted successfully for player {playerId}: {score}");
                    OnScoreSubmitted?.Invoke(playerId);
                }
                else
                {
                    string errorMessage = task.Exception?.Message ?? "Unknown error occurred";
                    Debug.LogError($"Failed to submit score: {errorMessage}");
                    OnFirebaseError?.Invoke($"Failed to submit score: {errorMessage}");
                }
            });
        }
        
        /// <summary>
        /// Submits a player's score using the current user ID.
        /// </summary>
        /// <param name="score">The player's score</param>
        public void SubmitScore(int score)
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                Debug.LogWarning("No authenticated user. Cannot submit score.");
                OnFirebaseError?.Invoke("No authenticated user");
                return;
            }
            
            SubmitScore(UserId, score);
        }
        
        /// <summary>
        /// Retrieves a player's score from the database.
        /// </summary>
        /// <param name="playerId">The player's unique identifier</param>
        /// <param name="callback">Callback with the retrieved score data</param>
        public void GetPlayerScore(string playerId, System.Action<Dictionary<string, object>> callback)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("Firebase not initialized. Cannot retrieve score.");
                OnFirebaseError?.Invoke("Firebase not initialized");
                callback?.Invoke(null);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(playerId))
            {
                Debug.LogWarning("Player ID cannot be empty. Cannot retrieve score.");
                OnFirebaseError?.Invoke("Player ID cannot be empty");
                callback?.Invoke(null);
                return;
            }
            
            DocumentReference docRef = Database.Collection(_defaultCollection).Document(playerId);
            docRef.GetSnapshotAsync().ContinueWithOnMainThread(task => {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    DocumentSnapshot snapshot = task.Result;
                    if (snapshot.Exists)
                    {
                        Dictionary<string, object> data = new Dictionary<string, object>(snapshot.ToDictionary());
                        Debug.Log($"Retrieved score data for player {playerId}");
                        OnDataRetrieved?.Invoke(playerId);
                        callback?.Invoke(data);
                    }
                    else
                    {
                        Debug.Log($"No score data found for player {playerId}");
                        callback?.Invoke(null);
                    }
                }
                else
                {
                    string errorMessage = task.Exception?.Message ?? "Unknown error occurred";
                    Debug.LogError($"Failed to retrieve score: {errorMessage}");
                    OnFirebaseError?.Invoke($"Failed to retrieve score: {errorMessage}");
                    callback?.Invoke(null);
                }
            });
        }
        
        /// <summary>
        /// Retrieves the current user's score from the database.
        /// </summary>
        /// <param name="callback">Callback with the retrieved score data</param>
        public void GetCurrentUserScore(System.Action<Dictionary<string, object>> callback)
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                Debug.LogWarning("No authenticated user. Cannot retrieve score.");
                OnFirebaseError?.Invoke("No authenticated user");
                callback?.Invoke(null);
                return;
            }
            
            GetPlayerScore(UserId, callback);
        }
        
        /// <summary>
        /// Signs in anonymously to Firebase.
        /// </summary>
        public void SignInAnonymously()
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("Firebase not initialized. Cannot sign in.");
                OnFirebaseError?.Invoke("Firebase not initialized");
                return;
            }
            
            if (IsAuthenticated)
            {
                Debug.LogWarning("Already authenticated. Cannot sign in again.");
                return;
            }
            
            // Note: This would require Firebase Auth integration
            // For now, we'll create a simple anonymous ID
            UserId = System.Guid.NewGuid().ToString();
            IsAuthenticated = true;
            
            Debug.Log($"Signed in anonymously with user ID: {UserId}");
        }
        
        /// <summary>
        /// Signs out of Firebase.
        /// </summary>
        public void SignOut()
        {
            if (!IsAuthenticated)
            {
                Debug.LogWarning("Not authenticated. Cannot sign out.");
                return;
            }
            
            UserId = null;
            IsAuthenticated = false;
            
            Debug.Log("Signed out of Firebase.");
        }
        
        /// <summary>
        /// Sets the default collection name for database operations.
        /// </summary>
        /// <param name="collectionName">The collection name to use</param>
        public void SetDefaultCollection(string collectionName)
        {
            if (string.IsNullOrWhiteSpace(collectionName))
            {
                Debug.LogWarning("Collection name cannot be empty.");
                return;
            }
            
            DefaultCollection = collectionName;
        }
        
        /// <summary>
        /// Enables or disables debug logging.
        /// </summary>
        /// <param name="enabled">Whether to enable debug logging</param>
        public void SetDebugLogging(bool enabled)
        {
            _enableDebugLogging = enabled;
        }
        
        /// <summary>
        /// Gets the current Firebase dependency status.
        /// </summary>
        /// <returns>Current dependency status</returns>
        public DependencyStatus GetDependencyStatus()
        {
            return _dependencyStatus;
        }
        
        #endregion

        #region Private Methods
        
        /// <summary>
        /// Called when Firebase dependencies are resolved successfully.
        /// </summary>
        private void OnFirebaseDependenciesResolved()
        {
            try
            {
                Database = FirebaseFirestore.DefaultInstance;
                IsInitialized = true;
                
                if (_enableDebugLogging)
                {
                    Debug.Log("Firebase is ready!");
                }
                
                OnFirebaseInitialized?.Invoke();
                
                // Auto sign in if enabled
                if (_enableAnonymousAuth && _autoSignIn)
                {
                    SignInAnonymously();
                }
            }
            catch (System.Exception e)
            {
                string errorMessage = $"Failed to initialize Firebase: {e.Message}";
                Debug.LogError(errorMessage);
                OnFirebaseError?.Invoke(errorMessage);
            }
        }
        
        #endregion

        #region Validation
        
        private void OnValidate()
        {
            // Ensure default collection is not null
            if (string.IsNullOrEmpty(_defaultCollection))
            {
                _defaultCollection = "leaderboard";
            }
        }
        
        #endregion
    }
}