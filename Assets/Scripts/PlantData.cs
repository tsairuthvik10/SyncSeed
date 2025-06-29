using UnityEngine;

[CreateAssetMenu(fileName = "PlantData", menuName = "ScriptableObjects/PlantData", order = 1)]
public class PlantData : ScriptableObject
{
    public string plantName;
    public float requiredWalkDistance;
    public int rhythmDifficulty;
    public float growthMultiplier;
    public float soundPitch;
    public Color plantColor;
    public string plantID;
}