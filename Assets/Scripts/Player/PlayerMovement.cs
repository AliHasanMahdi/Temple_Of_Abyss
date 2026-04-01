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

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
        HandleGravity();
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
        Vector2 moveInput = new Vector2(
            Keyboard.current.dKey.isPressed ? 1 : Keyboard.current.aKey.isPressed ? -1 : 0,
            Keyboard.current.wKey.isPressed ? 1 : Keyboard.current.sKey.isPressed ? -1 : 0
        );

        bool isRunning = Keyboard.current.leftShiftKey.isPressed;
        float speed = isRunning ? runSpeed : walkSpeed;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * speed * Time.deltaTime);
    }

    void HandleGravity()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
