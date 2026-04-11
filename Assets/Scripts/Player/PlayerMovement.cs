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
        controller = GetComponent<CharacterController>();
        pauseMenu = FindFirstObjectByType<PauseMenu>();

        EnsureFirstPersonCamera();
        EnsureBodyVisual();

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

        if (Keyboard.current == null)
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

    void HandleLook()
    {
        if (Mouse.current == null || playerCamera == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float mouseX = mouseDelta.x * mouseSensitivity * 0.01f;
        float mouseY = mouseDelta.y * mouseSensitivity * 0.01f;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        float moveX = GetAxis(Keyboard.current.aKey, Keyboard.current.dKey, Keyboard.current.leftArrowKey, Keyboard.current.rightArrowKey);
        float moveZ = GetAxis(Keyboard.current.sKey, Keyboard.current.wKey, Keyboard.current.downArrowKey, Keyboard.current.upArrowKey);

        bool isSprinting = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
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

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
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
        else if (velocity.y > 0f && !Keyboard.current.spaceKey.isPressed)
        {
            velocity.y += gravity * (lowJumpMultiplier - 1f) * Time.deltaTime;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleCrouch()
    {
        bool crouchPressed = Keyboard.current.cKey.isPressed || Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;

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

        bool isSprinting = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
        float targetFOVValue = isSprinting ? sprintFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOVValue, fovSpeed * Time.deltaTime);
    }
}

public class PlayerInteraction : MonoBehaviour
{
    public float interactionRange = 4f;
    public LayerMask interactionMask = ~0;

    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private PauseMenu pauseMenu;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    void OnDisable()
    {
        if (HUDManager.Instance != null)
            HUDManager.Instance.HideInteractionPrompt();
    }

    void Update()
    {
        pauseMenu ??= FindFirstObjectByType<PauseMenu>();

        if (pauseMenu != null && pauseMenu.IsPaused)
        {
            HidePrompt();
            return;
        }

        if (Keyboard.current == null || playerMovement == null || playerMovement.ViewTransform == null)
        {
            HidePrompt();
            return;
        }

        if (playerHealth != null && playerHealth.IsDead)
        {
            HidePrompt();
            return;
        }

        Interactable interactable = FindInteractable();
        if (interactable == null || !interactable.CanInteract(gameObject))
        {
            HidePrompt();
            return;
        }

        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowInteractionPrompt(interactable.GetPromptText());

        if (Keyboard.current.eKey.wasPressedThisFrame)
            interactable.Interact(gameObject);
    }

    Interactable FindInteractable()
    {
        Ray ray = new Ray(playerMovement.ViewTransform.position, playerMovement.ViewTransform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionMask, QueryTriggerInteraction.Collide))
            return null;

        return hit.collider.GetComponentInParent<Interactable>();
    }

    void HidePrompt()
    {
        if (HUDManager.Instance != null)
            HUDManager.Instance.HideInteractionPrompt();
    }
}

public class PlayerVisualAnimator : MonoBehaviour
{
    enum VisualState
    {
        Idle,
        Walk,
        Run,
        Jump,
        Death
    }

    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private Transform bodyVisual;
    private Renderer bodyRenderer;
    private Vector3 baseScale;
    private Vector3 basePosition;
    private Quaternion baseRotation;
    private VisualState currentState;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (bodyVisual == null)
            CacheBodyVisual();

        if (bodyVisual == null || playerMovement == null)
            return;

        currentState = ResolveState();
        AnimateState();
    }

    void CacheBodyVisual()
    {
        bodyVisual = transform.Find("BodyVisual");
        if (bodyVisual == null)
            return;

        bodyRenderer = bodyVisual.GetComponent<Renderer>();
        baseScale = bodyVisual.localScale;
        basePosition = bodyVisual.localPosition;
        baseRotation = bodyVisual.localRotation;
    }

    VisualState ResolveState()
    {
        if (playerHealth != null && playerHealth.IsDead)
            return VisualState.Death;

        if (!playerMovement.IsGrounded)
            return VisualState.Jump;

        if (playerMovement.IsRunning)
            return VisualState.Run;

        if (playerMovement.IsMoving)
            return VisualState.Walk;

        return VisualState.Idle;
    }

    void AnimateState()
    {
        float time = Time.time;

        switch (currentState)
        {
            case VisualState.Idle:
                bodyVisual.localPosition = basePosition + new Vector3(0f, Mathf.Sin(time * 1.5f) * 0.03f, 0f);
                bodyVisual.localScale = Vector3.Lerp(bodyVisual.localScale, baseScale, 8f * Time.deltaTime);
                bodyVisual.localRotation = Quaternion.Lerp(bodyVisual.localRotation, baseRotation, 8f * Time.deltaTime);
                SetColor(new Color(0.72f, 0.72f, 0.78f, 1f));
                break;

            case VisualState.Walk:
                bodyVisual.localPosition = basePosition + new Vector3(0f, Mathf.Abs(Mathf.Sin(time * 7f)) * 0.08f, 0f);
                bodyVisual.localScale = Vector3.Lerp(bodyVisual.localScale, baseScale + new Vector3(0.03f, -0.02f, 0.03f), 8f * Time.deltaTime);
                bodyVisual.localRotation = Quaternion.Lerp(bodyVisual.localRotation, Quaternion.Euler(0f, 0f, Mathf.Sin(time * 7f) * 3f), 10f * Time.deltaTime);
                SetColor(new Color(0.6f, 0.76f, 0.88f, 1f));
                break;

            case VisualState.Run:
                bodyVisual.localPosition = basePosition + new Vector3(0f, Mathf.Abs(Mathf.Sin(time * 10f)) * 0.12f, 0f);
                bodyVisual.localScale = Vector3.Lerp(bodyVisual.localScale, baseScale + new Vector3(0.05f, -0.05f, 0.05f), 10f * Time.deltaTime);
                bodyVisual.localRotation = Quaternion.Lerp(bodyVisual.localRotation, Quaternion.Euler(0f, 0f, Mathf.Sin(time * 10f) * 6f), 12f * Time.deltaTime);
                SetColor(new Color(0.9f, 0.66f, 0.3f, 1f));
                break;

            case VisualState.Jump:
                bodyVisual.localPosition = Vector3.Lerp(bodyVisual.localPosition, basePosition + new Vector3(0f, 0.08f, 0f), 8f * Time.deltaTime);
                bodyVisual.localScale = Vector3.Lerp(bodyVisual.localScale, baseScale + new Vector3(-0.08f, 0.12f, -0.08f), 8f * Time.deltaTime);
                bodyVisual.localRotation = Quaternion.Lerp(bodyVisual.localRotation, Quaternion.Euler(-10f, 0f, 0f), 8f * Time.deltaTime);
                SetColor(new Color(0.48f, 0.9f, 0.74f, 1f));
                break;

            case VisualState.Death:
                bodyVisual.localPosition = Vector3.Lerp(bodyVisual.localPosition, basePosition + new Vector3(0f, -0.5f, 0f), 4f * Time.deltaTime);
                bodyVisual.localScale = Vector3.Lerp(bodyVisual.localScale, baseScale, 4f * Time.deltaTime);
                bodyVisual.localRotation = Quaternion.Lerp(bodyVisual.localRotation, Quaternion.Euler(0f, 0f, 90f), 4f * Time.deltaTime);
                SetColor(new Color(0.24f, 0.12f, 0.12f, 1f));
                break;
        }
    }

    void SetColor(Color color)
    {
        if (bodyRenderer != null)
            bodyRenderer.material.color = color;
    }
}
