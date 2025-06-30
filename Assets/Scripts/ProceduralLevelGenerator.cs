using UnityEngine;

public class ProceduralLevelGenerator : MonoBehaviour
{
    public GameObject rhythmNodePrefab;
    public Transform spawnRoot;
    public int baseNodeCount = 3;
    public float baseBeatInterval = 2.0f;

    public int currentLevel = 1;

    public void GenerateLevel()
    {
        int nodeCount = baseNodeCount + currentLevel;
        float beatInterval = Mathf.Max(0.5f, baseBeatInterval - currentLevel * 0.1f);

        for (int i = 0; i < nodeCount; i++)
        {
            Vector3 position = spawnRoot.position + Random.insideUnitSphere * 1.5f;
            position.y = spawnRoot.position.y;
            Instantiate(rhythmNodePrefab, position, Quaternion.identity, spawnRoot);
        }

        RhythmManager.Instance.SetBeatInterval(beatInterval);
        MirrorManager.Instance.SetMirrorLimit(Mathf.Max(1, 3 - currentLevel / 2));
    }
}