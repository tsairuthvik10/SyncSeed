using UnityEngine;
using ZXing;
using ZXing.QrCode;

public class QRCodeManager : MonoBehaviour
{
    public Texture2D GenerateQRCode(string data)
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

        Color32[] pixelData = writer.Write(data);
        Texture2D texture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
        texture.SetPixels32(pixelData);
        texture.Apply();
        return texture;
    }

    public string DecodeQRCode(Texture2D qrTexture)
    {
        var reader = new BarcodeReader();
        var result = reader.Decode(qrTexture.GetPixels32(), qrTexture.width, qrTexture.height);
        return result?.Text;
    }
}