using UnityEngine;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    [Header("Ссылки на UI тултипа")]
    public GameObject tooltipPanel; 
    public Text tooltipText;        

    private void Awake()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }

    private void Update()
    {
        // Если тултип открыт, он должен следовать за курсором
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            // Смещаем чуть выше курсора, чтобы не закрывать стрелку
            tooltipPanel.transform.position = Input.mousePosition + Vector3.up * 50;
        }
    }

    public void ShowTooltip(string description)
    {
        if (tooltipPanel == null || tooltipText == null) return;
        tooltipText.text = description;
        tooltipPanel.SetActive(true);
    }


    public void HideTooltip()
    {
        if (tooltipPanel == null) return;
        tooltipPanel.SetActive(false);
    }
}
