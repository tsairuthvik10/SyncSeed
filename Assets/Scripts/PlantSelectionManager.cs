using System.Collections.Generic;
using UnityEngine;

public class PlantSelectionManager : MonoBehaviour
{
    public List<PlantData> allPlants;         // Assign in inspector with all your PlantData assets
    public int selectedPlantIndex = 0;

    public GameObject selectionPanel;         // UI Panel containing plant icons/buttons

    // Call to show the selection UI
    public void OpenSelectionPanel()
    {
        selectionPanel.SetActive(true);
        // Optional: populate icons dynamically if needed
    }

    // Called by UI buttons when player picks a plant
    public void SelectPlant(int index)
    {
        if (index >= 0 && index < allPlants.Count)
        {
            selectedPlantIndex = index;
            selectionPanel.SetActive(false);
        }
    }

    public PlantData GetSelectedPlant()
    {
        return allPlants[selectedPlantIndex];
    }
}