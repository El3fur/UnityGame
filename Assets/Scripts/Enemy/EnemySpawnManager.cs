using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawnManager : MonoBehaviour
{
    [Header("Тайминги спавна (сек)")]
    public float minSpawnDelay = 3f;
    public float maxSpawnDelay = 5f;

    [Header("Радиус спавна")]
    public float spawnRadius = 20f;

    [Header("Область, внутри которой не спавнить (замок)")]
    public CapsuleCollider castleArea;

    [Header("Пул врагов")]
    public List<GameObject> enemyPool;
    public int baseCount = 6;

    private int cycleCount = 0;
    private bool isRunning = false;

    /// <summary>
    /// Вызывается один раз после Bake NavMesh
    /// </summary>
    public void InitializeSpawning()
    {
        if (isRunning) return;
        isRunning = true;
        StartCoroutine(SpawnLoop());
        Debug.Log("[SpawnManager] Инициализация спавна завершена");
    }

    private IEnumerator SpawnLoop()
    {
        // Небольшая задержка перед первой волной
        yield return new WaitForSeconds(1f);

        while (true)
        {
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            Debug.Log($"[SpawnManager] Следующая волна через {delay:F1}s");
            yield return new WaitForSeconds(delay);

            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("[SpawnManager] Не сервер — пропускаю волну");
                continue;
            }

            cycleCount++;
            int countPerPlayer = baseCount + (cycleCount - 1);
            Debug.Log($"[SpawnManager] Волна #{cycleCount}: по {countPerPlayer} мобов на каждого игрока");

            // Найти всех игроков в сессии
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
        int maxAttempts = count * 10; // до 10 попыток на каждого моба

        while (spawned < count && attempts < maxAttempts)
        {
            attempts++;
            Vector3 rnd = center.position + Random.insideUnitSphere * spawnRadius;
            rnd.y = center.position.y;

            // исключаем точки внутри замка
            if (castleArea != null && castleArea.bounds.Contains(rnd))
                continue;

            // смотрим, есть ли там NavMesh
            if (NavMesh.SamplePosition(rnd, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                Vector3 spawnPos = hit.position + Vector3.up * 0.1f; // поднимаем над землёй
                var prefab = enemyPool[Random.Range(0, enemyPool.Count)];

                var go = Instantiate(prefab, spawnPos, Quaternion.identity);
                var netObj = go.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    netObj.Spawn();
                    Debug.Log($"[SpawnManager] Заспавнил {go.name} для {center.name} @ {spawnPos}");
                }
                else
                {
                    Debug.LogError($"[SpawnManager] У префаба {prefab.name} нет NetworkObject!");
                }

                spawned++;
            }
            else
            {
                // для отладки точек, где не удалось найти NavMesh
                Debug.LogWarning($"[SpawnManager] SamplePosition FAILED at {rnd}");
            }
        }

        Debug.Log($"[SpawnManager] Для {center.name} спавн: {spawned}/{count} (попыток {attempts})");
    }
}
