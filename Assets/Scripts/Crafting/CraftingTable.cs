using UnityEngine;
using UnityEngine.UI;

public class CraftingTable : MonoBehaviour
{
    [Header("Настройки Raycast")]
    [Tooltip("Максимальная дистанция взаимодействия с рабочим столом")]
    public float interactionDistance = 3f;
    [Tooltip("Слой, на котором находится стол (Workbench)")]
    public LayerMask craftingTableLayerMask;

    [Header("UI (Prompt Text)")]
    [Tooltip("Text с подсказкой \"Press E to use workbench\". Отключён по умолчанию.")]
    public Text promptText;

    private Camera playerCamera;
    private bool isAimingAtTable = false;

    private void Start()
    {
        
        playerCamera = Camera.main;
        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isAimingAtTable == false)
        {
            UIManager.Instance.craftingUI.Hide();
        }
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            
            if (playerCamera == null)
                return;
        }

        
        DoRaycast();
        if (!isAimingAtTable && UIManager.Instance.craftingUI.gameObject.activeSelf)
            UIManager.Instance.craftingUI.Hide();
        CheckUseInput();
    }

    private void DoRaycast()
    {
        isAimingAtTable = false;

        
#if UNITY_EDITOR
        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactionDistance, Color.red);
#endif

        
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                interactionDistance,
                craftingTableLayerMask,
                QueryTriggerInteraction.Collide))
        {
            
            CraftingTable table = hit.collider.GetComponentInParent<CraftingTable>();
            if (table == this)
            {
                isAimingAtTable = true;
            }
        }

        
        if (promptText != null)
            promptText.gameObject.SetActive(isAimingAtTable);
    }

    private void CheckUseInput()
    {
        if (!isAimingAtTable)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (promptText != null)
                promptText.gameObject.SetActive(false);

            bool craftingIsOpen = UIManager.Instance.craftingUI.gameObject.activeSelf;
            bool inventoryIsOpen = UIManager.Instance.inventoryUI.IsOpen;

            if (!craftingIsOpen)
            {
                
                if (!inventoryIsOpen)
                    UIManager.Instance.inventoryUI.Open();
                UIManager.Instance.craftingUI.Show();
            }
            else
            {
                
                UIManager.Instance.craftingUI.Hide();
                if (inventoryIsOpen)
                    UIManager.Instance.inventoryUI.Close();
            }
        }
    }

}
