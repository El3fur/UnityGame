using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerInteraction : NetworkBehaviour
{
    [Header("Raycast")]
    [Tooltip("Максимальная дистанция подбора")]
    public float interactionDistance = 5f;

    [Tooltip("Слой, на котором лежат предметы (Trigger)")]
    public LayerMask pickupLayerMask;

    [Header("UI (Prompt Text)")]
    [Tooltip("Если оставить пустым, найдётся объект с тегом PickupPrompt")]
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
            Debug.LogError("[PlayerInteraction] Не найдена MainCamera!");

       
        playerInventory = GetComponent<PlayerInventory>();
        if (playerInventory == null)
            Debug.LogError("[PlayerInteraction] PlayerInventory не найден!");

       
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
