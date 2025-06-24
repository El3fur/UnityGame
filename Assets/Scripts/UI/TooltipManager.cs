using UnityEngine;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    [Header("������ �� UI �������")]
    public GameObject tooltipPanel; 
    public Text tooltipText;        

    private void Awake()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }

    private void Update()
    {
        // ���� ������ ������, �� ������ ��������� �� ��������
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            // ������� ���� ���� �������, ����� �� ��������� �������
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
