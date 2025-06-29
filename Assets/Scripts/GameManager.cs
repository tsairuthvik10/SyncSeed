using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public PlantSelectionManager plantSelectionManager;
    public GameObject plantPrefab;

    // UI references
    public GameObject sharePanel;
    public GameObject inputPanel;
    public UnityEngine.UI.InputField echoCodeInputField;
    public QRCodeManager qrCodeManager;

    private ARRaycastManager arRaycastManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
        CloseSharePanel();
        CloseInputPanel();
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (arRaycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = hits[0].pose;

                    PlantData selectedPlant = plantSelectionManager.GetSelectedPlant();

                    GameObject newPlant = Instantiate(plantPrefab, hitPose.position, Quaternion.identity);
                    Plant plantComponent = newPlant.GetComponent<Plant>();
                    plantComponent.Initialize(this, selectedPlant);
                }
            }
        }
    }

    // Called by UI Plant Button
    public void OnPlantButtonClicked()
    {
        plantSelectionManager.OpenSelectionPanel();
    }

    // Called when player taps Share Echo Code button
    public void OpenSharePanel(string echoCode)
    {
        sharePanel.SetActive(true);
        qrCodeManager.GenerateQRCode(echoCode);
    }

    public void CloseSharePanel()
    {
        sharePanel.SetActive(false);
    }

    // Called when player taps input Echo Code button
    public void OpenInputPanel()
    {
        inputPanel.SetActive(true);
        echoCodeInputField.text = "";
    }

    public void CloseInputPanel()
    {
        inputPanel.SetActive(false);
    }

    public void OnCopyEchoCode(string echoCode)
    {
        ClipboardManager.CopyToClipboard(echoCode);
    }

    public void OnPasteEchoCode()
    {
        string pastedCode = ClipboardManager.PasteFromClipboard();
        echoCodeInputField.text = pastedCode;
    }

    // Called when user applies the code from input field or scanned QR
    public void OnEchoCodeInput(string code)
    {
        EchoCodeManager.ApplyCode(code, plantPrefab, this, plantSelectionManager.allPlants);
        CloseInputPanel();
    }
}