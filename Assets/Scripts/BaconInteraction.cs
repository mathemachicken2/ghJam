
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BaconInteraction : MonoBehaviour
{
    public Image whiteFadeImage;
    public TextMeshProUGUI endingText;
    public Image secondEndingImage;
    public float secondImageDelay = 2f;

    public float endingFadeDuration = 2f;
    public float textFadeDuration = 2f;
    public float mainMenuDelay = 5f;

    public string mainMenuSceneName = "MainMenu";

    
    public Transform player;
    public float interactionDistance = 3f;
    public TextMeshProUGUI interactionPrompt;

  
    public Transform playerCamera;
    public Transform cameraWaypoint;
    public float cameraMoveDuration = 1f;

    
    public GameObject bacon2Prefab;
    public GameObject bacon3Prefab;
    public GameObject originalBacon;

    private bool hasStarted = false;
    private bool playerInRange = false;

    
    public float moveForwardDistance = 0.3f;
    public float moveDuration = 0.3f;

    public bool HasWon { get; private set; }

    private void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.gameObject.SetActive(false);

        if (whiteFadeImage != null)
        {
            whiteFadeImage.gameObject.SetActive(false);

            Color color = whiteFadeImage.color;
            color.a = 0f;
            whiteFadeImage.color = color;
        }

        if (endingText != null)
        {
            endingText.gameObject.SetActive(false);

            Color color = endingText.color;
            color.a = 0f;
            endingText.color = color;
        }

        if (secondEndingImage != null)
        {
            secondEndingImage.gameObject.SetActive(false);

            Color color = secondEndingImage.color;
            color.a = 0f;
            secondEndingImage.color = color;
        }
    }

    private void Update()
    {
        if (hasStarted || player == null)
            return;

        float distance = Vector3.Distance(
            player.position,
            transform.position
        );

        playerInRange = distance <= interactionDistance;

        if (interactionPrompt != null)
            interactionPrompt.gameObject.SetActive(playerInRange);

        if (playerInRange && Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            hasStarted = true;
            HasWon = true;

            if (interactionPrompt != null)
                interactionPrompt.gameObject.SetActive(false);

            StartCoroutine(EatBaconSequence());
        }
    }


    private IEnumerator MoveBacon(GameObject bacon)
    {
        if (bacon == null)
            yield break;

        
        ConstantRotation rotation = bacon.GetComponent<ConstantRotation>();

        if (rotation != null)
            rotation.enabled = false;

        Vector3 startPosition = bacon.transform.position;

        
        Vector3 direction = (playerCamera.position - startPosition).normalized;

        Vector3 forwardPosition = startPosition +
            direction * moveForwardDistance;

        float elapsed = 0f;

     
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / moveDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            bacon.transform.position = Vector3.Lerp(
                startPosition,
                forwardPosition,
                smoothT
            );

            yield return null;
        }

        elapsed = 0f;

       
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / moveDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            bacon.transform.position = Vector3.Lerp(
                forwardPosition,
                startPosition,
                smoothT
            );

            yield return null;
        }

        bacon.transform.position = startPosition;
    }


    private IEnumerator EatBaconSequence()
    {
        Vector3 baconPosition = originalBacon.transform.position;
        Quaternion baconRotation = originalBacon.transform.rotation;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayEating();

        // Stop original bacon rotation
        ConstantRotation rotation = originalBacon.GetComponent<ConstantRotation>();

        if (rotation != null)
            rotation.enabled = false;

        // Disable player movement
        PlayerMovement movement = player.GetComponent<PlayerMovement>();

        if (movement != null)
            movement.enabled = false;

        // Move camera to waypoint
        if (playerCamera != null && cameraWaypoint != null)
        {
            Vector3 startPosition = playerCamera.position;
            Quaternion startRotation = playerCamera.rotation;

            Quaternion targetRotation = cameraWaypoint.rotation *
                Quaternion.Euler(10f, 180f, 0f);

            float elapsed = 0f;

            while (elapsed < cameraMoveDuration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / cameraMoveDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                playerCamera.position = Vector3.Lerp(
                    startPosition,
                    cameraWaypoint.position,
                    smoothT
                );

                playerCamera.rotation = Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    smoothT
                );

                yield return null;
            }

            playerCamera.position = cameraWaypoint.position;
            playerCamera.rotation = targetRotation;
        }

       
        yield return StartCoroutine(MoveBacon(originalBacon));

        Destroy(originalBacon);

       
        GameObject bacon2 = Instantiate(
            bacon2Prefab,
            baconPosition,
            baconRotation
        );

        yield return StartCoroutine(MoveBacon(bacon2));

        Destroy(bacon2);

       
        GameObject bacon3 = Instantiate(
            bacon3Prefab,
            baconPosition,
            baconRotation
        );

        yield return StartCoroutine(MoveBacon(bacon3));

        Destroy(bacon3);

        yield return new WaitForSeconds(1f);

        // Start ending sequence
        yield return StartCoroutine(EndingSequence());

    }


    private IEnumerator EndingSequence()
    {
        
        if (whiteFadeImage != null)
            whiteFadeImage.gameObject.SetActive(true);

        
        if (endingText != null)
            endingText.gameObject.SetActive(true);

        float elapsed = 0f;

        
        while (elapsed < Mathf.Max(endingFadeDuration, textFadeDuration))
        {
            elapsed += Time.deltaTime;

            if (whiteFadeImage != null)
            {
                Color imageColor = whiteFadeImage.color;
                imageColor.a = Mathf.Clamp01(elapsed / endingFadeDuration);
                whiteFadeImage.color = imageColor;
            }

            if (endingText != null)
            {
                Color textColor = endingText.color;
                textColor.a = Mathf.Clamp01(elapsed / textFadeDuration);
                endingText.color = textColor;
            }

            yield return null;
        }

        yield return new WaitForSeconds(secondImageDelay);

        // Activate second image
        if (secondEndingImage != null)
        {
            secondEndingImage.gameObject.SetActive(true);

            Color color = secondEndingImage.color;
            color.a = 1f;
            secondEndingImage.color = color;
        }
        float remainingTime = mainMenuDelay - elapsed;

        if (remainingTime > 0f)
            yield return new WaitForSeconds(remainingTime);

        
        SceneManager.LoadScene(mainMenuSceneName);
    }


}
