
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

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        waypointTriggered = new bool[respawnPoints.Length];
    }

    private void Update()
    {
        CheckRespawnPoints();
        FollowPlayer();
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

        // Calculate direction towards corridor centre
        float centering = rightDistance - leftDistance;

        Vector3 centeringDirection = right * centering;

        // Combine player direction and centering direction
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

        // Apply gravity
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
        if (hit.gameObject.CompareTag("Player"))
        {
            deathManager.Die();
        }
    }
    private void CheckRespawnPoints()
    {
        if (player == null || nextWaypointIndex >= respawnPoints.Length)
            return;

        RespawnPoint currentPoint = respawnPoints[nextWaypointIndex];

        if (currentPoint.playerWaypoint == null ||
            currentPoint.enemyWaypoint == null)
            return;

        float distance = Vector3.Distance(
            player.position,
            currentPoint.playerWaypoint.position
        );

        if (distance <= 2f)
        {
            // Move to the next waypoint immediately
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
