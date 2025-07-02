using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.Events;

namespace SyncSeed
{
    /// <summary>
    /// Handles AR anchor placement and tracking for AR experiences.
    /// Manages placement indicators and AR raycasting functionality.
    /// </summary>
    public class ARAnchorHandler : MonoBehaviour
    {
        #region Events
        
        [Header("AR Anchor Events")]
        public UnityEvent<Vector3> OnPlacementPositionUpdated;
        public UnityEvent<bool> OnPlacementValidChanged;
        public UnityEvent<Vector3> OnAnchorPlaced;
        public UnityEvent<string> OnARError;
        
        #endregion

        #region Serialized Fields
        
        [Header("AR Components")]
        [SerializeField] private GameObject _placementIndicator;
        [SerializeField] private ARRaycastManager _raycastManager;
        [SerializeField] private Camera _arCamera;
        
        [Header("Placement Settings")]
        [SerializeField] private TrackableType _trackableTypes = TrackableType.PlaneWithinPolygon;
        [SerializeField] private bool _autoUpdatePlacement = true;
        [SerializeField] private float _updateInterval = 0.1f;
        [SerializeField] private bool _showPlacementIndicator = true;
        
        [Header("Visual Feedback")]
        [SerializeField] private Material _validPlacementMaterial;
        [SerializeField] private Material _invalidPlacementMaterial;
        [SerializeField] private Color _validPlacementColor = Color.green;
        [SerializeField] private Color _invalidPlacementColor = Color.red;
        
        #endregion

        #region Properties
        
        public GameObject PlacementIndicator 
        { 
            get => _placementIndicator; 
            set => _placementIndicator = value; 
        }
        
        public ARRaycastManager RaycastManager 
        { 
            get => _raycastManager; 
            set => _raycastManager = value; 
        }
        
        public Camera ARCamera 
        { 
            get => _arCamera; 
            set => _arCamera = value; 
        }
        
        public bool IsPlacementValid { get; private set; }
        public Vector3 CurrentPlacementPosition { get; private set; }
        public Quaternion CurrentPlacementRotation { get; private set; }
        public bool IsInitialized { get; private set; }
        
        #endregion

        #region Private Fields
        
        private float _lastUpdateTime;
        private Renderer _placementIndicatorRenderer;
        private bool _isUpdating = false;
        
        #endregion

        #region Unity Lifecycle
        
        private void Start()
        {
            InitializeARAnchorHandler();
        }
        
        private void Update()
        {
            if (_autoUpdatePlacement && IsInitialized && !_isUpdating)
            {
                if (Time.time - _lastUpdateTime >= _updateInterval)
                {
                    UpdatePlacement();
                    _lastUpdateTime = Time.time;
                }
            }
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Updates the placement indicator position and validity.
        /// </summary>
        public void UpdatePlacement()
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("ARAnchorHandler not initialized. Cannot update placement.");
                return;
            }
            
            if (_isUpdating)
            {
                return;
            }
            
            _isUpdating = true;
            
            try
            {
                Vector2 screenCenter = GetScreenCenter();
                List<ARRaycastHit> hits = new List<ARRaycastHit>();
                
                bool isValidPlacement = false;
                Vector3 placementPosition = Vector3.zero;
                Quaternion placementRotation = Quaternion.identity;
                
                if (_raycastManager != null && _raycastManager.Raycast(screenCenter, hits, _trackableTypes))
                {
                    Pose hitPose = hits[0].pose;
                    placementPosition = hitPose.position;
                    placementRotation = hitPose.rotation;
                    isValidPlacement = true;
                }
                
                UpdatePlacementIndicator(isValidPlacement, placementPosition, placementRotation);
                
                // Update properties
                bool placementChanged = IsPlacementValid != isValidPlacement || 
                                      CurrentPlacementPosition != placementPosition;
                
                IsPlacementValid = isValidPlacement;
                CurrentPlacementPosition = placementPosition;
                CurrentPlacementRotation = placementRotation;
                
                // Invoke events
                OnPlacementPositionUpdated?.Invoke(placementPosition);
                
                if (placementChanged)
                {
                    OnPlacementValidChanged?.Invoke(isValidPlacement);
                }
            }
            catch (System.Exception e)
            {
                string errorMessage = $"Error updating placement: {e.Message}";
                Debug.LogError(errorMessage);
                OnARError?.Invoke(errorMessage);
            }
            finally
            {
                _isUpdating = false;
            }
        }
        
