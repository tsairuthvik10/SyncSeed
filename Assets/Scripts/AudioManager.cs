using UnityEngine;
using UnityEngine.Events;

namespace SyncSeed
{
    /// <summary>
    /// Manages audio playback and sound effects for the game.
    /// Provides centralized audio control with volume management and sound categorization.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        #region Singleton Implementation
        
        public static AudioManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioManager();
            }
            else if (Instance != this)
            {
                Debug.LogWarning("Multiple AudioManager instances detected. Destroying duplicate.");
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
        
        [Header("Audio Events")]
        public UnityEvent<AudioClip> OnSoundPlayed;
        public UnityEvent<float> OnMasterVolumeChanged;
        public UnityEvent<float> OnSFXVolumeChanged;
        public UnityEvent<float> OnMusicVolumeChanged;
        public UnityEvent OnAudioMuted;
        public UnityEvent OnAudioUnmuted;
        
        #endregion

        #region Serialized Fields
        
        [Header("Audio Sources")]
        [SerializeField] private AudioSource _masterAudioSource;
        [SerializeField] private AudioSource _sfxAudioSource;
        [SerializeField] private AudioSource _musicAudioSource;
        
        [Header("Sound Effects")]
        [SerializeField] private AudioClip _rhythmHitClip;
        [SerializeField] private AudioClip _levelCompleteClip;
        [SerializeField] private AudioClip _gameOverClip;
        [SerializeField] private AudioClip _buttonClickClip;
        
        [Header("Volume Settings")]
        [SerializeField] private float _masterVolume = 1.0f;
        [SerializeField] private float _sfxVolume = 1.0f;
        [SerializeField] private float _musicVolume = 0.8f;
        [SerializeField] private bool _isMuted = false;
        
        [Header("Configuration")]
        [SerializeField] private bool _autoInitializeAudioSources = true;
        [SerializeField] private bool _persistVolumeSettings = true;
        
        #endregion

        #region Properties
        
        public AudioSource MasterAudioSource 
        { 
            get => _masterAudioSource; 
            set => _masterAudioSource = value; 
        }
        
        public AudioSource SFXAudioSource 
        { 
            get => _sfxAudioSource; 
            set => _sfxAudioSource = value; 
        }
        
        public AudioSource MusicAudioSource 
        { 
            get => _musicAudioSource; 
            set => _musicAudioSource = value; 
        }
        
        public float MasterVolume 
        { 
            get => _masterVolume; 
            set 
            {
                _masterVolume = Mathf.Clamp01(value);
                UpdateMasterVolume();
                OnMasterVolumeChanged?.Invoke(_masterVolume);
            }
        }
        
        public float SFXVolume 
        { 
            get => _sfxVolume; 
            set 
            {
                _sfxVolume = Mathf.Clamp01(value);
                UpdateSFXVolume();
                OnSFXVolumeChanged?.Invoke(_sfxVolume);
            }
        }
        
        public float MusicVolume 
        { 
            get => _musicVolume; 
            set 
            {
                _musicVolume = Mathf.Clamp01(value);
                UpdateMusicVolume();
                OnMusicVolumeChanged?.Invoke(_musicVolume);
            }
        }
        
        public bool IsMuted 
        { 
            get => _isMuted; 
            set 
            {
                if (_isMuted != value)
                {
                    _isMuted = value;
                    UpdateMuteState();
                }
            }
        }
        
        public bool IsInitialized { get; private set; }
        
        #endregion

        #region Unity Lifecycle
        
        private void Start()
        {
            if (Instance == this)
            {
                LoadVolumeSettings();
            }
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Plays a sound effect using the SFX audio source.
        /// </summary>
        /// <param name="clip">The audio clip to play</param>
        public void PlaySound(AudioClip clip)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("AudioManager not initialized. Cannot play sound.");
                return;
            }
            
            if (clip == null)
            {
                Debug.LogWarning("Audio clip is null. Cannot play sound.");
                return;
            }
            
            if (_sfxAudioSource != null)
            {
                _sfxAudioSource.PlayOneShot(clip);
                OnSoundPlayed?.Invoke(clip);
                Debug.Log($"Playing sound: {clip.name}");
            }
            else
            {
                Debug.LogWarning("SFX AudioSource is null. Cannot play sound.");
            }
        }
        
        /// <summary>
        /// Plays the rhythm hit sound effect.
        /// </summary>
        public void PlayRhythmHitSound()
        {
            PlaySound(_rhythmHitClip);
        }
        
        /// <summary>
        /// Plays the level complete sound effect.
        /// </summary>
        public void PlayLevelCompleteSound()
        {
            PlaySound(_levelCompleteClip);
        }
        
        /// <summary>
        /// Plays the game over sound effect.
        /// </summary>
        public void PlayGameOverSound()
        {
            PlaySound(_gameOverClip);
        }
        
        /// <summary>
        /// Plays the button click sound effect.
        /// </summary>
        public void PlayButtonClickSound()
        {
            PlaySound(_buttonClickClip);
        }
        
        /// <summary>
        /// Plays background music using the music audio source.
        /// </summary>
        /// <param name="clip">The music clip to play</param>
        /// <param name="loop">Whether to loop the music</param>
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("AudioManager not initialized. Cannot play music.");
                return;
            }
            
            if (clip == null)
            {
                Debug.LogWarning("Music clip is null. Cannot play music.");
                return;
            }
            
            if (_musicAudioSource != null)
            {
                _musicAudioSource.clip = clip;
                _musicAudioSource.loop = loop;
                _musicAudioSource.Play();
                OnSoundPlayed?.Invoke(clip);
                Debug.Log($"Playing music: {clip.name}");
            }
            else
            {
                Debug.LogWarning("Music AudioSource is null. Cannot play music.");
            }
        }
        
        /// <summary>
        /// Stops the currently playing music.
        /// </summary>
        public void StopMusic()
        {
            if (_musicAudioSource != null)
            {
                _musicAudioSource.Stop();
                Debug.Log("Music stopped.");
            }
        }
        
        /// <summary>
        /// Pauses the currently playing music.
        /// </summary>
        public void PauseMusic()
        {
            if (_musicAudioSource != null)
            {
                _musicAudioSource.Pause();
                Debug.Log("Music paused.");
            }
        }
        
        /// <summary>
        /// Resumes the paused music.
        /// </summary>
        public void ResumeMusic()
        {
            if (_musicAudioSource != null)
            {
                _musicAudioSource.UnPause();
                Debug.Log("Music resumed.");
            }
        }
        
        /// <summary>
        /// Sets the master volume level.
        /// </summary>
        /// <param name="volume">Volume level (0-1)</param>
        public void SetMasterVolume(float volume)
        {
            MasterVolume = volume;
        }
        
        /// <summary>
        /// Sets the SFX volume level.
        /// </summary>
        /// <param name="volume">Volume level (0-1)</param>
        public void SetSFXVolume(float volume)
        {
            SFXVolume = volume;
        }
        
        /// <summary>
        /// Sets the music volume level.
        /// </summary>
        /// <param name="volume">Volume level (0-1)</param>
        public void SetMusicVolume(float volume)
        {
            MusicVolume = volume;
        }
        
        /// <summary>
        /// Mutes or unmutes all audio.
        /// </summary>
        /// <param name="muted">Whether to mute audio</param>
        public void SetMuted(bool muted)
        {
            IsMuted = muted;
        }
        
        /// <summary>
        /// Toggles the mute state.
        /// </summary>
        public void ToggleMute()
        {
            IsMuted = !IsMuted;
        }
        
        /// <summary>
        /// Sets a specific audio clip for a sound type.
        /// </summary>
        /// <param name="soundType">The type of sound to set</param>
        /// <param name="clip">The audio clip to use</param>
        public void SetSoundClip(SoundType soundType, AudioClip clip)
        {
            switch (soundType)
            {
                case SoundType.RhythmHit:
                    _rhythmHitClip = clip;
                    break;
                case SoundType.LevelComplete:
                    _levelCompleteClip = clip;
                    break;
                case SoundType.GameOver:
                    _gameOverClip = clip;
                    break;
                case SoundType.ButtonClick:
                    _buttonClickClip = clip;
                    break;
                default:
                    Debug.LogWarning($"Unknown sound type: {soundType}");
                    break;
            }
        }
        
        #endregion

        #region Private Methods
        
        /// <summary>
        /// Initializes the audio manager with default values and audio sources.
        /// </summary>
        private void InitializeAudioManager()
        {
            if (_autoInitializeAudioSources)
            {
                InitializeAudioSources();
            }
            
            // Validate volume settings
            _masterVolume = Mathf.Clamp01(_masterVolume);
            _sfxVolume = Mathf.Clamp01(_sfxVolume);
            _musicVolume = Mathf.Clamp01(_musicVolume);
            
            IsInitialized = true;
            Debug.Log("AudioManager initialized successfully.");
        }
        
        /// <summary>
        /// Initializes audio sources if they are not already assigned.
        /// </summary>
        private void InitializeAudioSources()
        {
            if (_masterAudioSource == null)
            {
                _masterAudioSource = GetComponent<AudioSource>();
                if (_masterAudioSource == null)
                {
                    _masterAudioSource = gameObject.AddComponent<AudioSource>();
                }
            }
            
            if (_sfxAudioSource == null)
            {
                GameObject sfxObject = new GameObject("SFX AudioSource");
                sfxObject.transform.SetParent(transform);
                _sfxAudioSource = sfxObject.AddComponent<AudioSource>();
                _sfxAudioSource.playOnAwake = false;
            }
            
            if (_musicAudioSource == null)
            {
                GameObject musicObject = new GameObject("Music AudioSource");
                musicObject.transform.SetParent(transform);
                _musicAudioSource = musicObject.AddComponent<AudioSource>();
                _musicAudioSource.playOnAwake = false;
                _musicAudioSource.loop = true;
            }
        }
        
        /// <summary>
        /// Updates the master volume on all audio sources.
        /// </summary>
        private void UpdateMasterVolume()
        {
            float effectiveVolume = _isMuted ? 0f : _masterVolume;
            
            if (_masterAudioSource != null)
            {
                _masterAudioSource.volume = effectiveVolume;
            }
            
            if (_sfxAudioSource != null)
            {
                _sfxAudioSource.volume = effectiveVolume * _sfxVolume;
            }
            
            if (_musicAudioSource != null)
            {
                _musicAudioSource.volume = effectiveVolume * _musicVolume;
            }
        }
        
        /// <summary>
        /// Updates the SFX volume.
        /// </summary>
        private void UpdateSFXVolume()
        {
            if (_sfxAudioSource != null)
            {
                _sfxAudioSource.volume = (_isMuted ? 0f : _masterVolume) * _sfxVolume;
            }
        }
        
        /// <summary>
        /// Updates the music volume.
        /// </summary>
        private void UpdateMusicVolume()
        {
            if (_musicAudioSource != null)
            {
                _musicAudioSource.volume = (_isMuted ? 0f : _masterVolume) * _musicVolume;
            }
        }
        
        /// <summary>
        /// Updates the mute state of all audio sources.
        /// </summary>
        private void UpdateMuteState()
        {
            UpdateMasterVolume();
            
            if (_isMuted)
            {
                OnAudioMuted?.Invoke();
            }
            else
            {
                OnAudioUnmuted?.Invoke();
            }
        }
        
        /// <summary>
        /// Loads volume settings from PlayerPrefs if persistence is enabled.
        /// </summary>
        private void LoadVolumeSettings()
        {
            if (_persistVolumeSettings)
            {
                _masterVolume = PlayerPrefs.GetFloat("MasterVolume", _masterVolume);
                _sfxVolume = PlayerPrefs.GetFloat("SFXVolume", _sfxVolume);
                _musicVolume = PlayerPrefs.GetFloat("MusicVolume", _musicVolume);
                _isMuted = PlayerPrefs.GetInt("IsMuted", _isMuted ? 1 : 0) == 1;
                
                UpdateMasterVolume();
            }
        }
        
        /// <summary>
        /// Saves volume settings to PlayerPrefs if persistence is enabled.
        /// </summary>
        private void SaveVolumeSettings()
        {
            if (_persistVolumeSettings)
            {
                PlayerPrefs.SetFloat("MasterVolume", _masterVolume);
                PlayerPrefs.SetFloat("SFXVolume", _sfxVolume);
                PlayerPrefs.SetFloat("MusicVolume", _musicVolume);
                PlayerPrefs.SetInt("IsMuted", _isMuted ? 1 : 0);
                PlayerPrefs.Save();
            }
        }
        
        #endregion

        #region Validation
        
        private void OnValidate()
        {
            // Ensure volume values are in valid range
            _masterVolume = Mathf.Clamp01(_masterVolume);
            _sfxVolume = Mathf.Clamp01(_sfxVolume);
            _musicVolume = Mathf.Clamp01(_musicVolume);
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveVolumeSettings();
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                SaveVolumeSettings();
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Enumeration of sound types for easy reference.
    /// </summary>
    public enum SoundType
    {
        RhythmHit,
        LevelComplete,
        GameOver,
        ButtonClick
    }
}