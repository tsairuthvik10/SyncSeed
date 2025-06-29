using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;
using ZXing.Common;
using System.Collections;
using UnityEngine.Android;

public class QRCodeManager : MonoBehaviour
{
    public RawImage qrCodeImage;
    public GameObject cameraScanPanel;
    public AspectRatioFitter qrCodeAspectRatioFitter;

    private WebCamTexture webCamTexture;
    private bool isScanning = false;

    // Generate and display QR code texture from input string
    public void GenerateQRCode(string codeText)
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = 256,
                Width = 256,
                Margin = 1
            }
        };
        var colorArray = writer.Write(codeText);
        Texture2D qrTexture = new Texture2D(256, 256);
        qrTexture.SetPixels32(colorArray);
        qrTexture.Apply();
        Texture2D texture = qrTexture;
        qrCodeImage.texture = texture;
        qrCodeAspectRatioFitter.aspectRatio = (float)texture.width / texture.height;
    }

    // Start camera and scan for QR codes
    public void StartScanning()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
            return;
        }

        if (webCamTexture == null)
        {
            webCamTexture = new WebCamTexture();
        }

        webCamTexture.Play();
        cameraScanPanel.SetActive(true);
        isScanning = true;
        StartCoroutine(Scan());
    }

    public void StopScanning()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
            webCamTexture.Stop();

        cameraScanPanel.SetActive(false);
        isScanning = false;
        StopAllCoroutines();
    }

    private IEnumerator Scan()
    {
        IBarcodeReader barcodeReader = new BarcodeReader();

        while (isScanning)
        {
            try
            {
                if (webCamTexture.width > 100 && webCamTexture.height > 100)
                {
                    var snap = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false);
                    snap.SetPixels32(webCamTexture.GetPixels32());
                    snap.Apply();

                    var result = barcodeReader.Decode(snap.GetPixels32(), snap.width, snap.height);

                    if (result != null)
                    {
                        Debug.Log("QR Code Detected: " + result.Text);
                        OnQRCodeDetected(result.Text);
                        StopScanning();
                    }
                }
            }
            catch
            {
                // ignore errors
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    // Event when QR code is detected
    private void OnQRCodeDetected(string code)
    {
        // Pass code to GameManager or EchoCodeManager input handler
        // Example (assuming GameManager has a public method to handle input code):
        FindObjectOfType<GameManager>().OnEchoCodeInput(code);
    }
}