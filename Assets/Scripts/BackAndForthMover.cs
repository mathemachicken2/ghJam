
using UnityEngine;

public class BackAndForthMover : MonoBehaviour
{
    [Header("Movement")]
    public Transform waypoint;
    public float moveSpeed = 4f;

    [Header("Rotation")]
    public float rotationSpeed = 180f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool movingToWaypoint = true;

    public DeathManager deathManager;

    private void Start()
    {
        startPosition = transform.position;
        targetPosition = waypoint.position;
    }

    private void Update()
    {
        
        Vector3 target = movingToWaypoint ? targetPosition : startPosition;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );

        if (transform.position == target)
        {
            movingToWaypoint = !movingToWaypoint;
        }

        
        transform.rotation *= Quaternion.Euler(
            0f,
            rotationSpeed * Time.deltaTime,
            0f
        );
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            deathManager.Die();
            Debug.Log("Player Died");
        }
    }
}