        /// <summary>
        /// Places an anchor at the current placement position.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate as an anchor</param>
        /// <returns>The placed anchor GameObject or null if failed</returns>
        public GameObject PlaceAnchor(GameObject prefab)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("ARAnchorHandler not initialized. Cannot place anchor.");
                OnARError?.Invoke("ARAnchorHandler not initialized");
                return null;
            }
            
            if (!IsPlacementValid)
            {
                Debug.LogWarning("Placement is not valid. Cannot place anchor.");
                OnARError?.Invoke("Placement is not valid");
                return null;
            }
            
            if (prefab == null)
            {
                Debug.LogWarning("Anchor prefab is null. Cannot place anchor.");
                OnARError?.Invoke("Anchor prefab is null");
                return null;
            }
            
            try
            {
                GameObject anchor = Instantiate(prefab, CurrentPlacementPosition, CurrentPlacementRotation);
                OnAnchorPlaced?.Invoke(CurrentPlacementPosition);
                
                Debug.Log($"Anchor placed successfully at position: {CurrentPlacementPosition}");
                return anchor;
            }
            catch (System.Exception e)
            {
                string errorMessage = $"Failed to place anchor: {e.Message}";
                Debug.LogError(errorMessage);
                OnARError?.Invoke(errorMessage);
                return null;
            }
        }
        
        /// <summary>
        /// Places an anchor at a specific world position.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate as an anchor</param>
        /// <param name="position">World position to place the anchor</param>
        /// <param name="rotation">Rotation for the anchor</param>
        /// <returns>The placed anchor GameObject or null if failed</returns>
        public GameObject PlaceAnchorAtPosition(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("ARAnchorHandler not initialized. Cannot place anchor.");
                OnARError?.Invoke("ARAnchorHandler not initialized");
                return null;
            }
            
            if (prefab == null)
            {
                Debug.LogWarning("Anchor prefab is null. Cannot place anchor.");
                OnARError?.Invoke("Anchor prefab is null");
                return null;
            }
            
            try
            {
                GameObject anchor = Instantiate(prefab, position, rotation);
                OnAnchorPlaced?.Invoke(position);
                
                Debug.Log($"Anchor placed successfully at position: {position}");
                return anchor;
            }
            catch (System.Exception e)
            {
                string errorMessage = $"Failed to place anchor: {e.Message}";
                Debug.LogError(errorMessage);
                OnARError?.Invoke(errorMessage);
                return null;
            }
        }
        
        /// <summary>
        /// Shows or hides the placement indicator.
        /// </summary>
        /// <param name="show">Whether to show the placement indicator</param>
        public void SetPlacementIndicatorVisible(bool show)
        {
            _showPlacementIndicator = show;
            
            if (_placementIndicator != null)
            {
                _placementIndicator.SetActive(show);
            }
        }
        
        /// <summary>
        /// Sets the trackable types for AR raycasting.
        /// </summary>
        /// <param name="trackableTypes">Trackable types to use</param>
        public void SetTrackableTypes(TrackableType trackableTypes)
        {
            _trackableTypes = trackableTypes;
        }
        
        /// <summary>
        /// Sets the update interval for placement updates.
        /// </summary>
        /// <param name="interval">Update interval in seconds</param>
        public void SetUpdateInterval(float interval)
        {
            _updateInterval = Mathf.Max(0.01f, interval);
        }
        
        /// <summary>
        /// Enables or disables automatic placement updates.
        /// </summary>
        /// <param name="enabled">Whether to enable auto updates</param>
        public void SetAutoUpdateEnabled(bool enabled)
        {
            _autoUpdatePlacement = enabled;
        }
        
        /// <summary>
        /// Sets the valid placement material.
        /// </summary>
        /// <param name="material">Material for valid placement</param>
        public void SetValidPlacementMaterial(Material material)
        {
            _validPlacementMaterial = material;
        }
        
        /// <summary>
        /// Sets the invalid placement material.
        /// </summary>
        /// <param name="material">Material for invalid placement</param>
        public void SetInvalidPlacementMaterial(Material material)
        {
            _invalidPlacementMaterial = material;
        }
        
        /// <summary>
        /// Sets the valid placement color.
        /// </summary>
        /// <param name="color">Color for valid placement</param>
        public void SetValidPlacementColor(Color color)
        {
            _validPlacementColor = color;
        }
        
        /// <summary>
        /// Sets the invalid placement color.
        /// </summary>
        /// <param name="color">Color for invalid placement</param>
        public void SetInvalidPlacementColor(Color color)
        {
            _invalidPlacementColor = color;
        }
        
        /// <summary>
        /// Gets the current screen center point for raycasting.
        /// </summary>
        /// <returns>Screen center point</returns>
        public Vector2 GetScreenCenter()
        {
            if (_arCamera != null)
            {
                return _arCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0f));
            }
            
            return new Vector2(Screen.width / 2f, Screen.height / 2f);
        }
        
        #endregion

        #region Private Methods
        
        /// <summary>
        /// Initializes the AR anchor handler with default values.
        /// </summary>
        private void InitializeARAnchorHandler()
        {
            // Validate configuration
            if (_updateInterval < 0.01f)
            {
                Debug.LogWarning("Update interval must be at least 0.01 seconds. Setting to 0.01.");
                _updateInterval = 0.01f;
            }
            
            // Get AR camera if not assigned
            if (_arCamera == null)
            {
                _arCamera = Camera.main;
                if (_arCamera == null)
                {
                    Debug.LogWarning("AR Camera reference is missing. Using Camera.main as fallback.");
                }
            }
            
            // Get placement indicator renderer
            if (_placementIndicator != null)
            {
                _placementIndicatorRenderer = _placementIndicator.GetComponent<Renderer>();
            }
            
            // Set initial state
            IsPlacementValid = false;
            CurrentPlacementPosition = Vector3.zero;
            CurrentPlacementRotation = Quaternion.identity;
            _lastUpdateTime = 0f;
            
            // Set placement indicator visibility
            SetPlacementIndicatorVisible(_showPlacementIndicator);
            
            IsInitialized = true;
            Debug.Log("ARAnchorHandler initialized successfully.");
        }
        
        /// <summary>
        /// Updates the placement indicator with new position and validity.
        /// </summary>
        /// <param name="isValid">Whether the placement is valid</param>
        /// <param name="position">Placement position</param>
        /// <param name="rotation">Placement rotation</param>
        private void UpdatePlacementIndicator(bool isValid, Vector3 position, Quaternion rotation)
        {
            if (_placementIndicator == null)
            {
                return;
            }
            
            // Update position and rotation
            _placementIndicator.transform.position = position;
            _placementIndicator.transform.rotation = rotation;
            
            // Update visual feedback
            if (_placementIndicatorRenderer != null)
            {
                if (isValid)
                {
                    if (_validPlacementMaterial != null)
                    {
                        _placementIndicatorRenderer.material = _validPlacementMaterial;
                    }
                    else
                    {
                        _placementIndicatorRenderer.material.color = _validPlacementColor;
                    }
                }
                else
                {
                    if (_invalidPlacementMaterial != null)
                    {
                        _placementIndicatorRenderer.material = _invalidPlacementMaterial;
                    }
                    else
                    {
                        _placementIndicatorRenderer.material.color = _invalidPlacementColor;
                    }
                }
            }
        }
        
        #endregion

        #region Validation
        
        private void OnValidate()
        {
            // Ensure update interval is positive
            _updateInterval = Mathf.Max(0.01f, _updateInterval);
        }
        
        #endregion
    }
}