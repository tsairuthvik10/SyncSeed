using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.Events;

namespace SyncSeed
{
    /// <summary>
    /// Manages AR mirror placement and interaction in the game world.
    /// Handles AR raycasting, mirror spawning, and visual effects.
    /// </summary>
    public class MirrorManager : MonoBehaviour
    {
        #region Singleton Implementation
        
        public static MirrorManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeMirrorManager();
            }
            else if (Instance != this)
            {
                Debug.LogWarning("Multiple MirrorManager instances detected. Destroying duplicate.");
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
        
        [Header("Mirror Events")]
        public UnityEvent<GameObject> OnMirrorPlaced;
        public UnityEvent OnMirrorLimitReached;
        public UnityEvent<int> OnMirrorCountChanged;
        public UnityEvent OnMirrorsReset;
        public UnityEvent<string> OnMirrorError;
        
        #endregion

        #region Serialized Fields
        
        [Header("Mirror Configuration")]
        [SerializeField] private GameObject _mirrorPrefab;
        [SerializeField] private int _maxMirrorsPerLevel = 3;
        [SerializeField] private float _mirrorPlacementHeight = 0f;
        [SerializeField] private float _mirrorSpacing = 1.5f;
        
        [Header("AR Components")]
        [SerializeField] private ARRaycastManager _raycastManager;
        [SerializeField] private Camera _arCamera;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem _mirrorSpawnEffect;
        [SerializeField] private bool _enableSpawnEffects = true;
        [SerializeField] private float _spawnEffectDuration = 2f;
        
        [Header("Placement Settings")]
        [SerializeField] private bool _autoFaceCamera = true;
        [SerializeField] private bool _snapToGrid = false;
        [SerializeField] private float _gridSize = 0.5f;
        [SerializeField] private LayerMask _placementLayerMask = -1;
        
        #endregion

        #region Properties
        
        public GameObject MirrorPrefab 
        { 
            get => _mirrorPrefab; 
            set => _mirrorPrefab = value; 
        }
        
        public int MaxMirrorsPerLevel 
        { 
            get => _maxMirrorsPerLevel; 
            set => _maxMirrorsPerLevel = Mathf.Max(0, value); 
        }
        
        public int CurrentMirrorCount { get; private set; }
        public bool CanPlaceMirror => CurrentMirrorCount < _maxMirrorsPerLevel;
        public bool IsInitialized { get; private set; }
        
        #endregion

        #region Private Fields
        
        private List<GameObject> _placedMirrors = new List<GameObject>();
        private bool _isPlacingMirror = false;
        
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
        /// Places a mirror at the center of the screen using AR raycasting.
        /// </summary>
        public void PlaceMirror()
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("MirrorManager not initialized. Cannot place mirror.");
                OnMirrorError?.Invoke("MirrorManager not initialized");
                return;
            }
            
            if (!CanPlaceMirror)
            {
                Debug.LogWarning("Mirror limit reached. Cannot place more mirrors.");
                OnMirrorLimitReached?.Invoke();
                return;
            }
            
            if (_isPlacingMirror)
            {
                Debug.LogWarning("Already placing a mirror. Please wait.");
                return;
            }
            
            _isPlacingMirror = true;
            
            Vector2 screenCenter = GetScreenCenter();
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            
            if (_raycastManager != null && _raycastManager.Raycast(screenCenter, hits, TrackableType.Planes))
            {
                Pose hitPose = hits[0].pose;
                Vector3 placementPosition = CalculatePlacementPosition(hitPose.position);
                Quaternion placementRotation = CalculatePlacementRotation(hitPose.position);
                
                GameObject mirror = CreateMirror(placementPosition, placementRotation);
                
                if (mirror != null)
                {
                    CurrentMirrorCount++;
                    _placedMirrors.Add(mirror);
                    
                    OnMirrorPlaced?.Invoke(mirror);
                    OnMirrorCountChanged?.Invoke(CurrentMirrorCount);
                    
                    Debug.Log($"Mirror placed successfully. Count: {CurrentMirrorCount}/{_maxMirrorsPerLevel}");
                }
                else
                {
                    OnMirrorError?.Invoke("Failed to create mirror");
                }
            }
            else
            {
                Debug.LogWarning("No valid placement surface found.");
                OnMirrorError?.Invoke("No valid placement surface found");
            }
            
