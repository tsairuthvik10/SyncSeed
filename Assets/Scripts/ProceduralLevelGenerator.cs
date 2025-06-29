using UnityEngine;

public class ProceduralLevelGenerator : MonoBehaviour
{
    public static ProceduralLevelGenerator Instance;
    public GameObject rhythmNodePrefab;

    void Awake()
    {
        Instance = this;
    }

    public void GenerateLevel(int difficulty)
    {
        int nodeCount = Mathf.Clamp(difficulty + 2, 3, 10);
        for (int i = 0; i < nodeCount; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-1.5f, 1.5f), 0, Random.Range(1f, 3f));
            Instantiate(rhythmNodePrefab, pos, Quaternion.identity);
        }
    }
}