
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]


public class EnemyBehaviour : MonoBehaviour
{
    [System.Serializable]
    public class RespawnPoint
    {
        public Transform playerWaypoint;
        public Transform enemyWaypoint;
    }

    [Header("Player")]
    public Transform player;

    [Header("Enemy Movement")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 5f;
    public float chaseSpeed = 8f;
    public float speedIncreaseDistance = 5f;
    public float speedDecreaseDistance = 3f;

    [Header("Respawn Points")]
    public RespawnPoint[] respawnPoints;

    [Header("Respawn Effects")]
    public GameObject respawnParticlePrefab;

    private CharacterController controller;
    private bool[] waypointTriggered;

    public float gravity = -20f;

    private float verticalVelocity;

    private int nextWaypointIndex = 0;

    public DeathManager deathManager;

    
    public float wallCheckDistance = 2f;
    public float centeringStrength = 3f;
    public float wallAvoidanceDistance = 0.8f;
    public LayerMask wallLayer;


    public float respawnHeightOffset = 0.3f;

    private bool hasCaughtPlayer;
    public Transform playerCamera;
    
    public float cameraZoomDuration = 1.2f;

    public Transform enemyFaceTarget;

    public float cameraDistance = 1.2f;
    public float cameraHeightOffset = 0.2f;

    public float shakeIntensity = 0.04f;
    public float shakeSpeed = 25f;
    public float shakeDuration = 0.5f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        waypointTriggered = new bool[respawnPoints.Length];
    }

    private void Update()
    {

        if (hasCaughtPlayer)
            return;

        CheckRespawnPoints();
        UpdateEnemySpeed();
        FollowPlayer();
    }

    private void UpdateEnemySpeed()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance >= speedIncreaseDistance)
        {
            moveSpeed = chaseSpeed;
        }
        else if (distance <= speedDecreaseDistance)
        {
            moveSpeed = 2f;
        }
    }
    private void FollowPlayer()
    {
        if (player == null)
            return;

        // Direction towards player
        Vector3 playerDirection = player.position - transform.position;
        playerDirection.y = 0f;

        if (playerDirection.sqrMagnitude > 0.01f)
        {
            playerDirection.Normalize();
        }

        // Check walls on both sides
        Vector3 right = transform.right;

        float leftDistance = wallCheckDistance;
        float rightDistance = wallCheckDistance;

        if (Physics.Raycast(
            transform.position,
            -right,
            out RaycastHit leftHit,
            wallCheckDistance,
            wallLayer))
        {
            leftDistance = leftHit.distance;
        }

        if (Physics.Raycast(
            transform.position,
            right,
            out RaycastHit rightHit,
            wallCheckDistance,
            wallLayer))
        {
            rightDistance = rightHit.distance;
        }

        
        float centering = rightDistance - leftDistance;

        Vector3 centeringDirection = right * centering;

        
        Vector3 finalDirection =
            playerDirection + centeringDirection * centeringStrength;

        finalDirection.y = 0f;

        if (finalDirection.sqrMagnitude > 0.01f)
        {
            finalDirection.Normalize();

            Quaternion targetRotation = Quaternion.LookRotation(finalDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

       
        if (controller.isGrounded)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 movement = transform.forward * moveSpeed;
        movement.y = verticalVelocity;

        controller.Move(movement * Time.deltaTime);
    }


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Player") && !hasCaughtPlayer)
        {
            hasCaughtPlayer = true;
            StartCoroutine(CatchPlayerSequence());
        }
    }

    private IEnumerator CatchPlayerSequence()
    {
        // Disable player movement
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerCamera == null || enemyFaceTarget == null)
        {
            deathManager.Die();
            yield break;
        }

        Vector3 startPosition = playerCamera.position;
        Quaternion startRotation = playerCamera.rotation;

        // Destination is the waypoint in front of the enemy's face
        Vector3 destination = enemyFaceTarget.position;

        Quaternion targetRotation = Quaternion.LookRotation(
            transform.position + Vector3.up * cameraHeightOffset - destination
        ) * Quaternion.Euler(-60f, 0f, 0f);

        float elapsed = 0f;
        deathManager.Die();

        while (elapsed < cameraZoomDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / cameraZoomDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            playerCamera.position = Vector3.Lerp(
                startPosition,
                destination,
                smoothT
            );

            playerCamera.rotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                smoothT
            );

            yield return null;
        }

        playerCamera.position = destination;
        playerCamera.rotation = targetRotation;

        // Shake camera after facing the enemy
        float shakeElapsed = 0f;

        while (shakeElapsed < shakeDuration)
        {
            shakeElapsed += Time.deltaTime;

            float shakeX = Random.Range(-1f, 1f) * shakeIntensity;
            float shakeY = Random.Range(-1f, 1f) * shakeIntensity;

            playerCamera.position = destination +
                playerCamera.right * shakeX +
                playerCamera.up * shakeY;

            yield return null;
        }

        // Restore exact camera position
        playerCamera.position = destination;

       
    }
    private void CheckRespawnPoints()
    {
        if (player == null || nextWaypointIndex >= respawnPoints.Length)
            return;

        RespawnPoint currentPoint = respawnPoints[nextWaypointIndex];

        if (currentPoint.playerWaypoint == null ||
            currentPoint.enemyWaypoint == null)
            return;

        float detectionRadius = 4f;

        Vector3 playerPosition = player.position;
        Vector3 waypointPosition = currentPoint.playerWaypoint.position;

        // Ignore height differences
        playerPosition.y = 0f;
        waypointPosition.y = 0f;

        float distance = Vector3.Distance(
            playerPosition,
            waypointPosition
        );

        if (distance <= detectionRadius)
        {
            nextWaypointIndex++;

            // Start respawn sequence
            RespawnEnemy(currentPoint.enemyWaypoint);
        }
    }

    private void RespawnEnemy(Transform spawnPoint)
    {
        StartCoroutine(RespawnSequence(spawnPoint));
    }

    private IEnumerator RespawnSequence(Transform spawnPoint)
    {
        // Spawn particles at the upcoming enemy position
        if (respawnParticlePrefab != null)
        {
            Instantiate(
                respawnParticlePrefab,
                spawnPoint.position,
                Quaternion.identity
            );
        }

        // Wait 1 second
        yield return new WaitForSeconds(1f);

        // Move enemy to spawn position
        controller.enabled = false;

        transform.position = spawnPoint.position + Vector3.up * respawnHeightOffset;
        transform.rotation = spawnPoint.rotation;

        controller.enabled = true;
    }
}
