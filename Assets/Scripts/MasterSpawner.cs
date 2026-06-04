using UnityEngine;
using System.Collections;

public class MasterSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject enemyPrefab;
    public GameObject trafficPrefab;
    public GameObject fuelCanPrefab;
    public GameObject shieldPrefab;
    public GameObject obstaclePrefab;

    [Header("Player")]
    public Transform player;

    [Header("Spawn Distances")]
    public float spawnAhead = 70f;
    public float roadWidth = 5f;

    [Header("Spawn Intervals")]
    public float enemyInterval = 6f;
    public float trafficInterval = 3f;
    public float fuelInterval = 8f;
    public float shieldInterval = 15f;
    public float obstacleInterval = 4f;

    [Header("Wave System")]
    public int currentWave = 1;
    public float waveDuration = 120f;
    public int maxEnemies = 3;

    // Timers
    private float enemyTimer = 0f;
    private float trafficTimer = 0f;
    private float fuelTimer = 0f;
    private float shieldTimer = 0f;
    private float obstacleTimer = 0f;
    private float waveTimer = 0f;

    // Counts
    private int enemyCount = 0;

    private GameUI gameUI;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        gameUI = FindObjectOfType<GameUI>();
    }

    void Update()
    {
        if (player == null) return;

        PlayerCarController pc = player.GetComponent<PlayerCarController>();
        if (pc != null && !pc.isAlive) return;

        // Update timers
        enemyTimer += Time.deltaTime;
        trafficTimer += Time.deltaTime;
        fuelTimer += Time.deltaTime;
        shieldTimer += Time.deltaTime;
        obstacleTimer += Time.deltaTime;
        waveTimer += Time.deltaTime;

        // Spawn enemy
        if (enemyTimer >= enemyInterval && enemyCount < maxEnemies)
        {
            enemyTimer = 0f;
            SpawnEnemy();
        }

        // Spawn traffic
        if (trafficTimer >= trafficInterval)
        {
            trafficTimer = 0f;
            SpawnTraffic();
        }

        // Spawn fuel can
        if (fuelTimer >= fuelInterval)
        {
            fuelTimer = 0f;
            SpawnPickup(fuelCanPrefab);
        }

        // Spawn shield
        if (shieldTimer >= shieldInterval)
        {
            shieldTimer = 0f;
            SpawnPickup(shieldPrefab);
        }

        // Spawn obstacle
        if (obstacleTimer >= obstacleInterval)
        {
            obstacleTimer = 0f;
            SpawnObstacle();
        }

        // Wave increase
        if (waveTimer >= waveDuration)
        {
            waveTimer = 0f;
            IncreaseWave();
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("Enemy Prefab not assigned!");
            return;
        }

        float randomX = Random.Range(-roadWidth, roadWidth);
        Vector3 pos = new Vector3(
            randomX,
            1f,
            player.position.z + spawnAhead
        );

        GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
        enemyCount++;

        // Scale enemy with wave number
        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.chaseSpeed = 25f + (currentWave * 3f);
            ai.health = 50f + (currentWave * 15f);
            ai.ramDamage = 15f + (currentWave * 2f);
        }

        StartCoroutine(TrackEnemy(enemy));
    }

    void SpawnTraffic()
    {
        if (trafficPrefab == null)
        {
            Debug.LogWarning("Traffic Prefab not assigned!");
            return;
        }

        float randomX = Random.Range(-roadWidth, roadWidth);
        bool oncoming = Random.Range(0, 2) == 0;

        float zOffset = oncoming ? spawnAhead : spawnAhead * 0.5f;
        Vector3 pos = new Vector3(
            randomX,
            1f,
            player.position.z + zOffset
        );

        GameObject traffic = Instantiate(trafficPrefab, pos, Quaternion.identity);

        TrafficCar tc = traffic.GetComponent<TrafficCar>();
        if (tc != null)
        {
            tc.isOncoming = oncoming;
        }
    }

    void SpawnPickup(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Pickup Prefab not assigned!");
            return;
        }

        float randomX = Random.Range(-roadWidth + 1f, roadWidth - 1f);
        Vector3 pos = new Vector3(
            randomX,
            1.5f,
            player.position.z + (spawnAhead * 0.8f)
        );

        Instantiate(prefab, pos, Quaternion.identity);
    }

    void SpawnObstacle()
    {
        if (obstaclePrefab == null)
        {
            Debug.LogWarning("Obstacle Prefab not assigned!");
            return;
        }

        float randomX = Random.Range(-roadWidth + 1f, roadWidth - 1f);
        Vector3 pos = new Vector3(
            randomX,
            1f,
            player.position.z + spawnAhead
        );

        Instantiate(obstaclePrefab, pos, Quaternion.identity);
    }

    void IncreaseWave()
    {
        currentWave++;
        maxEnemies++;

        // Increase difficulty
        enemyInterval = Mathf.Max(enemyInterval - 0.5f, 2f);
        trafficInterval = Mathf.Max(trafficInterval - 0.3f, 1f);
        obstacleInterval = Mathf.Max(obstacleInterval - 0.3f, 1f);

        Debug.Log("WAVE " + currentWave + " STARTED!");

        if (gameUI != null)
        {
            gameUI.ShowWave(currentWave);
        }
    }

    IEnumerator TrackEnemy(GameObject enemy)
    {
        yield return new WaitForSeconds(30f);

        if (enemy != null)
        {
            enemyCount--;
            Destroy(enemy);
        }
    }
}