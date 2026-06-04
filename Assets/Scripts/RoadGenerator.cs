using UnityEngine;
using System.Collections.Generic;

public class RoadGenerator : MonoBehaviour
{
    [Header("Road Settings")]
    public GameObject roadChunkPrefab;
    public int chunksVisible = 7;
    public float chunkLength = 50f;

    [Header("References")]
    public Transform player;

    private List<GameObject> activeChunks = new List<GameObject>();
    private float nextSpawnZ = 0f;
    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        nextSpawnZ = -(chunkLength / 2f);

        for (int i = 0; i < chunksVisible; i++)
        {
            SpawnChunk();
        }
    }
    void Update()
    {
        if (player == null) return;

        PlayerCarController pc = player.GetComponent<PlayerCarController>();
        if (pc != null && !pc.isAlive) return;

        while (nextSpawnZ < player.position.z + (chunksVisible * chunkLength))
        {
            SpawnChunk();
            DeleteOldChunk();
        }
    }
    void SpawnChunk()
    {
        if (roadChunkPrefab == null)
        {
            Debug.LogError("Road Chunk Prefab is missing!");
            return;
        }
        Vector3 spawnPosition = new Vector3(
            0,
            0,
            nextSpawnZ + (chunkLength / 2f)
        );
        GameObject chunk = Instantiate(
            roadChunkPrefab,
            spawnPosition,
            Quaternion.identity
        );
        chunk.name = "RoadChunk_" + activeChunks.Count;
        activeChunks.Add(chunk);
        nextSpawnZ += chunkLength;
    }

    void DeleteOldChunk()
    {
        if (activeChunks.Count > chunksVisible + 2)
        {
            GameObject oldChunk = activeChunks[0];
            activeChunks.RemoveAt(0);
            Destroy(oldChunk);
        }
    }
}