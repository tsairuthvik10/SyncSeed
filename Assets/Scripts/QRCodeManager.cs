using UnityEngine;
using ZXing;
using ZXing.QrCode;
using UnityEngine.Events;

namespace SyncSeed
{
    /// <summary>
    /// Manages QR code generation and decoding functionality.
    /// Provides utilities for creating and reading QR codes in the game.
    /// </summary>
    public class QRCodeManager : MonoBehaviour
    {
        #region Events
        
        [Header("QR Code Events")]
        public UnityEvent<Texture2D> OnQRCodeGenerated;
        public UnityEvent<string> OnQRCodeDecoded;
        public UnityEvent<string> OnQRCodeError;
        
        #endregion

        #region Serialized Fields
        
        [Header("QR Code Configuration")]
        [SerializeField] private int _defaultWidth = 256;
        [SerializeField] private int _defaultHeight = 256;
        [SerializeField] private int _defaultMargin = 1;
        [SerializeField] private BarcodeFormat _defaultFormat = BarcodeFormat.QR_CODE;
        
        [Header("Generation Settings")]
        [SerializeField] private bool _enableErrorCorrection = true;
        [SerializeField] private ErrorCorrectionLevel _errorCorrectionLevel = ErrorCorrectionLevel.M;
        [SerializeField] private bool _autoOptimizeSize = true;
        
        #endregion

        #region Properties
        
        public int DefaultWidth 
        { 
            get => _defaultWidth; 
            set => _defaultWidth = Mathf.Max(32, value); 
        }
        
        public int DefaultHeight 
        { 
            get => _defaultHeight; 
            set => _defaultHeight = Mathf.Max(32, value); 
        }
        
        public int DefaultMargin 
        { 
            get => _defaultMargin; 
            set => _defaultMargin = Mathf.Max(0, value); 
        }
        
        public BarcodeFormat DefaultFormat 
        { 
            get => _defaultFormat; 
            set => _defaultFormat = value; 
        }
        
        public bool EnableErrorCorrection 
        { 
            get => _enableErrorCorrection; 
            set => _enableErrorCorrection = value; 
        }
        
        public ErrorCorrectionLevel ErrorCorrectionLevel 
        { 
            get => _errorCorrectionLevel; 
            set => _errorCorrectionLevel = value; 
        }
        
        #endregion

        #region Unity Lifecycle
        
