
using UnityEngine;
using System.Collections;

public class WaypointMover : MonoBehaviour
{
    [Header("Movement")]
    public Transform waypoint;
    public float moveSpeed = 10f;
    public float waitTime = 3f;

    private Vector3 startPosition;

    public DeathManager deathManager;

    private void Start()
    {
        startPosition = transform.position;
        StartCoroutine(MoveLoop());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            deathManager.Die();
            Debug.Log("Player Dead");
        }
    }
    private IEnumerator MoveLoop()
    {
        while (true)
        {
            
            yield return MoveToPosition(waypoint.position);

           
            yield return MoveToPosition(startPosition);

            
            yield return new WaitForSeconds(waitTime);
        }
    }

    private IEnumerator MoveToPosition(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = target;
    }
}
