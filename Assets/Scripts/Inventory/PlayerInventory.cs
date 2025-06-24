using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerInventory : MonoBehaviour
{
    [System.Serializable]
    public class InventorySlot
    {
        public InventoryItem item;
        public int amount;
    }


    public InventorySlot[] inventorySlots;
    public InventorySlot[] hotbarSlots;


    public int InventorySize => inventorySlots != null ? inventorySlots.Length : 0;
    public int HotbarSize => hotbarSlots != null ? hotbarSlots.Length : 0;


    public bool AddItemToSlot(int slotIndex, bool toHotbar, InventoryItem item, int qty = 1)
    {
        Debug.Log($"[Inventory] Попытка добавить {item?.itemId} x{qty} в {(toHotbar ? "hotbar" : "inventory")} слот {slotIndex}");

        var slots = toHotbar ? hotbarSlots : inventorySlots;
        if (slots == null || slotIndex < 0 || slotIndex >= slots.Length || item == null)
        {
            Debug.LogWarning("[Inventory] Слот недопустим или item null");
            return false;
        }

        var slot = slots[slotIndex];
        if (slot.item == null)
        {
            slot.item = item;
            slot.amount = Mathf.Min(qty, item.maxStack);
            Debug.Log("[Inventory] Предмет успешно помещён в пустой слот");
            return true;
        }
        else if (slot.item == item && item.stackable && slot.amount < item.maxStack)
        {
            slot.amount = Mathf.Min(slot.amount + qty, item.maxStack);
            Debug.Log("[Inventory] Предмет стекнулся в существующий слот");
            return true;
        }

        Debug.Log("[Inventory] Слот занят несовместимым предметом или стек полон");
        return false;
    }


    public void Swap(bool fromHotbar, int fromIndex, bool toHotbar, int toIndex)
    {
        var src = fromHotbar ? hotbarSlots : inventorySlots;
        var dst = toHotbar ? hotbarSlots : inventorySlots;
        if (src == null || dst == null ||
            fromIndex < 0 || fromIndex >= src.Length ||
            toIndex < 0 || toIndex >= dst.Length)
            return;

        var tmp = src[fromIndex];
        src[fromIndex] = dst[toIndex];
        dst[toIndex] = tmp;
    }


    public List<string> GetInventoryItems()
    {
        if (inventorySlots == null) return new List<string>();
        return inventorySlots
            .Select(s => s.item != null ? s.item.itemId : string.Empty)
            .ToList();
    }

    public List<string> GetHotbarItems()
    {
        if (hotbarSlots == null) return new List<string>();
        return hotbarSlots
            .Select(s => s.item != null ? s.item.itemId : string.Empty)
            .ToList();
    }
    public void DropItem(bool isHotbar, int index)
    {
        var slots = isHotbar ? hotbarSlots : inventorySlots;
        if (slots == null || index < 0 || index >= slots.Length) return;

        var slot = slots[index];
        if (slot.item == null) return;

        Vector3 spawnPos = transform.position + transform.forward * 2f;
        GameObject go = Instantiate(slot.item.worldPrefab, spawnPos, Quaternion.identity);
        var pickup = go.GetComponent<ItemPickup>();
        if (pickup != null)
        {
            pickup.itemData = slot.item;
            pickup.amount = slot.amount;
        }

        slot.item = null;
        slot.amount = 0;
    }
    public List<int> GetInventoryAmounts()
    {
        if (inventorySlots == null) return new List<int>();
        return inventorySlots.Select(s => s.item != null ? s.amount : 0).ToList();
    }

    public List<int> GetHotbarAmounts()
    {
        if (hotbarSlots == null) return new List<int>();
        return hotbarSlots.Select(s => s.item != null ? s.amount : 0).ToList();
    }

    public bool AddItem(InventoryItem item, int amount)
    {
        if (item == null || amount <= 0) return false;


        for (int i = 0; i < InventorySize; i++)
        {
            var slot = inventorySlots[i];
            if (slot.item == item && item.stackable && slot.amount < item.maxStack)
            {
                int spaceLeft = item.maxStack - slot.amount;
                int toAdd = Mathf.Min(spaceLeft, amount);
                slot.amount += toAdd;
                amount -= toAdd;
                if (amount <= 0)
                    return true;
            }
        }


        for (int i = 0; i < InventorySize; i++)
        {
            var slot = inventorySlots[i];
            if (slot.item == null)
            {
                int toPut = Mathf.Min(item.maxStack, amount);
                slot.item = item;
                slot.amount = toPut;
                amount -= toPut;
                if (amount <= 0)
                    return true;
            }
        }


        for (int i = 0; i < HotbarSize; i++)
        {
            var slot = hotbarSlots[i];
            if (slot.item == item && item.stackable && slot.amount < item.maxStack)
            {
                int spaceLeft = item.maxStack - slot.amount;
                int toAdd = Mathf.Min(spaceLeft, amount);
                slot.amount += toAdd;
                amount -= toAdd;
                if (amount <= 0)
                    return true;
            }
        }


        for (int i = 0; i < HotbarSize; i++)
        {
            var slot = hotbarSlots[i];
            if (slot.item == null)
            {
                int toPut = Mathf.Min(item.maxStack, amount);
                slot.item = item;
                slot.amount = toPut;
                amount -= toPut;
                if (amount <= 0)
                    return true;
            }
        }
        return false;
    }
        public bool HasItem(string itemId, int requiredAmount)
    {
        if (string.IsNullOrEmpty(itemId) || requiredAmount <= 0)
            return false;

        int total = 0;
        
        foreach (var slot in inventorySlots)
        {
            if (slot.item != null && slot.item.itemId == itemId)
            {
                total += slot.amount;
                if (total >= requiredAmount)
                    return true;
            }
        }
        // Подсчитываем по hotbarSlots
        foreach (var slot in hotbarSlots)
        {
            if (slot.item != null && slot.item.itemId == itemId)
            {
                total += slot.amount;
                if (total >= requiredAmount)
                    return true;
            }
        }
        return false;
    }

    
    public bool RemoveItem(string itemId, int removeAmount)
    {
        if (string.IsNullOrEmpty(itemId) || removeAmount <= 0)
            return false;

        int toRemove = removeAmount;

        
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            var slot = inventorySlots[i];
            if (slot.item != null && slot.item.itemId == itemId && slot.amount > 0)
            {
                int taken = Mathf.Min(slot.amount, toRemove);
                slot.amount -= taken;
                toRemove -= taken;
                if (slot.amount == 0)
                    slot.item = null;

                if (toRemove <= 0)
                    return true;
            }
        }

        
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            var slot = hotbarSlots[i];
            if (slot.item != null && slot.item.itemId == itemId && slot.amount > 0)
            {
                int taken = Mathf.Min(slot.amount, toRemove);
                slot.amount -= taken;
                toRemove -= taken;
                if (slot.amount == 0)
                    slot.item = null;

                if (toRemove <= 0)
                    return true;
            }
        }

       
        return false;
    }


}
