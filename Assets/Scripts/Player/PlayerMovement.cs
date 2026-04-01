using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float jumpHeight = 2f;
    public float gravity = -15f;

    [Header("Look Settings")]
    public float mouseSensitivity = 200f;
    public Transform playerCamera;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    private bool isGrounded;
    private bool jumpPressed = false;

    // Crouch
    private float originalHeight;
    private float crouchHeight = 1f;
    private Vector3 originalCameraPos;
    private Vector3 crouchCameraPos = new Vector3(0, 0.2f, 0);
    private bool isCrouching = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Save original values for crouch
        originalHeight = controller.height;
        originalCameraPos = playerCamera.localPosition;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
        HandleGravity();
        HandleCrouch();

        // Capture jump in Update so it never gets missed
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            jumpPressed = true;
    }

    void HandleLook()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current.dKey.isPressed) moveX = 1f;
        if (Keyboard.current.aKey.isPressed) moveX = -1f;
        if (Keyboard.current.wKey.isPressed) moveZ = 1f;
        if (Keyboard.current.sKey.isPressed) moveZ = -1f;

        // Slower when crouching
        float speed = isCrouching ? walkSpeed * 0.5f :
                      Keyboard.current.leftShiftKey.isPressed ? runSpeed : walkSpeed;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * speed * Time.deltaTime);
    }

    void HandleGravity()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Use the jumpPressed flag instead of wasPressedThisFrame
        if (jumpPressed && isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpPressed = false;
        }
        else
        {
            jumpPressed = false;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleCrouch()
    {
        // Press C to crouch, release C to stand
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            isCrouching = true;
            controller.height = crouchHeight;
            playerCamera.localPosition = crouchCameraPos;
        }

        if (Keyboard.current.cKey.wasReleasedThisFrame)
        {
            // Check there is space above before standing up
            if (!Physics.Raycast(transform.position, Vector3.up, originalHeight))
            {
                isCrouching = false;
                controller.height = originalHeight;
                playerCamera.localPosition = originalCameraPos;
            }
        }
    }
}