        private void Start()
        {
            ValidateConfiguration();
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Generates a QR code texture from the provided data.
        /// </summary>
        /// <param name="data">The data to encode in the QR code</param>
        /// <returns>Generated QR code texture or null if failed</returns>
        public Texture2D GenerateQRCode(string data)
        {
            return GenerateQRCode(data, _defaultWidth, _defaultHeight, _defaultMargin);
        }
        
        /// <summary>
        /// Generates a QR code texture with custom dimensions.
        /// </summary>
        /// <param name="data">The data to encode in the QR code</param>
        /// <param name="width">Width of the QR code</param>
        /// <param name="height">Height of the QR code</param>
        /// <param name="margin">Margin around the QR code</param>
        /// <returns>Generated QR code texture or null if failed</returns>
        public Texture2D GenerateQRCode(string data, int width, int height, int margin = 1)
        {
            if (string.IsNullOrEmpty(data))
            {
                Debug.LogWarning("Data cannot be null or empty. Cannot generate QR code.");
                OnQRCodeError?.Invoke("Data cannot be null or empty");
                return null;
            }
            
            if (width < 32 || height < 32)
            {
                Debug.LogWarning("QR code dimensions must be at least 32x32. Cannot generate QR code.");
                OnQRCodeError?.Invoke("QR code dimensions too small");
                return null;
            }
            
            try
            {
                var writer = new BarcodeWriter
                {
                    Format = _defaultFormat,
                    Options = new QrCodeEncodingOptions
                    {
                        Height = height,
                        Width = width,
                        Margin = margin,
                        ErrorCorrection = _enableErrorCorrection ? _errorCorrectionLevel : ErrorCorrectionLevel.L
                    }
                };
                
                Color32[] pixelData = writer.Write(data);
                Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                texture.SetPixels32(pixelData);
                texture.Apply();
                
                OnQRCodeGenerated?.Invoke(texture);
                Debug.Log($"QR code generated successfully with data: {data}");
                
                return texture;
            }
            catch (System.Exception e)
            {
                string errorMessage = $"Failed to generate QR code: {e.Message}";
                Debug.LogError(errorMessage);
                OnQRCodeError?.Invoke(errorMessage);
                return null;
            }
        }
        
        /// <summary>
        /// Generates a QR code texture optimized for the data size.
        /// </summary>
        /// <param name="data">The data to encode in the QR code</param>
        /// <returns>Generated QR code texture or null if failed</returns>
        public Texture2D GenerateOptimizedQRCode(string data)
        {
            if (!_autoOptimizeSize)
            {
                return GenerateQRCode(data);
            }
            
            // Calculate optimal size based on data length
            int optimalSize = CalculateOptimalSize(data);
            return GenerateQRCode(data, optimalSize, optimalSize);
        }
        
        /// <summary>
        /// Decodes a QR code from a texture.
        /// </summary>
        /// <param name="qrTexture">The texture containing the QR code</param>
        /// <returns>Decoded string or null if failed</returns>
        public string DecodeQRCode(Texture2D qrTexture)
        {
            if (qrTexture == null)
            {
                Debug.LogWarning("QR texture is null. Cannot decode QR code.");
                OnQRCodeError?.Invoke("QR texture is null");
                return null;
            }
            
            try
            {
                var reader = new BarcodeReader();
                var result = reader.Decode(qrTexture.GetPixels32(), qrTexture.width, qrTexture.height);
                
                if (result != null && !string.IsNullOrEmpty(result.Text))
                {
                    OnQRCodeDecoded?.Invoke(result.Text);
                    Debug.Log($"QR code decoded successfully: {result.Text}");
                    return result.Text;
                }
                else
                {
                    Debug.LogWarning("No valid QR code found in texture.");
                    OnQRCodeError?.Invoke("No valid QR code found");
                    return null;
                }
            }
            catch (System.Exception e)
            {
                string errorMessage = $"Failed to decode QR code: {e.Message}";
                Debug.LogError(errorMessage);
                OnQRCodeError?.Invoke(errorMessage);
                return null;
            }
        }
        
        /// <summary>
        /// Decodes a QR code from a texture asynchronously.
        /// </summary>
        /// <param name="qrTexture">The texture containing the QR code</param>
        /// <param name="callback">Callback with the decoded result</param>
        public void DecodeQRCodeAsync(Texture2D qrTexture, System.Action<string> callback)
        {
            if (qrTexture == null)
            {
                Debug.LogWarning("QR texture is null. Cannot decode QR code.");
                OnQRCodeError?.Invoke("QR texture is null");
                callback?.Invoke(null);
                return;
            }
            
            // Run decoding in a separate thread to avoid blocking the main thread
            System.Threading.Tasks.Task.Run(() => {
                try
                {
                    var reader = new BarcodeReader();
                    var result = reader.Decode(qrTexture.GetPixels32(), qrTexture.width, qrTexture.height);
                    
                    // Return to main thread for callback
                    UnityEngine.MonoBehaviour.print($"QR code decoded: {result?.Text}");
                    callback?.Invoke(result?.Text);
                }
                catch (System.Exception e)
                {
                    string errorMessage = $"Failed to decode QR code: {e.Message}";
                    Debug.LogError(errorMessage);
                    OnQRCodeError?.Invoke(errorMessage);
                    callback?.Invoke(null);
                }
            });
        }
        
        /// <summary>
        /// Sets the default QR code dimensions.
        /// </summary>
        /// <param name="width">Default width</param>
        /// <param name="height">Default height</param>
        public void SetDefaultDimensions(int width, int height)
        {
            DefaultWidth = width;
            DefaultHeight = height;
        }
        
        /// <summary>
        /// Sets the default margin for QR codes.
        /// </summary>
        /// <param name="margin">Default margin</param>
        public void SetDefaultMargin(int margin)
        {
            DefaultMargin = margin;
        }
        
        /// <summary>
        /// Sets the default barcode format.
        /// </summary>
        /// <param name="format">Default format</param>
        public void SetDefaultFormat(BarcodeFormat format)
        {
            DefaultFormat = format;
        }
        
        /// <summary>
        /// Enables or disables error correction.
        /// </summary>
        /// <param name="enabled">Whether to enable error correction</param>
        public void SetErrorCorrectionEnabled(bool enabled)
        {
            EnableErrorCorrection = enabled;
        }
        
        /// <summary>
        /// Sets the error correction level.
        /// </summary>
        /// <param name="level">Error correction level</param>
        public void SetErrorCorrectionLevel(ErrorCorrectionLevel level)
        {
            ErrorCorrectionLevel = level;
        }
        
        /// <summary>
        /// Enables or disables automatic size optimization.
        /// </summary>
        /// <param name="enabled">Whether to enable auto optimization</param>
        public void SetAutoOptimizeSize(bool enabled)
        {
            _autoOptimizeSize = enabled;
        }
        
        /// <summary>
        /// Tests QR code generation with sample data.
        /// </summary>
        /// <returns>Generated test QR code texture</returns>
        public Texture2D TestQRCodeGeneration()
        {
            string testData = "SyncSeed Test QR Code - " + System.DateTime.Now.ToString();
            return GenerateQRCode(testData);
        }
        
        #endregion

        #region Private Methods
        
        /// <summary>
        /// Validates the QR code manager configuration.
        /// </summary>
        private void ValidateConfiguration()
        {
            if (_defaultWidth < 32)
            {
                Debug.LogWarning("Default width must be at least 32. Setting to 32.");
                _defaultWidth = 32;
            }
            
            if (_defaultHeight < 32)
            {
                Debug.LogWarning("Default height must be at least 32. Setting to 32.");
                _defaultHeight = 32;
            }
            
            if (_defaultMargin < 0)
            {
                Debug.LogWarning("Default margin cannot be negative. Setting to 0.");
                _defaultMargin = 0;
            }
            
            Debug.Log("QRCodeManager configuration validated.");
        }
        
        /// <summary>
        /// Calculates the optimal QR code size based on data length.
        /// </summary>
        /// <param name="data">The data to encode</param>
        /// <returns>Optimal size for the QR code</returns>
        private int CalculateOptimalSize(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return _defaultWidth;
            }
            
            // Simple heuristic: base size on data length
            int baseSize = 128;
            int sizeIncrement = 64;
            int maxSize = 512;
            
            int optimalSize = baseSize + (data.Length / 10) * sizeIncrement;
            optimalSize = Mathf.Clamp(optimalSize, baseSize, maxSize);
            
            // Ensure size is a multiple of 8 for better QR code generation
            optimalSize = (optimalSize / 8) * 8;
            
            return optimalSize;
        }
        
        #endregion

        #region Validation
        
        private void OnValidate()
        {
            // Ensure dimensions are at least 32
            _defaultWidth = Mathf.Max(32, _defaultWidth);
            _defaultHeight = Mathf.Max(32, _defaultHeight);
            
            // Ensure margin is non-negative
            _defaultMargin = Mathf.Max(0, _defaultMargin);
        }
        
        #endregion
    }
    
    /// <summary>
    /// Error correction levels for QR codes.
    /// </summary>
    public enum ErrorCorrectionLevel
    {
        L = 0, // Low - 7% recovery capacity
        M = 1, // Medium - 15% recovery capacity
        Q = 2, // Quartile - 25% recovery capacity
        H = 3  // High - 30% recovery capacity
    }
}