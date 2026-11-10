
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Gravity")]
    public float gravity = -20f;

    private float verticalVelocity;

    [Header("Camera")]
    public Transform playerCamera;
    public float mouseSensitivity = 100f;
    public float verticalLimit = 90f;
    public float horizontalLimit = 90f;

   
    public float cameraFreezeDuration = 2f;

    private bool cameraControlEnabled = false;

    [Header("Start Fade")]
    public Image startOverlay;
    public float fadeDuration = 2f;

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

       

        // Set white overlay at game start
        if (startOverlay != null)
        {
            startOverlay.gameObject.SetActive(true);
            startOverlay.color = Color.white;
        }

        StartCoroutine(StartupSequence());
    }

    private IEnumerator StartupSequence()
    {
        // Start fading the white overlay
        if (startOverlay != null)
        {
            StartCoroutine(FadeStartOverlay());
        }

        // Keep camera frozen
        yield return new WaitForSeconds(cameraFreezeDuration);

        // Enable camera movement
        cameraControlEnabled = true;
    }

    private IEnumerator FadeStartOverlay()
    {
        float elapsed = 0f;

        Color color = startOverlay.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);

            startOverlay.color = new Color(1f, 1f, 1f, alpha);

            yield return null;
        }

        startOverlay.color = new Color(1f, 1f, 1f, 0f);

        startOverlay.gameObject.SetActive(false);
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

        Vector3 forward = playerCamera.forward;
        Vector3 right = playerCamera.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 move = (forward * input.y + right * input.x).normalized;

        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        verticalVelocity = Mathf.Max(verticalVelocity, -30f);

        Vector3 finalMovement = move * moveSpeed;
        finalMovement.y = verticalVelocity;

        controller.Move(finalMovement * Time.deltaTime);
    }

    private void HandleCamera()
    {
        if (!cameraControlEnabled || isTurning || Mouse.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -verticalLimit, verticalLimit);

        yRotation += mouseX;
        yRotation = Mathf.Clamp(yRotation, -horizontalLimit, horizontalLimit);

        playerCamera.localRotation = Quaternion.Euler(
            xRotation,
            yRotation,
            0f
        );
    }

    private void HandleTurnAround()
    {
        if (!cameraControlEnabled)
            return;

        if (Keyboard.current != null &&
            (Keyboard.current.leftShiftKey.wasPressedThisFrame ||
             Keyboard.current.rightShiftKey.wasPressedThisFrame))
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

        yRotation = 0f;

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        isTurnedAround = !isTurnedAround;
        isTurning = false;
    }
}
