using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerInteraction : NetworkBehaviour
{
    [Header("Raycast")]
    [Tooltip("������������ ��������� �������")]
    public float interactionDistance = 5f;

    [Tooltip("����, �� ������� ����� �������� (Trigger)")]
    public LayerMask pickupLayerMask;

    [Header("UI (Prompt Text)")]
    [Tooltip("���� �������� ������, ������� ������ � ����� PickupPrompt")]
    public Text promptText;

    private Camera playerCamera;
    private PlayerInventory playerInventory;
    private ItemPickup currentTarget;

    private void Start()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        
        playerCamera = Camera.main;
        if (playerCamera == null)
            Debug.LogError("[PlayerInteraction] �� ������� MainCamera!");

       
        playerInventory = GetComponent<PlayerInventory>();
        if (playerInventory == null)
            Debug.LogError("[PlayerInteraction] PlayerInventory �� ������!");

       
        if (promptText == null)
        {
            GameObject txtObj = GameObject.FindWithTag("PickupPrompt");
            if (txtObj != null)
                promptText = txtObj.GetComponent<Text>();
        }

        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!IsOwner) return;

        DoRaycast();
        CheckPickupInput();
    }

    private void DoRaycast()
    {
        
        currentTarget = null;

        
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                interactionDistance,
                pickupLayerMask,
                QueryTriggerInteraction.Collide))
        {
            
            ItemPickup item = hit.collider.GetComponent<ItemPickup>();
            if (item != null)
                currentTarget = item;
        }

        
        if (promptText != null)
            promptText.gameObject.SetActive(currentTarget != null);
    }

    private void CheckPickupInput()
    {
        if (currentTarget == null)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            bool picked = currentTarget.TryPickup(playerInventory);
            if (picked && promptText != null)
            {
                promptText.gameObject.SetActive(false);
                currentTarget = null;
            }
        }
    }
}
