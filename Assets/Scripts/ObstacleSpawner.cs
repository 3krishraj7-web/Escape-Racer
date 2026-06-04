using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject obstaclePrefab;
    public Transform player;
    public float spawnDistance = 70f;
    public float spawnInterval = 3f;
    public float roadWidth = 4f;
    public int maxObstacles = 5;
    private float spawnTimer = 0f;
    private int obstacleCount = 0;
    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    void Update()
    {
        if (player == null) return;
        PlayerCarController pc = player.GetComponent<PlayerCarController>();
        if (pc != null && !pc.isAlive) return;
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval && obstacleCount < maxObstacles)
        {
            spawnTimer = 0;
            SpawnObstacle();
        }
    }
    void SpawnObstacle()
    {
        float randomX = Random.Range(-roadWidth, roadWidth);
        Vector3 spawnPos = new Vector3(
            randomX,
            1f,
            player.position.z + spawnDistance
        );
        GameObject obs = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
        obstacleCount++;
        StartCoroutine(CleanupObstacle(obs));
    }
    System.Collections.IEnumerator CleanupObstacle(GameObject obs)
    {
        yield return new WaitForSeconds(20f);
        if (obs != null)
        {
            obstacleCount--;
            Destroy(obs);
        }
    }
}