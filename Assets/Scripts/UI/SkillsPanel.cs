using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SkillsPanel : MonoBehaviour
{
    [Header("Ссылки на UI")]
    public Text XPText;
    public Text SkillPointsText;
    public Button DashButton;
    public TooltipManager tooltipManager;
    private PlayerExperience localExp;

    private void Start()
    {
        if (!NetworkManager.Singleton.IsClient ||
            !NetworkManager.Singleton.IsConnectedClient)
            return;

        var client = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (client == null) return;

        localExp = client.GetComponent<PlayerExperience>();
        if (localExp == null) return;

       
        localExp.CurrentXP.OnValueChanged += UpdateXPDisplay;
        localExp.XPThreshold.OnValueChanged += UpdateXPDisplay;

        
        localExp.SkillPoints.OnValueChanged += OnSkillPointsChanged;
        localExp.HasDashSkill.OnValueChanged += OnHasDashSkillChanged;

        
        UpdateXPDisplay(0, localExp.CurrentXP.Value);
        UpdateSkillPointsDisplay(localExp.SkillPoints.Value);
        RefreshDashButton();

        DashButton.onClick.AddListener(OnDashButtonClicked);
    }

    private void OnDestroy()
    {
        if (localExp != null)
        {
            localExp.CurrentXP.OnValueChanged -= UpdateXPDisplay;
            localExp.XPThreshold.OnValueChanged -= UpdateXPDisplay;
            localExp.SkillPoints.OnValueChanged -= OnSkillPointsChanged;
            localExp.HasDashSkill.OnValueChanged -= OnHasDashSkillChanged;
        }
    }

    // Этот метод вызывается, когда меняется CurrentXP или XPThreshold
    private void UpdateXPDisplay(int oldVal, int newVal)
    {
        if (localExp == null) return;
        XPText.text = $"XP: {localExp.CurrentXP.Value} / {localExp.XPThreshold.Value}";
    }

    
    private void OnSkillPointsChanged(int oldVal, int newVal)
    {
        UpdateSkillPointsDisplay(newVal);
        RefreshDashButton();
    }

    
    private void OnHasDashSkillChanged(bool oldVal, bool newVal)
    {
        RefreshDashButton();
    }

    
    private void UpdateSkillPointsDisplay(int value)
    {
        SkillPointsText.text = $"Очки: {value}";
    }

    
    private void RefreshDashButton()
    {
        if (localExp == null) return;
        DashButton.interactable = localExp.SkillPoints.Value > 0 && !localExp.HasDashSkill.Value;
    }

    private void OnDashButtonClicked()
    {
        if (localExp == null) return;
        localExp.BuyDashSkillServerRpc();
    }
}
