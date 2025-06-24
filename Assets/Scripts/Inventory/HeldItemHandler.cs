using UnityEngine;
using Unity.Netcode;

public class HeldItemHandler : NetworkBehaviour
{
    public Transform handSocket;
    private GameObject currentHeldObject;
    private string currentItemId;
    private PlayerInventory inventory;    
    public InventoryItem currentHeldItem { get; private set; }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            inventory = GetComponent<PlayerInventory>();
    }

    void Update()
    {
        if (!IsOwner) return;

        for (int i = 0; i < 5; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                TryEquipHotbarItem(i);
            }
        }
    }

    void TryEquipHotbarItem(int index)
    {
        if (inventory == null) return;
        var slot = inventory.hotbarSlots[index];
        if (slot.item == null)
        {
            Debug.Log("[HeldItem] Пустой слот");
            EquipItemServerRpc("");
        }
        else
        {
            Debug.Log($"[HeldItem] Запрос на {slot.item.itemId}");
            EquipItemServerRpc(slot.item.itemId);
        }
    }

    [ServerRpc]
    void EquipItemServerRpc(string itemId)
    {
        EquipItemClientRpc(itemId);
    }

    [ClientRpc]
    void EquipItemClientRpc(string itemId)
    {
        
        if (currentItemId == itemId)
            return;

        
        if (currentHeldObject != null)
        {
            Destroy(currentHeldObject);
            currentHeldObject = null;
        }

        currentItemId = itemId;
        currentHeldItem = null;

        if (string.IsNullOrEmpty(itemId))
            return;

        var item = ItemDatabase.Instance.GetItemById(itemId);
        if (item == null || item.handPrefab == null)
        {
            Debug.LogWarning($"[HeldItem] itemId {itemId} не найден или нет handPrefab");
            return;
        }

        
        currentHeldItem = item;

        
        var instance = Instantiate(item.handPrefab, handSocket);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        currentHeldObject = instance;
    }
}
