using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    public Image iconImage;
    public Text amountText;
    public string itemId;
    public int slotIndex;
    public bool isHotbarSlot;

    private Transform originalParent;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Set(string newItemId, Sprite icon, int amount)
    {
        itemId = newItemId;
        iconImage.sprite = icon;
        iconImage.enabled = !string.IsNullOrEmpty(itemId);

        if (!string.IsNullOrEmpty(itemId) && amount > 1)
        {
            amountText.text = amount.ToString();
            amountText.enabled = true;
        }
        else
        {
            amountText.enabled = false;
        }
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"[Slot] Начало перетаскивания itemId={itemId}");
        originalParent = transform.parent;
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[Slot] Завершено перетаскивание itemId={itemId}");

        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;
        canvasGroup.blocksRaycasts = true;

        
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log($"[Slot] Выброс предмета itemId={itemId} из слота {slotIndex}");

            var inv = UIManager.Instance.inventoryUI.Inv;
            inv.DropItem(isHotbarSlot, slotIndex);
            UIManager.Instance.inventoryUI.RefreshAll();
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        
        var from = eventData.pointerDrag?.GetComponent<InventorySlot>();
        if (from == null) return;
        var ui = UIManager.Instance.inventoryUI;
        var inv = ui.Inv;         
        var fromArray = from.isHotbarSlot ? inv.hotbarSlots : inv.inventorySlots;
        var toArray = isHotbarSlot ? inv.hotbarSlots : inv.inventorySlots;
        var src = fromArray[from.slotIndex];
        var dst = toArray[slotIndex];

        
        if (src.item != null
            && dst.item != null
            && src.item == dst.item
            && src.item.stackable)
        {
            int total = src.amount + dst.amount;
            int maxSt = src.item.maxStack;
            
            dst.amount = Mathf.Min(total, maxSt);            
            src.amount = total - dst.amount;

            
            if (src.amount == 0)
                src.item = null;
        }
        else
        {
            
            inv.Swap(from.isHotbarSlot, from.slotIndex, isHotbarSlot, slotIndex);
        }

        ui.RefreshAll();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
            return;

        
        var ui = UIManager.Instance.inventoryUI;
        var playerInv = ui.Inv; 

        
        PlayerInventory.InventorySlot[] systemSlots = isHotbarSlot
            ? playerInv.hotbarSlots
            : playerInv.inventorySlots;

        var sysSlot = systemSlots[slotIndex];
        if (sysSlot.item == null || sysSlot.amount < 2)
            return;

        
        PlayerInventory.InventorySlot[] targetArray = systemSlots;
        int emptyIndex = -1;
        for (int i = 0; i < targetArray.Length; i++)
        {
            if (targetArray[i].item == null)
            {
                emptyIndex = i;
                break;
            }
        }
       
        if (emptyIndex < 0)
        {
            var otherArray = isHotbarSlot
                ? playerInv.inventorySlots
                : playerInv.hotbarSlots;
            for (int i = 0; i < otherArray.Length; i++)
            {
                if (otherArray[i].item == null)
                {
                    emptyIndex = i;
                    targetArray = otherArray;
                    break;
                }
            }
        }
        if (emptyIndex < 0)
            return; 

       
        sysSlot.amount -= 1;

        
        targetArray[emptyIndex].item = sysSlot.item;
        targetArray[emptyIndex].amount = 1;

        
        ui.RefreshAll();
    }



}
