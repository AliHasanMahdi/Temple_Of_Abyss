using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float jumpHeight = 2f;
    public float gravity = -15f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 120f;
    public Transform playerCamera;

    [Header("Jump")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("Crouch")]
    public float crouchHeight = 1f;
    public float crouchSpeed = 8f;

    [Header("FOV")]
    public Camera cam;
    public float normalFOV = 60f;
    public float sprintFOV = 75f;
    public float fovSpeed = 8f;

    [Header("Head Bob")]
    public float bobSpeed = 14f;
    public float bobAmount = 0.05f;

    private CharacterController controller;
    private PauseMenu pauseMenu;
    private Vector3 velocity;
    private float xRotation;
    private bool isGrounded;
    private bool isCrouching;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private float originalHeight;
    private float targetHeight;
    private Vector3 originalCameraPos;
    private Vector3 crouchCameraPos;
    private Vector3 targetCameraPos;
    private float bobTimer;
    private float moveInputMagnitude;
    private bool isRunning;

    public bool IsGrounded => isGrounded;
    public bool IsMoving => moveInputMagnitude > 0.1f;
    public bool IsRunning => isRunning && IsMoving && !isCrouching;
    public bool IsCrouching => isCrouching;
    public float VerticalVelocity => velocity.y;
    public Transform ViewTransform => playerCamera;

    void Start()
    {
        Time.timeScale = 1f;
        controller = GetComponent<CharacterController>();
        pauseMenu = FindFirstObjectByType<PauseMenu>();

        EnsureFirstPersonCamera();
        EnsureBodyVisual();
        EnsureSupportComponents();
        PromotePlayerCamera();

        originalHeight = Mathf.Max(controller.height, 2f);
        targetHeight = originalHeight;
        originalCameraPos = playerCamera.localPosition;
        crouchCameraPos = originalCameraPos - new Vector3(0f, 0.55f, 0f);
        targetCameraPos = originalCameraPos;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        pauseMenu ??= FindFirstObjectByType<PauseMenu>();

        if (pauseMenu != null && pauseMenu.IsPaused)
            return;

        HandleLook();
        HandleMovement();
        HandleGravity();
        HandleCrouch();
        HandleHeadBob();
        HandleFOV();
    }

    void EnsureFirstPersonCamera()
    {
        if (playerCamera == null)
        {
            Transform existingCamera = transform.Find("PlayerCamera");
            if (existingCamera != null)
            {
                playerCamera = existingCamera;
            }
            else
            {
                GameObject cameraObject = new GameObject("PlayerCamera");
                playerCamera = cameraObject.transform;
                playerCamera.SetParent(transform, false);
                playerCamera.localPosition = new Vector3(0f, 0.75f, 0f);
            }
        }

        cam ??= playerCamera.GetComponent<Camera>();
        if (cam == null)
            cam = playerCamera.gameObject.AddComponent<Camera>();

        if (playerCamera.GetComponent<AudioListener>() == null)
            playerCamera.gameObject.AddComponent<AudioListener>();

        if (playerCamera.localPosition == Vector3.zero)
            playerCamera.localPosition = new Vector3(0f, 0.75f, 0f);
    }

    void EnsureBodyVisual()
    {
        if (transform.Find("BodyVisual") != null)
            return;

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "BodyVisual";
        body.transform.SetParent(transform, false);
        body.transform.localPosition = new Vector3(0f, -0.2f, 0f);
        body.transform.localScale = new Vector3(0.45f, 0.85f, 0.45f);

        Collider bodyCollider = body.GetComponent<Collider>();
        if (bodyCollider != null)
            Destroy(bodyCollider);

        Renderer bodyRenderer = body.GetComponent<Renderer>();
        if (bodyRenderer != null)
        {
            bodyRenderer.shadowCastingMode = ShadowCastingMode.On;
            bodyRenderer.receiveShadows = true;
            bodyRenderer.material.color = new Color(0.72f, 0.72f, 0.78f, 1f);
        }
    }

    void EnsureSupportComponents()
    {
        if (GetComponent<PlayerHealth>() == null)
            gameObject.AddComponent<PlayerHealth>();

        if (GetComponent<PlayerInteraction>() == null)
            gameObject.AddComponent<PlayerInteraction>();

        if (GetComponent<PlayerVisualAnimator>() == null)
            gameObject.AddComponent<PlayerVisualAnimator>();
    }

    void PromotePlayerCamera()
    {
        if (cam == null)
            return;

        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera sceneCamera in cameras)
        {
            bool isPlayerCamera = sceneCamera == cam;
            sceneCamera.enabled = isPlayerCamera;

            AudioListener listener = sceneCamera.GetComponent<AudioListener>();
            if (listener != null)
                listener.enabled = isPlayerCamera;
        }

        cam.tag = "MainCamera";
        cam.enabled = true;
    }

    void HandleLook()
    {
        if (playerCamera == null)
            return;

        Vector2 mouseDelta = ReadMouseDelta();
        if (mouseDelta.sqrMagnitude <= 0f)
            return;

        float mouseX = mouseDelta.x;
        float mouseY = mouseDelta.y;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        Vector2 moveInput = ReadMoveInput();
        float moveX = moveInput.x;
        float moveZ = moveInput.y;

        bool isSprinting = IsSprintPressed();
        float speed = isCrouching ? walkSpeed * 0.5f : isSprinting ? runSpeed : walkSpeed;

        Vector3 move = (transform.right * moveX + transform.forward * moveZ).normalized;
        moveInputMagnitude = move.magnitude;
        isRunning = isSprinting;

        float control = isGrounded ? 1f : 0.5f;
        controller.Move(move * speed * control * Time.deltaTime);
    }

    float GetAxis(KeyControl negativePrimary, KeyControl positivePrimary, KeyControl negativeSecondary, KeyControl positiveSecondary)
    {
        float value = 0f;

        if (negativePrimary.isPressed || negativeSecondary.isPressed)
            value -= 1f;

        if (positivePrimary.isPressed || positiveSecondary.isPressed)
            value += 1f;

        return value;
    }

    Vector2 ReadMoveInput()
    {
        if (Keyboard.current != null)
        {
            float moveX = GetAxis(Keyboard.current.aKey, Keyboard.current.dKey, Keyboard.current.leftArrowKey, Keyboard.current.rightArrowKey);
            float moveZ = GetAxis(Keyboard.current.sKey, Keyboard.current.wKey, Keyboard.current.downArrowKey, Keyboard.current.upArrowKey);
            return new Vector2(moveX, moveZ);
        }

        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    Vector2 ReadMouseDelta()
    {
        if (Mouse.current != null)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            return delta * mouseSensitivity * 0.01f;
        }

        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * mouseSensitivity;
    }

    bool WasJumpPressedThisFrame()
    {
        if (Keyboard.current != null)
            return Keyboard.current.spaceKey.wasPressedThisFrame;

        return Input.GetKeyDown(KeyCode.Space);
    }

    bool IsJumpHeld()
    {
        if (Keyboard.current != null)
            return Keyboard.current.spaceKey.isPressed;

        return Input.GetKey(KeyCode.Space);
    }

    bool IsSprintPressed()
    {
        if (Keyboard.current != null)
            return Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;

        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    bool IsCrouchPressed()
    {
        if (Keyboard.current != null)
            return Keyboard.current.cKey.isPressed || Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;

        return Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }

    void HandleGravity()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;

            if (velocity.y < 0f)
                velocity.y = -2f;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (WasJumpPressedThisFrame())
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        if (jumpBufferTimer > 0f && coyoteTimer > 0f && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferTimer = 0f;
        }

        if (velocity.y < 0f)
        {
            velocity.y += gravity * (fallMultiplier - 1f) * Time.deltaTime;
        }
        else if (velocity.y > 0f && !IsJumpHeld())
        {
            velocity.y += gravity * (lowJumpMultiplier - 1f) * Time.deltaTime;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleCrouch()
    {
        bool crouchPressed = IsCrouchPressed();

        if (crouchPressed)
        {
            isCrouching = true;
            targetHeight = crouchHeight;
            targetCameraPos = crouchCameraPos;
        }
        else if (!Physics.Raycast(transform.position, Vector3.up, originalHeight))
        {
            isCrouching = false;
            targetHeight = originalHeight;
            targetCameraPos = originalCameraPos;
        }

        controller.height = Mathf.Lerp(controller.height, targetHeight, crouchSpeed * Time.deltaTime);
    }

    void HandleHeadBob()
    {
        if (playerCamera == null)
            return;

        float moveAmount = new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;

        if (controller.isGrounded && moveAmount > 0.1f)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            float bobOffset = Mathf.Sin(bobTimer) * bobAmount;

            Vector3 newPos = targetCameraPos;
            newPos.y += bobOffset;
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, newPos, 10f * Time.deltaTime);
        }
        else
        {
            bobTimer = 0f;
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, targetCameraPos, 10f * Time.deltaTime);
        }
    }

    void HandleFOV()
    {
        if (cam == null)
            return;

        bool isSprinting = IsSprintPressed();
        float targetFOVValue = isSprinting ? sprintFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOVValue, fovSpeed * Time.deltaTime);
    }
}
