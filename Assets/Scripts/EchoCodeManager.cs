using UnityEngine;
using System.Collections.Generic;

public static class EchoCodeManager
{
    public static string GenerateCode(Vector3 position, string plantID, float pitch)
    {
        int x = Mathf.RoundToInt(position.x * 100);
        int z = Mathf.RoundToInt(position.z * 100);
        int p = Mathf.RoundToInt(pitch * 100);

        return plantID + "-" + x.ToString("X") + "-" + z.ToString("X") + "-" + p.ToString();
    }

    public static void ApplyCode(string code, GameObject plantPrefab, GameManager manager, List<PlantData> allPlantTypes)
    {
        string[] parts = code.Split('-');
        if(parts.Length != 4) {
            Debug.LogWarning("Invalid Echo Code format");
            return;
        }
        string plantID = parts[0];

        float x = int.Parse(parts[1], System.Globalization.NumberStyles.HexNumber) / 100f;
        float z = int.Parse(parts[2], System.Globalization.NumberStyles.HexNumber) / 100f;
        float pitch = int.Parse(parts[3]) / 100f;

        PlantData matchedData = allPlantTypes.Find(p => p.plantID == plantID);
        if (matchedData == null)
        {
            Debug.LogWarning("No matching plant data found for ID: " + plantID);
            return;
        }

        Vector3 spawnPos = new Vector3(x, 0, z);
        GameObject plant = GameObject.Instantiate(plantPrefab, spawnPos, Quaternion.identity);

        Plant plantComponent = plant.GetComponent<Plant>();
        plantComponent.Initialize(manager, matchedData);
        plant.GetComponent<AudioSource>().pitch = pitch;
    }
}