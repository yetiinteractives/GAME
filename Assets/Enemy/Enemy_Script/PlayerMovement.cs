using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Look Settings")]
    public Transform playerCamera;
    public float lookSensitivity = 2f;
    public float maxLookAngle = 80f;

    private InputAction sprintAction;
    private CharacterController controller;
    private Vector3 velocity;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float xRotation;

    private bool jumpPressed;
    private bool isSprinting;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        var playerInput = GetComponent<PlayerInput>();
        sprintAction = playerInput.actions["Sprint"];

    }

    void Update()
    {
        isSprinting = sprintAction.IsPressed();
        HandleMovement();
        HandleLook();
        ApplyGravity();
    }

    void HandleMovement()
    {
        float speed = isSprinting ? sprintSpeed : walkSpeed;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * speed * Time.deltaTime);

        if (jumpPressed && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        jumpPressed = false;

        controller.Move(velocity * Time.deltaTime);
    }

    void HandleLook()
    {
        xRotation -= lookInput.y * lookSensitivity;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * lookInput.x * lookSensitivity);
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
    }

    // ---- Input Callbacks ----
    public void OnMove(InputValue value) => moveInput = value.Get<Vector2>();
    public void OnLook(InputValue value) => lookInput = value.Get<Vector2>();
    public void OnJump(InputValue value) => jumpPressed = true;
    public void OnSprint(InputValue value) => isSprinting = value.Get<float>() > 0f;
}
