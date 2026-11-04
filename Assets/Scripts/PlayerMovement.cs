
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Camera")]
    public Transform playerCamera;
    public float mouseSensitivity = 100f;
    public float verticalLimit = 90f;
    public float horizontalLimit = 90f;

    [Header("Turn Around")]
    public float turnDuration = 0.4f;

    private float xRotation;
    private float yRotation;

    private bool isTurning;
    private bool isTurnedAround;

    private CharacterController controller;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMovement();
        HandleCamera();
        HandleTurnAround();
    }


    private void HandleMovement()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) input.y += 1;
            if (Keyboard.current.sKey.isPressed) input.y -= 1;
            if (Keyboard.current.aKey.isPressed) input.x -= 1;
            if (Keyboard.current.dKey.isPressed) input.x += 1;
        }

        // Get movement directions from the camera
        Vector3 forward = playerCamera.forward;
        Vector3 right = playerCamera.right;

        // Keep movement horizontal
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 move = (forward * input.y + right * input.x).normalized;

        if (controller != null)
            controller.Move(move * moveSpeed * Time.deltaTime);
        else
            transform.position += move * moveSpeed * Time.deltaTime;
    }


    private void HandleCamera()
    {
        if (isTurning || Mouse.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;

        // Vertical camera rotation
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -verticalLimit, verticalLimit);

        // Horizontal camera rotation
        yRotation += mouseX;
        yRotation = Mathf.Clamp(yRotation, -horizontalLimit, horizontalLimit);

        playerCamera.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }

    private void HandleTurnAround()
    {
        if (Keyboard.current != null &&
            Keyboard.current.leftShiftKey.wasPressedThisFrame ||
            Keyboard.current != null &&
            Keyboard.current.rightShiftKey.wasPressedThisFrame)
        {
            if (!isTurning)
                StartCoroutine(TurnAround());
        }
    }

    private IEnumerator TurnAround()
    {
        isTurning = true;

        float startRotation = transform.eulerAngles.y;
        float targetRotation = startRotation + 180f;

        float elapsed = 0f;

        while (elapsed < turnDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / turnDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            transform.rotation = Quaternion.Euler(
                0f,
                Mathf.LerpAngle(startRotation, targetRotation, smoothT),
                0f
            );

            yield return null;
        }

        transform.rotation = Quaternion.Euler(0f, targetRotation, 0f);

        // Reset horizontal camera angle after turning
        yRotation = 0f;

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        isTurnedAround = !isTurnedAround;
        isTurning = false;
    }
}
