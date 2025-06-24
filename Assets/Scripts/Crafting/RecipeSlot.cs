using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RecipeSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [HideInInspector] public CraftingRecipe recipe;

    [Header("UI References (ссылки на общий TooltipPanel)")]
    public GameObject tooltipPanel;   
    public Text tooltipText;

    public Image iconImage;
    public Text nameText;

    private CraftingUI parentUI;

    public void Initialize(CraftingRecipe recipeData, CraftingUI parent, GameObject tooltipPanel, Text tooltipText)
    {
        recipe = recipeData;
        parentUI = parent;
        this.tooltipPanel = tooltipPanel;
        this.tooltipText = tooltipText;
        iconImage.sprite = recipeData.resultItem.icon;
        nameText.text = recipeData.resultItem.itemName;

        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }
    private void Update()
    {
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            
            tooltipPanel.transform.position = Input.mousePosition + (Vector3.up * 50);
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPanel == null || tooltipText == null)
            return;

        
        string s = "";
        foreach (var ing in recipe.ingredients)
            s += $"{ing.item.itemName} - {ing.amount}\n";

        tooltipText.text = s.TrimEnd('\n');
        tooltipPanel.SetActive(true);

        
        tooltipPanel.transform.position = Input.mousePosition + (Vector3.up * 50);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipPanel.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            parentUI.TryCraft(recipe);
    }
}
