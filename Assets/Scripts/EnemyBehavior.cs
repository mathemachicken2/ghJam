
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

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

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

        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        controller.enabled = true;
    }
}
