using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawnerManager : NetworkBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints; // Optional: you can define where enemies appear

    [SerializeField] private int totalEnemiesToSpawn = 7;
    [SerializeField] private float spawnInterval = 15f;

    private int spawnedCount = 0;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(SpawnEnemiesOverTime());
        }
    }

    private IEnumerator SpawnEnemiesOverTime()
    {
        while (spawnedCount < totalEnemiesToSpawn)
        {
            SpawnEnemy();
            spawnedCount++;

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        Vector3 spawnPosition;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            spawnPosition = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        }
        else
        {
            spawnPosition = transform.position + Random.insideUnitSphere * 5f;
            spawnPosition.y = 0f; // optional: flatten Y
        }

        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemy.GetComponent<NetworkObject>().Spawn(); // ✅ Network-spawned
    }
}
