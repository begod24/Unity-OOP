using UnityEngine;

// Simple FPS-style controller using CharacterController.
// Attach to a Capsule that also has a CharacterController component.
[RequireComponent(typeof(CharacterController))]
public sealed class PlayerCapsuleController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runMultiplier = 1.6f;
    [SerializeField] private float gravity = -18f;
    [SerializeField] private float jumpHeight = 1.2f;

    [Header("Mouse Look")]
    [SerializeField] private Transform cameraPivot; // Assign your child Camera transform
    [SerializeField] private float mouseSensitivity = 120f; // degrees per second
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    private CharacterController controller;
    private float verticalVelocity;
    private float pitch; // camera X rotation accumulator

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraPivot == null)
            cameraPivot = GetComponentInChildren<Camera>()?.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Look();
        Move();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Yaw (body)
        transform.Rotate(Vector3.up, mouseX);

        // Pitch (camera)
        pitch = Mathf.Clamp(pitch - mouseY, minPitch, maxPitch);
        if (cameraPivot != null)
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void Move()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A/D
        float v = Input.GetAxisRaw("Vertical");   // W/S
        Vector3 input = Vector3.ClampMagnitude(new Vector3(h, 0f, v), 1f);

        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? runMultiplier : 1f);
        Vector3 move = (transform.forward * input.z + transform.right * input.x) * speed;

        // Gravity & jump
        if (controller.isGrounded)
        {
            verticalVelocity = -1f;
            if (Input.GetKeyDown(KeyCode.Space))
                verticalVelocity = Mathf.Sqrt(-2f * gravity * jumpHeight);
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        move.y = verticalVelocity;
        controller.Move(move * Time.deltaTime);
    }
}

