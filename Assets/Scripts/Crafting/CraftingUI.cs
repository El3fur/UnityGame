using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CraftingUI : MonoBehaviour
{
    [Header("References")]
    public Transform contentParent;
    public GameObject recipeSlotPrefab;
    public List<CraftingRecipe> allRecipes = new List<CraftingRecipe>();

    private PlayerInventory playerInventory;
    public GameObject tooltipPanel;
    public Text tooltipText;

    private void Awake()
    {
        
        TryFindInventory();
    }

    private void Start()
    {
        
        gameObject.SetActive(false);

        
        PopulateRecipes();
    }

    private void Update()
    {
        
        if (playerInventory == null)
            TryFindInventory();
    }

    private void TryFindInventory()
    {
        
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            playerInventory = playerGO.GetComponent<PlayerInventory>();

        if (playerInventory == null)
            Debug.LogError("[CraftingUI] PlayerInventory �� ������ �� ������� � ����� Player!");
    }

    private void PopulateRecipes()
    {
        if (recipeSlotPrefab == null || contentParent == null)
        {
            Debug.LogError("[CraftingUI] recipeSlotPrefab ��� contentParent �� �����!");
            return;
        }

        
        foreach (Transform t in contentParent)
            Destroy(t.gameObject);

        foreach (var recipe in allRecipes)
        {
            GameObject go = Instantiate(recipeSlotPrefab, contentParent);
            var slot = go.GetComponent<RecipeSlot>();
            slot.Initialize(recipe, this, tooltipPanel, tooltipText);
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void TryCraft(CraftingRecipe recipe)
    {
        
        if (playerInventory == null)
        {
            Debug.LogWarning("[Crafting] �� ���� ��������: PlayerInventory �� ��� �� ������!");
            return;
        }

        
        foreach (var ing in recipe.ingredients)
        {
            if (!playerInventory.HasItem(ing.item.itemId, ing.amount))
            {
                Debug.Log($"[Crafting] ���������� ������� �{recipe.resultItem.itemName}�: �� ������� {ing.item.itemName}");
                return;
            }
        }

        
        foreach (var ing in recipe.ingredients)
            playerInventory.RemoveItem(ing.item.itemId, ing.amount);

       
        bool added = playerInventory.AddItem(recipe.resultItem, recipe.resultAmount);
        if (!added)
        {
            Debug.Log($"[Crafting] �� ������� �������� �{recipe.resultItem.itemName}� � ��������� � ��� �����");
            return;
        }

        
        UIManager.Instance.inventoryUI.RefreshAll();
        Debug.Log($"[Crafting] �������: �{recipe.resultItem.itemName}� x{recipe.resultAmount}");
    }
}
