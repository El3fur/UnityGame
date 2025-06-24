using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewInventoryItem", menuName = "Inventory/Item")]
public class InventoryItem : ScriptableObject
{
    [Header("Unique ID")]
    public string itemId;      
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;
    public GameObject worldPrefab;
    public GameObject handPrefab;


    public enum ItemType { Item, Weapon, Tool, Food, Armor }
    public ItemType type;
    public bool stackable = true;
    public int maxStack = 99;

    public int damageToEnemies = 0;
    public int damageToResources = 0;
}