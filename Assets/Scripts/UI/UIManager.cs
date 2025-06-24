using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panels")]
    public GameObject inventoryPanel;
    public GameObject hotbarPanel;
    public GameObject settingsPanel;
    public GameObject mapPanel;
    public GameObject AbilitypPanel;
    public CraftingUI craftingUI;
    public PlayerInventory playerInventory;

    [Header("Systems")]
    public InventoryUI inventoryUI;

 
    public bool IsUIOpen => inventoryPanel.activeSelf
                         || settingsPanel.activeSelf
                         || mapPanel.activeSelf
                         || AbilitypPanel.activeSelf;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        inventoryPanel.SetActive(false);
        settingsPanel.SetActive(false);
        mapPanel.SetActive(false);
        hotbarPanel.SetActive(true);
        AbilitypPanel.SetActive(false);
        ApplyCursorState();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            TogglePanel(inventoryPanel);
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePanel(settingsPanel);
        if (Input.GetKeyDown(KeyCode.M))
            TogglePanel(mapPanel);
        if (Input.GetKeyDown(KeyCode.K))
            TogglePanel(AbilitypPanel);
    }

    public void TogglePanel(GameObject panel)
    {
        bool willOpen = !panel.activeSelf;
    
        inventoryPanel.SetActive(false);
        settingsPanel.SetActive(false);
        mapPanel.SetActive(false);
        AbilitypPanel.SetActive(false);

        if (willOpen) panel.SetActive(true);

        ApplyCursorState();
    }

    public void ApplyCursorState()
    {
        if (IsUIOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    public void ResumeGame()
    {
        settingsPanel.SetActive(false);
        ApplyCursorState();
        Debug.Log("Button resume click");
    }
    public void ExitGame()
    {
        Debug.Log("Button Exit click");
        Application.Quit();

    }
}
