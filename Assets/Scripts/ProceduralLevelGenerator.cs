using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace SyncSeed
{
    /// <summary>
    /// Generates procedural levels with increasing difficulty.
    /// Creates rhythm nodes and configures game parameters based on level progression.
    /// </summary>
    public class ProceduralLevelGenerator : MonoBehaviour
    {
        #region Events
        
        [Header("Level Generation Events")]
        public UnityEvent<int> OnLevelGenerated;
        public UnityEvent<int> OnNodeCountChanged;
        public UnityEvent<float> OnBeatIntervalChanged;
        public UnityEvent<int> OnMirrorLimitChanged;
        public UnityEvent<string> OnGenerationError;
        
        #endregion

        #region Serialized Fields
        
        [Header("Level Configuration")]
        [SerializeField] private GameObject _rhythmNodePrefab;
        [SerializeField] private Transform _spawnRoot;
        [SerializeField] private int _baseNodeCount = 3;
        [SerializeField] private float _baseBeatInterval = 2.0f;
        [SerializeField] private int _currentLevel = 1;
        
        [Header("Difficulty Scaling")]
        [SerializeField] private float _nodeCountIncreasePerLevel = 1f;
        [SerializeField] private float _beatIntervalDecreasePerLevel = 0.1f;
        [SerializeField] private float _minBeatInterval = 0.5f;
        [SerializeField] private float _maxBeatInterval = 5.0f;
        
        [Header("Mirror Configuration")]
        [SerializeField] private int _baseMirrorLimit = 3;
        [SerializeField] private float _mirrorLimitDecreasePerLevel = 0.5f;
        [SerializeField] private int _minMirrorLimit = 1;
        [SerializeField] private int _maxMirrorLimit = 10;
        
        [Header("Spawn Settings")]
        [SerializeField] private float _spawnRadius = 1.5f;
        [SerializeField] private float _minSpawnDistance = 0.5f;
        [SerializeField] private bool _avoidOverlap = true;
        [SerializeField] private int _maxSpawnAttempts = 10;
        
        [Header("Generation Settings")]
        [SerializeField] private bool _autoGenerateOnStart = false;
        [SerializeField] private bool _clearPreviousLevel = true;
        [SerializeField] private bool _applyDifficultyScaling = true;
        
        #endregion

        #region Properties
        
        public GameObject RhythmNodePrefab 
        { 
            get => _rhythmNodePrefab; 
            set => _rhythmNodePrefab = value; 
        }
        
        public Transform SpawnRoot 
        { 
            get => _spawnRoot; 
            set => _spawnRoot = value; 
        }
        
        public int CurrentLevel 
        { 
            get => _currentLevel; 
            set => _currentLevel = Mathf.Max(1, value); 
        }
        
        public int BaseNodeCount 
        { 
            get => _baseNodeCount; 
            set => _baseNodeCount = Mathf.Max(1, value); 
        }
        
        public float BaseBeatInterval 
        { 
            get => _baseBeatInterval; 
            set => _baseBeatInterval = Mathf.Clamp(value, _minBeatInterval, _maxBeatInterval); 
        }
        
        public bool IsInitialized { get; private set; }
        
        #endregion

        #region Private Fields
        
        private List<GameObject> _spawnedNodes = new List<GameObject>();
        private bool _isGenerating = false;
        
        #endregion

        #region Unity Lifecycle
        
        private void Start()
        {
            InitializeLevelGenerator();
            
            if (_autoGenerateOnStart)
            {
                GenerateLevel();
            }
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Generates a new level with the current level settings.
        /// </summary>
        public void GenerateLevel()
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("LevelGenerator not initialized. Cannot generate level.");
                OnGenerationError?.Invoke("LevelGenerator not initialized");
                return;
            }
            
            if (_isGenerating)
            {
                Debug.LogWarning("Level generation already in progress. Please wait.");
                return;
            }
            
            _isGenerating = true;
            
            try
            {
                // Clear previous level if enabled
                if (_clearPreviousLevel)
                {
                    ClearCurrentLevel();
                }
                
                // Calculate level parameters
                int nodeCount = CalculateNodeCount();
                float beatInterval = CalculateBeatInterval();
                int mirrorLimit = CalculateMirrorLimit();
                
                // Spawn rhythm nodes
                SpawnRhythmNodes(nodeCount);
                
                // Apply difficulty settings
                if (_applyDifficultyScaling)
                {
                    ApplyDifficultySettings(beatInterval, mirrorLimit);
                }
                
                OnLevelGenerated?.Invoke(_currentLevel);
                Debug.Log($"Level {_currentLevel} generated successfully with {nodeCount} nodes.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error generating level: {e.Message}");
                OnGenerationError?.Invoke($"Error generating level: {e.Message}");
            }
            finally
            {
                _isGenerating = false;
            }
        }
        
        /// <summary>
        /// Generates a level for a specific level number.
        /// </summary>
        /// <param name="levelNumber">The level number to generate</param>
        public void GenerateLevel(int levelNumber)
        {
            CurrentLevel = levelNumber;
            GenerateLevel();
        }
        
        /// <summary>
        /// Advances to the next level and generates it.
        /// </summary>
        public void GenerateNextLevel()
        {
            CurrentLevel++;
            GenerateLevel();
        }
        
        /// <summary>
        /// Clears all spawned nodes from the current level.
        /// </summary>
        public void ClearCurrentLevel()
        {
            foreach (GameObject node in _spawnedNodes)
            {
                if (node != null)
                {
                    Destroy(node);
                }
            }
            
            _spawnedNodes.Clear();
            Debug.Log("Current level cleared.");
        }
        
        /// <summary>
        /// Sets the current level number.
        /// </summary>
        /// <param name="level">The level number to set</param>
        public void SetLevel(int level)
        {
            CurrentLevel = level;
        }
        
        /// <summary>
        /// Sets the base node count for level generation.
        /// </summary>
        /// <param name="nodeCount">Base number of nodes</param>
        public void SetBaseNodeCount(int nodeCount)
        {
            BaseNodeCount = nodeCount;
        }
        
        /// <summary>
        /// Sets the base beat interval for level generation.
        /// </summary>
        /// <param name="beatInterval">Base beat interval in seconds</param>
        public void SetBaseBeatInterval(float beatInterval)
        {
            BaseBeatInterval = beatInterval;
        }
        
        /// <summary>
        /// Sets the spawn radius for rhythm nodes.
        /// </summary>
        /// <param name="radius">Spawn radius</param>
        public void SetSpawnRadius(float radius)
        {
            _spawnRadius = Mathf.Max(0.1f, radius);
        }
        
        /// <summary>
        /// Gets all currently spawned rhythm nodes.
        /// </summary>
        /// <returns>List of spawned node GameObjects</returns>
        public List<GameObject> GetSpawnedNodes()
        {
            return new List<GameObject>(_spawnedNodes);
        }
        
        /// <summary>
        /// Gets the number of currently spawned nodes.
        /// </summary>
        /// <returns>Number of spawned nodes</returns>
        public int GetSpawnedNodeCount()
        {
            return _spawnedNodes.Count;
        }
        
        #endregion

        #region Private Methods
        
        /// <summary>
        /// Initializes the level generator with default values.
        /// </summary>
        private void InitializeLevelGenerator()
        {
            // Validate configuration
            if (_baseNodeCount < 1)
            {
                Debug.LogWarning("Base node count must be at least 1. Setting to 1.");
                _baseNodeCount = 1;
            }
            
            if (_baseBeatInterval < _minBeatInterval || _baseBeatInterval > _maxBeatInterval)
            {
                Debug.LogWarning("Base beat interval is outside valid range. Clamping to valid range.");
                _baseBeatInterval = Mathf.Clamp(_baseBeatInterval, _minBeatInterval, _maxBeatInterval);
            }
            
            if (_spawnRadius < 0.1f)
            {
                Debug.LogWarning("Spawn radius must be at least 0.1. Setting to 0.1.");
                _spawnRadius = 0.1f;
            }
            
            if (_minSpawnDistance < 0.1f)
            {
                Debug.LogWarning("Minimum spawn distance must be at least 0.1. Setting to 0.1.");
                _minSpawnDistance = 0.1f;
            }
            
            _spawnedNodes.Clear();
            _currentLevel = Mathf.Max(1, _currentLevel);
            
            IsInitialized = true;
            Debug.Log("ProceduralLevelGenerator initialized successfully.");
        }
        
        /// <summary>
        /// Calculates the number of nodes for the current level.
        /// </summary>
        /// <returns>Number of nodes to spawn</returns>
        private int CalculateNodeCount()
        {
            int nodeCount = _baseNodeCount + Mathf.RoundToInt((_currentLevel - 1) * _nodeCountIncreasePerLevel);
            nodeCount = Mathf.Max(1, nodeCount);
            
            OnNodeCountChanged?.Invoke(nodeCount);
            return nodeCount;
        }
        
        /// <summary>
        /// Calculates the beat interval for the current level.
        /// </summary>
        /// <returns>Beat interval in seconds</returns>
        private float CalculateBeatInterval()
        {
            float beatInterval = _baseBeatInterval - (_currentLevel - 1) * _beatIntervalDecreasePerLevel;
            beatInterval = Mathf.Clamp(beatInterval, _minBeatInterval, _maxBeatInterval);
            
            OnBeatIntervalChanged?.Invoke(beatInterval);
            return beatInterval;
        }
        
        /// <summary>
        /// Calculates the mirror limit for the current level.
        /// </summary>
        /// <returns>Mirror limit</returns>
        private int CalculateMirrorLimit()
        {
            int mirrorLimit = _baseMirrorLimit - Mathf.RoundToInt((_currentLevel - 1) * _mirrorLimitDecreasePerLevel);
            mirrorLimit = Mathf.Clamp(mirrorLimit, _minMirrorLimit, _maxMirrorLimit);
            
            OnMirrorLimitChanged?.Invoke(mirrorLimit);
            return mirrorLimit;
        }
        
        /// <summary>
        /// Spawns rhythm nodes at calculated positions.
        /// </summary>
        /// <param name="nodeCount">Number of nodes to spawn</param>
        private void SpawnRhythmNodes(int nodeCount)
        {
            if (_rhythmNodePrefab == null)
            {
                Debug.LogError("Rhythm node prefab is null. Cannot spawn nodes.");
                return;
            }
            
            if (_spawnRoot == null)
            {
                Debug.LogError("Spawn root is null. Cannot spawn nodes.");
                return;
            }
            
            for (int i = 0; i < nodeCount; i++)
            {
                Vector3 spawnPosition = CalculateSpawnPosition();
                GameObject node = Instantiate(_rhythmNodePrefab, spawnPosition, Quaternion.identity, _spawnRoot);
                
                if (node != null)
                {
                    _spawnedNodes.Add(node);
                    
                    // Configure the rhythm node if it has a RhythmNode component
                    RhythmNode rhythmNode = node.GetComponent<RhythmNode>();
                    if (rhythmNode != null)
                    {
                        rhythmNode.SetBeatInterval(CalculateBeatInterval());
                    }
                }
            }
        }
        
        /// <summary>
        /// Calculates a spawn position for a rhythm node.
        /// </summary>
        /// <returns>Valid spawn position</returns>
        private Vector3 CalculateSpawnPosition()
        {
            Vector3 basePosition = _spawnRoot.position;
            
            for (int attempt = 0; attempt < _maxSpawnAttempts; attempt++)
            {
                Vector3 randomOffset = Random.insideUnitSphere * _spawnRadius;
                randomOffset.y = 0; // Keep nodes at the same height as spawn root
                
                Vector3 candidatePosition = basePosition + randomOffset;
                
                if (!_avoidOverlap || IsValidSpawnPosition(candidatePosition))
                {
                    return candidatePosition;
                }
            }
            
            // If we can't find a valid position, return a random position
            Vector3 fallbackPosition = basePosition + Random.insideUnitSphere * _spawnRadius;
            fallbackPosition.y = basePosition.y;
            return fallbackPosition;
        }
        
        /// <summary>
        /// Checks if a position is valid for spawning (not too close to existing nodes).
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <returns>True if position is valid</returns>
        private bool IsValidSpawnPosition(Vector3 position)
        {
            foreach (GameObject node in _spawnedNodes)
            {
                if (node != null)
                {
                    float distance = Vector3.Distance(position, node.transform.position);
                    if (distance < _minSpawnDistance)
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Applies difficulty settings to the game managers.
        /// </summary>
        /// <param name="beatInterval">Beat interval to set</param>
        /// <param name="mirrorLimit">Mirror limit to set</param>
        private void ApplyDifficultySettings(float beatInterval, int mirrorLimit)
        {
            // Set rhythm manager beat interval
            if (RhythmManager.Instance != null)
            {
                RhythmManager.Instance.SetBeatInterval(beatInterval);
            }
            else
            {
                Debug.LogWarning("RhythmManager instance not found. Cannot set beat interval.");
            }
            
            // Set mirror manager limit
            if (MirrorManager.Instance != null)
            {
                MirrorManager.Instance.SetMirrorLimit(mirrorLimit);
            }
            else
            {
                Debug.LogWarning("MirrorManager instance not found. Cannot set mirror limit.");
            }
        }
        
        #endregion

        #region Validation
        
        private void OnValidate()
        {
            // Ensure base node count is at least 1
            _baseNodeCount = Mathf.Max(1, _baseNodeCount);
            
            // Ensure beat intervals are in valid range
            _baseBeatInterval = Mathf.Clamp(_baseBeatInterval, _minBeatInterval, _maxBeatInterval);
            _minBeatInterval = Mathf.Max(0.1f, _minBeatInterval);
            _maxBeatInterval = Mathf.Max(_minBeatInterval, _maxBeatInterval);
            
            // Ensure mirror limits are valid
            _baseMirrorLimit = Mathf.Clamp(_baseMirrorLimit, _minMirrorLimit, _maxMirrorLimit);
            _minMirrorLimit = Mathf.Max(0, _minMirrorLimit);
            _maxMirrorLimit = Mathf.Max(_minMirrorLimit, _maxMirrorLimit);
            
            // Ensure spawn settings are valid
            _spawnRadius = Mathf.Max(0.1f, _spawnRadius);
            _minSpawnDistance = Mathf.Max(0.1f, _minSpawnDistance);
            _maxSpawnAttempts = Mathf.Max(1, _maxSpawnAttempts);
            
            // Ensure current level is at least 1
            _currentLevel = Mathf.Max(1, _currentLevel);
        }
        
        #endregion
    }
}