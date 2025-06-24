using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    [Header("Данные предмета")]
    public InventoryItem itemData;
    public int amount = 1;

    
    private IOutline outlineComponent;

    private void Awake()
    {
        
        outlineComponent = GetComponent<IOutline>();
        if (outlineComponent != null)
            outlineComponent.SetEnabled(false);
    }
    /// <param name="playerInv">Ссылка на PlayerInventory (Inventory). </param>   
    public bool TryPickup(PlayerInventory playerInv)
    {
        if (playerInv == null || itemData == null)
        {
            Debug.LogWarning("[ItemPickup] Не удалось подобрать: playerInv или itemData == null");
            return false;
        }
        
        bool added = playerInv.AddItem(itemData, amount);

        if (added)
        {
           
            if (UIManager.Instance != null && UIManager.Instance.inventoryUI != null)
                UIManager.Instance.inventoryUI.RefreshAll();

            
            Destroy(gameObject);
            return true;
        }
        else
        {
            Debug.Log("[ItemPickup] В инвентаре нет свободного места!");
            return false;
        }
    }

    /// <param name="enable">true — включить обводку, false — выключить.</param>
    public void SetOutline(bool enable)
    {
        if (outlineComponent != null)
            outlineComponent.SetEnabled(enable);
    }
}

public interface IOutline
{
    void SetEnabled(bool enabled);
}
