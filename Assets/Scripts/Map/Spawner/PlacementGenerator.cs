using System.Collections;
using UnityEngine;

public class PlacementGenerator : MonoBehaviour
{
    [System.Serializable]
    public class Spawnable
    {
        public GameObject prefab;
        public int density = 100;
    }

    [SerializeField] private Spawnable[] spawnables;
    [Header("Raycast Settings")]
    [SerializeField] private float maxHeight = 200f;
    [SerializeField] private float minHeight = 1f;
    [SerializeField] private Vector2 xRange = new Vector2(-2500f, 2500f);
    [SerializeField] private Vector2 zRange = new Vector2(-2500f, 2500f);
    [Header("Surface Filtering")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask excludeMask;

    private const int BATCH_SIZE = 20;

    private void Start()
    {
        StartCoroutine(GenerateAllCoroutine());
    }

    private IEnumerator GenerateAllCoroutine()
    {
        Clear();
        foreach (var s in spawnables)
        {
            for (int i = 0; i < s.density; i++)
            {
                // Raycast вниз
                Vector3 pos = new Vector3(
                    Random.Range(xRange.x, xRange.y),
                    maxHeight,
                    Random.Range(zRange.x, zRange.y)
                );
                if (Physics.Raycast(pos, Vector3.down, out var hit, Mathf.Infinity, groundMask))
                {
                    if (hit.point.y >= minHeight &&
                        ((1 << hit.collider.gameObject.layer) & excludeMask) == 0)
                    {
                        var go = Instantiate(s.prefab, hit.point, Quaternion.identity, transform);
                        // случайный поворот и масштаб (если нужно Ч можно добавить)
                    }
                }

                // уступаем кадр
                if (i % BATCH_SIZE == 0)
                    yield return null;
            }
        }
    }

    private void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }
}
