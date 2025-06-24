using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawnManager : MonoBehaviour
{
    [Header("�������� ������ (���)")]
    public float minSpawnDelay = 3f;
    public float maxSpawnDelay = 5f;

    [Header("������ ������")]
    public float spawnRadius = 20f;

    [Header("�������, ������ ������� �� �������� (�����)")]
    public CapsuleCollider castleArea;

    [Header("��� ������")]
    public List<GameObject> enemyPool;
    public int baseCount = 6;

    private int cycleCount = 0;
    private bool isRunning = false;

    /// <summary>
    /// ���������� ���� ��� ����� Bake NavMesh
    /// </summary>
    public void InitializeSpawning()
    {
        if (isRunning) return;
        isRunning = true;
        StartCoroutine(SpawnLoop());
        Debug.Log("[SpawnManager] ������������� ������ ���������");
    }

    private IEnumerator SpawnLoop()
    {
        // ��������� �������� ����� ������ ������
        yield return new WaitForSeconds(1f);

        while (true)
        {
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            Debug.Log($"[SpawnManager] ��������� ����� ����� {delay:F1}s");
            yield return new WaitForSeconds(delay);

            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("[SpawnManager] �� ������ � ��������� �����");
                continue;
            }

            cycleCount++;
            int countPerPlayer = baseCount + (cycleCount - 1);
            Debug.Log($"[SpawnManager] ����� #{cycleCount}: �� {countPerPlayer} ����� �� ������� ������");

            // ����� ���� ������� � ������
            var players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var pgo in players)
            {
                SpawnWaveAround(pgo.transform, countPerPlayer);
            }
        }
    }

    private void SpawnWaveAround(Transform center, int count)
    {
        int spawned = 0;
        int attempts = 0;
        int maxAttempts = count * 10; // �� 10 ������� �� ������� ����

        while (spawned < count && attempts < maxAttempts)
        {
            attempts++;
            Vector3 rnd = center.position + Random.insideUnitSphere * spawnRadius;
            rnd.y = center.position.y;

            // ��������� ����� ������ �����
            if (castleArea != null && castleArea.bounds.Contains(rnd))
                continue;

            // �������, ���� �� ��� NavMesh
            if (NavMesh.SamplePosition(rnd, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                Vector3 spawnPos = hit.position + Vector3.up * 0.1f; // ��������� ��� �����
                var prefab = enemyPool[Random.Range(0, enemyPool.Count)];

                var go = Instantiate(prefab, spawnPos, Quaternion.identity);
                var netObj = go.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    netObj.Spawn();
                    Debug.Log($"[SpawnManager] ��������� {go.name} ��� {center.name} @ {spawnPos}");
                }
                else
                {
                    Debug.LogError($"[SpawnManager] � ������� {prefab.name} ��� NetworkObject!");
                }

                spawned++;
            }
            else
            {
                // ��� ������� �����, ��� �� ������� ����� NavMesh
                Debug.LogWarning($"[SpawnManager] SamplePosition FAILED at {rnd}");
            }
        }

        Debug.Log($"[SpawnManager] ��� {center.name} �����: {spawned}/{count} (������� {attempts})");
    }
}
