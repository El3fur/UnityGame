﻿using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;
using Unity.AI.Navigation;      // NavMeshSurface
using UnityEngine.AI;

public class MapGenerator : MonoBehaviour
{
    [Tooltip("Компонент NavMeshSurface на вашей карте")]
    public NavMeshSurface navSurface;
    [Tooltip("Ссылка на EnemySpawnManager")]
    public EnemySpawnManager spawnManager;
    public enum DrawMode { NoiseMap, ColourMap, Mesh, FalloffMap };
    public DrawMode drawMode;

    public Noise.NormalizeMode normalizeMode;

    public const int mapChunkSize = 241;
    [Range(0, 6)]
    public int editorPreviewLOD;
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public bool useFalloff;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;

    public TerrainType[] regions;
    public Material terrainMaterial; // Перетащи сюда материал с твоим шейдером


    float[,] falloffMap;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    private void Start()
    {
        DrawMapInEditor();
        Debug.Log("[MapGenerator] Start() called");
        OnMapMeshGenerated();
    }
    void Awake()
    {
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
    }

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);

        if (drawMode == DrawMode.Mesh)
        {
            MapDisplay display = FindFirstObjectByType<MapDisplay>();
            MeshData meshData = MeshGenerator.GenerateTerrainMesh(
                mapData.heightMap,
                meshHeightMultiplier,
                meshHeightCurve,
                editorPreviewLOD
            );

            display.DrawMesh(meshData.CreateMesh(), terrainMaterial);
        }
    }
    public void RequestMapData(Vector2 centre, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate {
            MapDataThread(centre, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 centre, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(centre);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    MapData GenerateMapData(Vector2 centre)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, centre + offset, normalizeMode);

        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                if (useFalloff)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                }
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight >= regions[i].height)
                    {
                        colourMap[y * mapChunkSize + x] = regions[i].colour;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }


        return new MapData(noiseMap, colourMap);
    }

    void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }


    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }

    }
    public void OnMapMeshGenerated()
    {
        StartCoroutine(BuildNavMeshRoutine());
    }
    private IEnumerator BuildNavMeshRoutine()
    {
        Debug.Log("[MapGenerator] Начинаю сброс старого NavMesh");
        navSurface.RemoveData();     // очищаем предыдущий
        navSurface.AddData();        // создаём пустой NavMeshData

        // Синхронная сборка (можно сделать UpdateNavMesh → asynchronous)
        Debug.Log("[MapGenerator] Выполняю BuildNavMesh()");
        navSurface.BuildNavMesh();   // блокирует, но только здесь, сразу после генерации карты

        // Проверяем, что данные создались
        if (navSurface.navMeshData == null)
        {
            Debug.LogError("[MapGenerator] Ошибка: navMeshData == null после BuildNavMesh()");
            yield break;
        }
        Debug.Log("[MapGenerator] NavMesh готов, запускаю спавн");

        // Уведомляем спавнер стартовать волны
        spawnManager.InitializeSpawning();

        yield break;
    }

}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colourMap;

    public MapData(float[,] heightMap, Color[] colourMap)
    {
        this.heightMap = heightMap;
        this.colourMap = colourMap;
    }
}