using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float jumpHeight = 2f;
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

    // ── FOOTSTEP SOUNDS ───────────────────────────────────────────
    [Header("Footstep Sounds")]
    [Tooltip("AudioSource on this GameObject used for footsteps")]
    public AudioSource footstepAudioSource;

    [Tooltip("Clips played while walking (e.g. Footstep_Boots_01..07)")]
    public AudioClip[] walkFootstepClips;

    [Tooltip("Clips played while sprinting (e.g. Footstep_Deep_01..07)")]
    public AudioClip[] runFootstepClips;

    [Tooltip("Steps per second while walking")]
    public float walkStepRate = 1.8f;

    [Tooltip("Steps per second while running")]
    public float runStepRate = 2.8f;

    [Range(0f, 1f)]
    public float footstepVolume = 0.5f;
    // ─────────────────────────────────────────────────────────────

    private CharacterController controller;
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

    // Footstep timing
    private float footstepTimer = 0f;
    private bool hasMovementInput = false;

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

        // Auto-create AudioSource if not assigned
        if (footstepAudioSource == null)
        {
            footstepAudioSource = gameObject.AddComponent<AudioSource>();
            footstepAudioSource.spatialBlend = 0f; // 2D sound for player
            footstepAudioSource.playOnAwake = false;
        }

        if (!HasPlayableClips(walkFootstepClips))
            walkFootstepClips = TempleAudio.LoadClips(
                "TempleAudio/Footsteps/Footstep_Boots_01",
                "TempleAudio/Footsteps/Footstep_Boots_02",
                "TempleAudio/Footsteps/Footstep_Boots_03",
                "TempleAudio/Footsteps/Footstep_Boots_04",
                "TempleAudio/Footsteps/Footstep_Boots_05",
                "TempleAudio/Footsteps/Footstep_Boots_06",
                "TempleAudio/Footsteps/Footstep_Boots_07");

        if (!HasPlayableClips(runFootstepClips))
            runFootstepClips = TempleAudio.LoadClips(
                "TempleAudio/Footsteps/Footstep_Deep_01",
                "TempleAudio/Footsteps/Footstep_Deep_02",
                "TempleAudio/Footsteps/Footstep_Deep_03",
                "TempleAudio/Footsteps/Footstep_Deep_04",
                "TempleAudio/Footsteps/Footstep_Deep_05",
                "TempleAudio/Footsteps/Footstep_Deep_06",
                "TempleAudio/Footsteps/Footstep_Deep_07");
    }

    void OnEnable()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        HandleLook();
        HandleMovement();
        HandleGravity();
        HandleCrouch();
        HandleHeadBob();
        HandleFOV();
        HandleFootsteps();   // <-- new
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
        if (controller == null || !controller.enabled) return;

        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current.aKey.isPressed) moveX = -1f;
        if (Keyboard.current.dKey.isPressed) moveX = 1f;
        if (Keyboard.current.wKey.isPressed) moveZ = 1f;
        if (Keyboard.current.sKey.isPressed) moveZ = -1f;
        hasMovementInput = Mathf.Abs(moveX) > 0f || Mathf.Abs(moveZ) > 0f;

        float speed = isCrouching ? walkSpeed * 0.5f :
                      Keyboard.current.leftShiftKey.isPressed ? runSpeed : walkSpeed;

        Vector3 move = (transform.right * moveX + transform.forward * moveZ).normalized;

        float control = isGrounded ? 1f : 0.5f;
        controller.Move(move * speed * control * Time.deltaTime);
    }

    void HandleGravity()
    {
        if (controller == null || !controller.enabled) return;

        isGrounded = controller.isGrounded || IsTouchingGround();

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;

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

    // ── FOOTSTEP SOUND ──────────────────────────────────────────
    void HandleFootsteps()
    {
        if (!isGrounded) return;
        if (footstepAudioSource == null) return;

        if (!hasMovementInput)
        {
            footstepTimer = 0f;
            return;
        }

        bool isSprinting = Keyboard.current.leftShiftKey.isPressed && !isCrouching;
        float stepRate = isSprinting ? runStepRate : walkStepRate;
        AudioClip[] clips = (isSprinting && runFootstepClips != null && runFootstepClips.Length > 0)
            ? runFootstepClips
            : walkFootstepClips;

        footstepTimer += Time.deltaTime;

        if (footstepTimer >= 1f / stepRate)
        {
            footstepTimer = 0f;
            PlayRandomFootstep(clips);
        }
    }

    void PlayRandomFootstep(AudioClip[] clips)
    {
        if (!HasPlayableClips(clips)) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        for (int i = 0; clip == null && i < clips.Length; i++)
            clip = clips[i];

        footstepAudioSource.pitch = Random.Range(0.92f, 1.08f); // slight pitch variation
<<<<<<< Updated upstream
        footstepAudioSource.PlayOneShot(clip, footstepVolume);
=======
        footstepAudioSource.PlayOneShot(clip, TempleAudio.ScaleSfxVolume(footstepVolume));
>>>>>>> Stashed changes
    }

    bool HasPlayableClips(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return false;
        foreach (AudioClip clip in clips)
        {
            if (clip != null) return true;
        }
        return false;
    }
    // ────────────────────────────────────────────────────────────

    public void OnPause()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        velocity = Vector3.zero;
    }

    public void OnResume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        velocity = Vector3.zero;
    }
}
