using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float jumpHeight = 1f;
    public float gravity = -15f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 200f;
    public Transform playerCamera;

    [Header("Jump")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask = ~0;

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

    [Header("Footsteps")]
    public AudioSource footstepAudioSource;
    public AudioClip[] walkFootstepClips;
    public AudioClip[] runFootstepClips;
    public float walkStepRate = 2.2f;
    public float runStepRate = 3.3f;
    public float footstepVolume = 0.28f;

    private CharacterController controller;
    private Vector3 currentHorizontalMove;
    private Vector3 velocity;
    private float xRotation = 0f;

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
    private float footstepTimer;

    public Transform ViewTransform => playerCamera != null ? playerCamera : transform;
    public bool IsGrounded => isGrounded;
    public bool IsMoving
    {
        get
        {
            return currentHorizontalMove.sqrMagnitude > 0.01f || HasMovementInput();
        }
    }

    public bool IsRunning
    {
        get
        {
            if (!IsMoving || isCrouching)
                return false;

            return Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        }
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        originalHeight = controller.height;
        targetHeight = originalHeight;

        originalCameraPos = playerCamera.localPosition;
        crouchCameraPos = originalCameraPos - new Vector3(0, 1.0f, 0);
        targetCameraPos = originalCameraPos;

        ConfigureFootsteps();
    }

    void OnEnable()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // ESC is handled ONLY by PauseMenu — do not handle it here
        // This stops the double-pause conflict

        // If game is paused, stop all player input
        if (Time.timeScale == 0f) return;

        HandleLook();
        HandleMovement();
        HandleGravity();
        HandleCrouch();
        HandleHeadBob();
        HandleFOV();
        HandleFootsteps();
    }

    void HandleLook()
    {
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
        if (controller == null || !controller.enabled)
        {
            currentHorizontalMove = Vector3.zero;
            return;
        }

        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current.aKey.isPressed) moveX = -1f;
        if (Keyboard.current.dKey.isPressed) moveX = 1f;
        if (Keyboard.current.wKey.isPressed) moveZ = 1f;
        if (Keyboard.current.sKey.isPressed) moveZ = -1f;

        float speed = isCrouching ? walkSpeed * 0.5f :
                      Keyboard.current.leftShiftKey.isPressed ? runSpeed : walkSpeed;

        Vector3 move = (transform.right * moveX + transform.forward * moveZ).normalized;

        float control = isGrounded ? 1f : 0.5f;
        currentHorizontalMove = move * speed * control;
        controller.Move(currentHorizontalMove * Time.deltaTime);
    }

    void HandleGravity()
    {
        if (controller == null || !controller.enabled) return;

        isGrounded = controller.isGrounded || IsTouchingGround();

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;

            // Reset vertical velocity when grounded — this stops falling through floor
            if (velocity.y < 0)
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

        if (velocity.y < 0)
            velocity.y += gravity * (fallMultiplier - 1) * Time.deltaTime;
        else if (velocity.y > 0 && !Keyboard.current.spaceKey.isPressed)
            velocity.y += gravity * (lowJumpMultiplier - 1) * Time.deltaTime;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    bool IsTouchingGround()
    {
        Vector3 center = transform.TransformPoint(controller.center);
        float bottomOffset = (controller.height * 0.5f) - controller.radius;
        float checkRadius = Mathf.Max(0.05f, controller.radius * 0.9f);
        float checkDistance = bottomOffset + groundCheckDistance;

        RaycastHit[] hits = Physics.SphereCastAll(
            center,
            checkRadius,
            Vector3.down,
            checkDistance,
            groundMask,
            QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null) continue;
            if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
            return true;
        }

        return false;
    }

    void HandleCrouch()
    {
        if (Keyboard.current.cKey.isPressed)
        {
            isCrouching = true;
            targetHeight = crouchHeight;
            targetCameraPos = crouchCameraPos;
        }
        else
        {
            if (!Physics.Raycast(transform.position, Vector3.up, originalHeight))
            {
                isCrouching = false;
                targetHeight = originalHeight;
                targetCameraPos = originalCameraPos;
            }
        }

        controller.height = Mathf.Lerp(controller.height, targetHeight, crouchSpeed * Time.deltaTime);
    }

    void HandleHeadBob()
    {
        float moveAmount = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;

        if (controller.isGrounded && moveAmount > 0.1f)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            float bobOffset = Mathf.Sin(bobTimer) * bobAmount;

            Vector3 newPos = targetCameraPos;
            newPos.y += bobOffset;

            playerCamera.localPosition = Vector3.Lerp(
                playerCamera.localPosition, newPos, 10f * Time.deltaTime);
        }
        else
        {
            bobTimer = 0;
            playerCamera.localPosition = Vector3.Lerp(
                playerCamera.localPosition, targetCameraPos, 10f * Time.deltaTime);
        }
    }

    void HandleFOV()
    {
        float targetFOV = Keyboard.current.leftShiftKey.isPressed ? sprintFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, fovSpeed * Time.deltaTime);
    }

    void ConfigureFootsteps()
    {
        if (footstepAudioSource == null)
            footstepAudioSource = GetComponent<AudioSource>();
        if (footstepAudioSource == null)
            footstepAudioSource = gameObject.AddComponent<AudioSource>();

        footstepAudioSource.playOnAwake = false;
        footstepAudioSource.loop = false;
        footstepAudioSource.spatialBlend = 0f;
        footstepAudioSource.ignoreListenerPause = true;

        if (walkFootstepClips == null || walkFootstepClips.Length == 0)
        {
            walkFootstepClips = TempleAudio.LoadClips(
                "TempleAudio/Footsteps/Footstep_Boots_01",
                "TempleAudio/Footsteps/Footstep_Boots_02",
                "TempleAudio/Footsteps/Footstep_Boots_03",
                "TempleAudio/Footsteps/Footstep_Boots_04",
                "TempleAudio/Footsteps/Footstep_Boots_05",
                "TempleAudio/Footsteps/Footstep_Boots_06",
                "TempleAudio/Footsteps/Footstep_Boots_07");
        }

        if (runFootstepClips == null || runFootstepClips.Length == 0)
        {
            runFootstepClips = TempleAudio.LoadClips(
                "TempleAudio/Footsteps/Footstep_Deep_01",
                "TempleAudio/Footsteps/Footstep_Deep_02",
                "TempleAudio/Footsteps/Footstep_Deep_03",
                "TempleAudio/Footsteps/Footstep_Deep_04",
                "TempleAudio/Footsteps/Footstep_Deep_05",
                "TempleAudio/Footsteps/Footstep_Deep_06",
                "TempleAudio/Footsteps/Footstep_Deep_07");
        }
    }

    void HandleFootsteps()
    {
        if (footstepAudioSource == null)
            return;

        if (!IsGrounded || !HasMovementInput())
        {
            footstepTimer = 0f;
            return;
        }

        AudioClip[] clips = IsRunning && runFootstepClips != null && runFootstepClips.Length > 0
            ? runFootstepClips
            : walkFootstepClips;
        if (clips == null || clips.Length == 0)
            return;

        float stepRate = IsRunning ? runStepRate : walkStepRate;
        if (stepRate <= 0f)
            return;

        footstepTimer += Time.deltaTime;
        if (footstepTimer < 1f / stepRate)
            return;

        footstepTimer = 0f;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip == null)
            return;

        footstepAudioSource.pitch = Random.Range(0.94f, 1.06f);
        footstepAudioSource.PlayOneShot(clip, TempleAudio.ScaleSfxVolume(footstepVolume));
        TempleAudio.PlaySfx(clip, footstepVolume);
    }

    bool HasMovementInput()
    {
        if (Keyboard.current == null)
            return false;

        return Keyboard.current.wKey.isPressed ||
               Keyboard.current.aKey.isPressed ||
               Keyboard.current.sKey.isPressed ||
               Keyboard.current.dKey.isPressed;
    }

    // Called by PauseMenu to lock/unlock cursor
    public void OnPause()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // Reset velocity so player doesn't fall through floor on resume
        currentHorizontalMove = Vector3.zero;
        velocity = Vector3.zero;
    }

    public void OnResume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Reset velocity again on resume to be safe
        currentHorizontalMove = Vector3.zero;
        velocity = Vector3.zero;
    }
}
