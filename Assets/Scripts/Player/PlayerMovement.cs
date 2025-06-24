// PlayerMovement.cs
using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(NetworkObject))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Camera Rotation")]
    public float mouseSensitivity = 2f;
    private float verticalRotation = 0f;
    private Transform cameraTransform;

    [Header("Ground Movement")]
    public float MoveSpeed = 5f;
    public float sprintSpeed = 8f;
    private float currentSpeed;
    private float moveHorizontal, moveForward;
    private Animator animator;
    public bool IsSprinting { get; private set; }
    public event Action<bool> OnSprintStateChanged;

    [Header("Jumping")]
    public float jumpForce = 10f;
    public float fallMultiplier = 2.5f;
    public float ascendMultiplier = 2f;
    private bool isGrounded = true;
    public LayerMask groundLayer;
    private float groundCheckTimer, groundCheckDelay = 0.3f;
    private float playerHeight, raycastDistance;

    [Header("Crouching")]
    public float crouchHeight = 1f;
    private float originalHeight;
    private bool isCrouching;
    public float crouchSpeed = 2.5f;

    [Header("Dome Constraint")]
    public GameObject dome;
    private float domeRadius;
    private Vector3 domeCenter;

    private Rigidbody rb;

    // для отладки рывков
    private Vector3 lastPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.freezeRotation = true;

        animator = GetComponentInChildren<Animator>();
        cameraTransform = Camera.main.transform;

        originalHeight = GetComponent<CapsuleCollider>().height;
        playerHeight = originalHeight * transform.localScale.y;
        raycastDistance = playerHeight / 2f + 0.1f;

        currentSpeed = MoveSpeed;

        if (dome != null)
        {
            domeCenter = dome.transform.position;
            float maxScale = Mathf.Max(dome.transform.localScale.x, dome.transform.localScale.y, dome.transform.localScale.z);
            domeRadius = dome.GetComponent<SphereCollider>().radius * maxScale;
        }
        else Debug.LogError("[PlayerMovement] Dome object not assigned!");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[PlayerMovement] OnNetworkSpawn. IsOwner={IsOwner}, IsServer={IsServer}");
        if (IsOwner)
        {
            // Отключаем сетевые компоненты, чтобы локальная физика не перетиралась
            var nt = GetComponent<NetworkTransform>();
            if (nt != null)
            {
                nt.enabled = false;
                Debug.Log("[PlayerMovement] Disabled NetworkTransform for owner");
            }

            var nr = GetComponent<NetworkRigidbody>();
            if (nr != null)
            {
                nr.enabled = false;
                Debug.Log("[PlayerMovement] Disabled NetworkRigidbody for owner");
            }

            // Включаем инвентарь
            if (UIManager.Instance != null && UIManager.Instance.inventoryUI != null)
            {
                var inv = GetComponent<PlayerInventory>();
                UIManager.Instance.inventoryUI.Init(inv);
                Debug.Log("[PlayerMovement] Inventory UI initialized.");
            }

            lastPosition = transform.position;
        }
        else
        {
            // на клиентах без права владения — полностью выключаем скрипт
            enabled = false;
            var cam = GetComponentInChildren<Camera>();
            if (cam) cam.gameObject.SetActive(false);
            var listener = GetComponentInChildren<AudioListener>();
            if (listener) listener.enabled = false;
        }
    }

    private void Update()
    {
        if (!IsOwner || UIManager.Instance.IsUIOpen) return;

        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveForward = Input.GetAxisRaw("Vertical");

        // Sprint
        bool wantsSprint = Input.GetKey(KeyCode.LeftShift)
                           && (moveHorizontal != 0f || moveForward != 0f)
                           && !isCrouching;
        if (wantsSprint != IsSprinting)
        {
            IsSprinting = wantsSprint;
            OnSprintStateChanged?.Invoke(IsSprinting);
        }
        currentSpeed = IsSprinting ? sprintSpeed : MoveSpeed;

        // Crouch toggle
        if (Input.GetKeyDown(KeyCode.C))
            ToggleCrouch();

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
            Jump();

        // Ground check после прыжка
        if (!isGrounded && groundCheckTimer <= 0f)
        {
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            isGrounded = Physics.Raycast(origin, Vector3.down, raycastDistance, groundLayer);
        }
        else groundCheckTimer -= Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (!IsOwner || UIManager.Instance.IsUIOpen) return;

        // поворот камеры
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.Rotate(0f, mx, 0f);
        verticalRotation = Mathf.Clamp(verticalRotation - my, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        // отладка рывков
        float jump = Vector3.Distance(transform.position, lastPosition);
        if (jump > 0.1f)

        lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (!IsOwner || UIManager.Instance.IsUIOpen) return;

        MovePlayer();
        ApplyJumpPhysics();
        ConstrainToDome();
    }

    private void MovePlayer()
    {
        Vector3 dir = (transform.right * moveHorizontal + transform.forward * moveForward).normalized;
        float speed = isCrouching ? crouchSpeed : currentSpeed;
        Vector3 desired = dir * speed;

        Vector3 vel = rb.linearVelocity;
        vel.x = desired.x;
        vel.z = desired.z;
        if (isGrounded && moveHorizontal == 0f && moveForward == 0f)
        {
            vel.x = vel.z = 0f;
        }
        rb.linearVelocity = vel;

        float mag = new Vector3(moveHorizontal, 0, moveForward).magnitude;
        float norm = mag == 0f ? 0f : (IsSprinting ? 1f : 0.5f);
        animator.SetFloat("Speed", norm);
    }

    private void Jump()
    {
        isGrounded = false;
        groundCheckTimer = groundCheckDelay;
        Vector3 v = rb.linearVelocity;
        v.y = jumpForce;
        rb.linearVelocity = v;
        animator.SetTrigger("Jump");
    }

    private void ApplyJumpPhysics()
    {
        Vector3 v = rb.linearVelocity;
        if (v.y < 0f)
            v.y += Physics.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        else if (v.y > 0f && !Input.GetButton("Jump"))
            v.y += Physics.gravity.y * (ascendMultiplier - 1f) * Time.fixedDeltaTime;
        rb.linearVelocity = v;
    }

    private void ToggleCrouch()
    {
        isCrouching = !isCrouching;
        var cap = GetComponent<CapsuleCollider>();
        if (isCrouching)
        {
            cap.height = crouchHeight;
            cameraTransform.localPosition = new Vector3(0f, crouchHeight / 2f, 0f);
        }
        else
        {
            cap.height = originalHeight;
            cameraTransform.localPosition = new Vector3(0f, originalHeight / 2f, 0f);
        }
    }

    private void ConstrainToDome()
    {
        if (dome == null) return;

        domeCenter = dome.transform.position;
        float maxScale = Mathf.Max(dome.transform.localScale.x, dome.transform.localScale.y, dome.transform.localScale.z);
        domeRadius = dome.GetComponent<SphereCollider>().radius * maxScale;

        Vector3 offset = transform.position - domeCenter;
        if (offset.magnitude > domeRadius)
        {
            Vector3 corrected = domeCenter + offset.normalized * domeRadius;
            rb.MovePosition(corrected); 
            Vector3 vel = rb.linearVelocity;
            Vector3 outVel = Vector3.Project(vel, offset.normalized);
            rb.linearVelocity -= outVel;
        }
    }
}