            _isPlacingMirror = false;
        }
        
        /// <summary>
        /// Places a mirror at a specific world position.
        /// </summary>
        /// <param name="position">World position to place the mirror</param>
        /// <param name="rotation">Rotation for the mirror</param>
        /// <returns>True if mirror was placed successfully</returns>
        public bool PlaceMirrorAtPosition(Vector3 position, Quaternion rotation)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("MirrorManager not initialized. Cannot place mirror.");
                OnMirrorError?.Invoke("MirrorManager not initialized");
                return false;
            }
            
            if (!CanPlaceMirror)
            {
                Debug.LogWarning("Mirror limit reached. Cannot place more mirrors.");
                OnMirrorLimitReached?.Invoke();
                return false;
            }
            
            Vector3 placementPosition = CalculatePlacementPosition(position);
            Quaternion placementRotation = CalculatePlacementRotation(position);
            
            GameObject mirror = CreateMirror(placementPosition, placementRotation);
            
            if (mirror != null)
            {
                CurrentMirrorCount++;
                _placedMirrors.Add(mirror);
                
                OnMirrorPlaced?.Invoke(mirror);
                OnMirrorCountChanged?.Invoke(CurrentMirrorCount);
                
                Debug.Log($"Mirror placed at position successfully. Count: {CurrentMirrorCount}/{_maxMirrorsPerLevel}");
                return true;
            }
            
            OnMirrorError?.Invoke("Failed to create mirror at position");
            return false;
        }
        
        /// <summary>
        /// Sets the maximum number of mirrors allowed per level.
        /// </summary>
        /// <param name="newLimit">New mirror limit</param>
        public void SetMirrorLimit(int newLimit)
        {
            MaxMirrorsPerLevel = newLimit;
            Debug.Log($"Mirror limit set to: {_maxMirrorsPerLevel}");
        }
        
        /// <summary>
        /// Resets the mirror count and removes all placed mirrors.
        /// </summary>
        public void ResetMirrorCount()
        {
            RemoveAllMirrors();
            CurrentMirrorCount = 0;
            OnMirrorsReset?.Invoke();
            OnMirrorCountChanged?.Invoke(CurrentMirrorCount);
            Debug.Log("Mirror count reset.");
        }
        
        /// <summary>
        /// Removes a specific mirror from the scene.
        /// </summary>
        /// <param name="mirror">The mirror to remove</param>
        /// <returns>True if mirror was removed successfully</returns>
        public bool RemoveMirror(GameObject mirror)
        {
            if (mirror == null)
            {
                Debug.LogWarning("Mirror reference is null. Cannot remove.");
                return false;
            }
            
            if (_placedMirrors.Contains(mirror))
            {
                _placedMirrors.Remove(mirror);
                Destroy(mirror);
                CurrentMirrorCount--;
                OnMirrorCountChanged?.Invoke(CurrentMirrorCount);
                Debug.Log($"Mirror removed. Count: {CurrentMirrorCount}/{_maxMirrorsPerLevel}");
                return true;
            }
            
            Debug.LogWarning("Mirror not found in placed mirrors list.");
            return false;
        }
        
        /// <summary>
        /// Removes all placed mirrors from the scene.
        /// </summary>
        public void RemoveAllMirrors()
        {
            foreach (GameObject mirror in _placedMirrors)
            {
                if (mirror != null)
                {
                    Destroy(mirror);
                }
            }
            
            _placedMirrors.Clear();
            Debug.Log("All mirrors removed.");
        }
        
        /// <summary>
        /// Gets all currently placed mirrors.
        /// </summary>
        /// <returns>List of placed mirror GameObjects</returns>
        public List<GameObject> GetPlacedMirrors()
        {
            return new List<GameObject>(_placedMirrors);
        }
        
        /// <summary>
        /// Enables or disables spawn effects.
        /// </summary>
        /// <param name="enabled">Whether to enable spawn effects</param>
        public void SetSpawnEffectsEnabled(bool enabled)
        {
            _enableSpawnEffects = enabled;
        }
        
        /// <summary>
        /// Sets the mirror placement height offset.
        /// </summary>
        /// <param name="height">Height offset from placement surface</param>
        public void SetPlacementHeight(float height)
        {
            _mirrorPlacementHeight = height;
        }
        
        #endregion

        #region Private Methods
        
        /// <summary>
        /// Initializes the mirror manager with default values.
        /// </summary>
        private void InitializeMirrorManager()
        {
            // Validate configuration
            if (_maxMirrorsPerLevel < 0)
            {
                Debug.LogWarning("Max mirrors per level cannot be negative. Setting to 0.");
                _maxMirrorsPerLevel = 0;
            }
            
            if (_spawnEffectDuration < 0)
            {
                Debug.LogWarning("Spawn effect duration cannot be negative. Setting to 0.");
                _spawnEffectDuration = 0;
            }
            
            CurrentMirrorCount = 0;
            _placedMirrors.Clear();
            
            IsInitialized = true;
            Debug.Log("MirrorManager initialized successfully.");
        }
        
        /// <summary>
        /// Validates that all required references are set.
        /// </summary>
        private void ValidateReferences()
        {
            if (_mirrorPrefab == null)
            {
                Debug.LogError("Mirror prefab reference is missing in MirrorManager.");
            }
            
            if (_raycastManager == null)
            {
                Debug.LogError("AR Raycast Manager reference is missing in MirrorManager.");
            }
            
            if (_arCamera == null)
            {
                _arCamera = Camera.main;
                if (_arCamera == null)
                {
                    Debug.LogWarning("AR Camera reference is missing. Using Camera.main as fallback.");
                }
            }
        }
        
        /// <summary>
        /// Gets the center point of the screen for raycasting.
        /// </summary>
        /// <returns>Screen center point</returns>
        private Vector2 GetScreenCenter()
        {
            if (_arCamera != null)
            {
                return _arCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0f));
            }
            
            return new Vector2(Screen.width / 2f, Screen.height / 2f);
        }
        
        /// <summary>
        /// Calculates the final placement position for a mirror.
        /// </summary>
        /// <param name="hitPosition">Position from raycast hit</param>
        /// <returns>Adjusted placement position</returns>
        private Vector3 CalculatePlacementPosition(Vector3 hitPosition)
        {
            Vector3 placementPosition = hitPosition;
            placementPosition.y += _mirrorPlacementHeight;
            
            if (_snapToGrid)
            {
                placementPosition.x = Mathf.Round(placementPosition.x / _gridSize) * _gridSize;
                placementPosition.z = Mathf.Round(placementPosition.z / _gridSize) * _gridSize;
            }
            
            return placementPosition;
        }
        
        /// <summary>
        /// Calculates the rotation for a mirror based on camera position.
        /// </summary>
        /// <param name="mirrorPosition">Position of the mirror</param>
        /// <returns>Rotation for the mirror</returns>
        private Quaternion CalculatePlacementRotation(Vector3 mirrorPosition)
        {
            if (_autoFaceCamera && _arCamera != null)
            {
                Vector3 directionToCamera = _arCamera.transform.position - mirrorPosition;
                return Quaternion.LookRotation(directionToCamera.normalized);
            }
            
            return Quaternion.identity;
        }
        
        /// <summary>
        /// Creates a mirror GameObject at the specified position and rotation.
        /// </summary>
        /// <param name="position">Position to place the mirror</param>
        /// <param name="rotation">Rotation for the mirror</param>
        /// <returns>Created mirror GameObject or null if failed</returns>
        private GameObject CreateMirror(Vector3 position, Quaternion rotation)
        {
            if (_mirrorPrefab == null)
            {
                Debug.LogError("Mirror prefab is null. Cannot create mirror.");
                return null;
            }
            
            GameObject mirror = Instantiate(_mirrorPrefab, position, rotation);
            
            if (mirror != null)
            {
                // Apply spawn effect if enabled
                if (_enableSpawnEffects && _mirrorSpawnEffect != null)
                {
                    CreateSpawnEffect(position);
                }
                
                Debug.Log($"Mirror created at position: {position}");
            }
            
            return mirror;
        }
        
        /// <summary>
        /// Creates a spawn effect at the specified position.
        /// </summary>
        /// <param name="position">Position for the spawn effect</param>
        private void CreateSpawnEffect(Vector3 position)
        {
            if (_mirrorSpawnEffect == null) return;
            
            ParticleSystem effect = Instantiate(_mirrorSpawnEffect, position, Quaternion.identity);
            effect.Play();
            
            // Destroy the effect after it finishes
            Destroy(effect.gameObject, _spawnEffectDuration);
        }
        
        #endregion

        #region Validation
        
        private void OnValidate()
        {
            // Ensure max mirrors is non-negative
            _maxMirrorsPerLevel = Mathf.Max(0, _maxMirrorsPerLevel);
            
            // Ensure spawn effect duration is non-negative
            _spawnEffectDuration = Mathf.Max(0f, _spawnEffectDuration);
            
            // Ensure grid size is positive
            _gridSize = Mathf.Max(0.1f, _gridSize);
        }
        
        #endregion
    }
}