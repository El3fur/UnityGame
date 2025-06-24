using UnityEngine;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;
    [SerializeField] private List<InventoryItem> items;
    private Dictionary<string, InventoryItem> dict;

    private void Awake()
    {
        Instance = this;
        dict = new Dictionary<string, InventoryItem>();
        foreach (var it in items) dict[it.itemId] = it;
    }

    public InventoryItem GetItemById(string id)
        => dict.ContainsKey(id) ? dict[id] : null;
}
