using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public Transform player;
    public float spawnDistance = 60f;
    public float spawnInterval = 5f;
    public int maxEnemies = 3;

    [Header("Road Settings")]
    public float roadWidth = 5f;

    [Header("Wave Settings")]
    public float waveDuration = 120f;
    private int currentWave = 1;
    private float waveTimer = 0f;

    private float spawnTimer = 0f;
    private int currentEnemyCount = 0;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        PlayerCarController pc = player.GetComponent<PlayerCarController>();
        if (pc != null && !pc.isAlive) return;

        // Wave timer
        waveTimer += Time.deltaTime;
        if (waveTimer >= waveDuration)
        {
            waveTimer = 0;
            IncreaseWave();
        }

        // Spawn timer
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval && currentEnemyCount < maxEnemies)
        {
            spawnTimer = 0;
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        float randomX = Random.Range(-roadWidth, roadWidth);
        Vector3 spawnPos = new Vector3(
            randomX,
            1f,
            player.position.z + spawnDistance
        );

        GameObject enemy = Instantiate(
            enemyPrefab, spawnPos, Quaternion.identity);

        enemy.transform.rotation = Quaternion.Euler(0, 180, 0);
        currentEnemyCount++;

        // Scale with wave - using correct variable names
        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.chaseSpeed = 25f + (currentWave * 3f);
            ai.health = 50f + (currentWave * 10f);
            ai.ramDamage = 15f + (currentWave * 2f);
        }

        StartCoroutine(CleanupEnemy(enemy));
    }

    void IncreaseWave()
    {
        currentWave++;
        maxEnemies += 1;
        spawnInterval -= 0.5f;
        spawnInterval = Mathf.Max(spawnInterval, 1f);
        Debug.Log("WAVE " + currentWave + "! Enemies: " + maxEnemies);
    }

    IEnumerator CleanupEnemy(GameObject enemy)
    {
        yield return new WaitForSeconds(30f);
        if (enemy != null)
        {
            currentEnemyCount--;
            Destroy(enemy);
        }
    }
}