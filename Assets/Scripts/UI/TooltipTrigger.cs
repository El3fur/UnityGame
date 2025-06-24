using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea(3, 10)]
    [Tooltip("Описание этого скилла. Будет показываться в тултипе.")]
    public string description;

    [Tooltip("Схлопнёт TooltipManager из сцены. Если не задано, попытается найти автоматически.")]
    public TooltipManager tooltipManager;

    private void Awake()
    {
        
        if (tooltipManager == null)
            tooltipManager = FindFirstObjectByType<TooltipManager>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipManager != null)
            tooltipManager.ShowTooltip(description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipManager != null)
            tooltipManager.HideTooltip();
    }
}
