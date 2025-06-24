using System.Collections;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(PlayerMovement))]
public class FOVController : NetworkBehaviour
{
    [Header("Настройки FOV")]
    [Tooltip("Камера, чьё поле зрения будем менять.")]
    public Camera playerCamera;
    [Tooltip("FOV при обычной ходьбе.")]
    public float defaultFOV = 100f;
    [Tooltip("FOV при спринте.")]
    public float sprintFOV = 120f;
    [Tooltip("FOV во время рывка.")]
    public float dashFOV = 120f;
    [Tooltip("Скорость интерполяции FOV.")]
    public float smoothSpeed = 8f;

    private PlayerMovement movement;
    private PlayerDash dash;
    private bool isDashing = false;
    private float targetFOV;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        dash = GetComponent<PlayerDash>();

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        targetFOV = defaultFOV;

        if (playerCamera != null)
            playerCamera.fieldOfView = defaultFOV;
    }

    private void Update()
    {
        if (!IsOwner) return; 

        
        if (dash != null && dash.IsCurrentlyDashing && !isDashing)
        {
            
            isDashing = true;
            targetFOV = dashFOV;
            
        }
        else if (dash != null && !dash.IsCurrentlyDashing && isDashing)
        {
            
            isDashing = false;
        }

        
        if (!isDashing && movement != null)
        {
            if (movement.IsSprinting)
                targetFOV = sprintFOV;
            else
                targetFOV = defaultFOV;
        }

        
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                targetFOV,
                Time.deltaTime * smoothSpeed
            );
        }
    }
}
