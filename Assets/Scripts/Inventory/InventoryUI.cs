using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public Transform inventoryGrid;
    public Transform hotbarGrid;
    public GameObject slotPrefab;
    public PlayerInventory Inv { get; private set; }
    private PlayerInventory inv;
    private List<InventorySlot> invSlots = new();
    private List<InventorySlot> hotSlots = new();

    public void Init(PlayerInventory playerInv)
    {
        Inv = playerInv;
        Debug.Log("[InventoryUI] Init вызван");

        if (invSlots.Count == 0)
        {
            Debug.Log($"[InventoryUI] Создание {Inv.InventorySize} инвентарь-слотов");
            for (int i = 0; i < Inv.InventorySize; i++)
            {
                var go = Instantiate(slotPrefab, inventoryGrid);
                var slot = go.GetComponent<InventorySlot>();
                slot.slotIndex = i;
                slot.isHotbarSlot = false;
                invSlots.Add(slot);
            }
        }

        if (hotSlots.Count == 0)
        {
            Debug.Log($"[InventoryUI] Создание {Inv.HotbarSize} хотбар-слотов");
            for (int i = 0; i < Inv.HotbarSize; i++)
            {
                var go = Instantiate(slotPrefab, hotbarGrid);
                var slot = go.GetComponent<InventorySlot>();
                slot.slotIndex = i;
                slot.isHotbarSlot = true;
                hotSlots.Add(slot);
            }
        }

        RefreshAll();
    }

    public void RefreshAll()
    {
        if (Inv == null)
        {
            Debug.LogWarning("[InventoryUI] Inv == null в RefreshAll");
            return;
        }

        var itemIds = Inv.GetInventoryItems();
        var amounts = Inv.GetInventoryAmounts();
        for (int i = 0; i < invSlots.Count; i++)
        {
            var id = itemIds[i];
            var item = ItemDatabase.Instance.GetItemById(id);
            int amt = amounts[i];
            invSlots[i].Set(id, item?.icon, amt);
        }

        var hotbarIds = Inv.GetHotbarItems();
        var hotbarAmounts = Inv.GetHotbarAmounts();
        for (int i = 0; i < hotSlots.Count; i++)
        {
            var id = hotbarIds[i];
            var item = ItemDatabase.Instance.GetItemById(id);
            int amt = hotbarAmounts[i];
            hotSlots[i].Set(id, item?.icon, amt);
        }
    }


    public bool IsOpen => gameObject.activeSelf;

    public void Open()
    {
        gameObject.SetActive(true);
        UIManager.Instance.inventoryPanel.SetActive(true); // если у вас InventoryUI == inventoryPanel
        UIManager.Instance.ApplyCursorState(); // вызываем смену курсора
    }

    public void Close()
    {
        gameObject.SetActive(false);
        UIManager.Instance.inventoryPanel.SetActive(false);
        UIManager.Instance.ApplyCursorState();
    }

    public void SwapItems(InventorySlot a, InventorySlot b)
    {
        
        inv.Swap(a.isHotbarSlot, a.slotIndex, b.isHotbarSlot, b.slotIndex);
        RefreshAll();
    }
}
