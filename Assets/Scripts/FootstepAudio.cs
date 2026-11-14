
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class FootstepAudio : MonoBehaviour
{
    public AudioClip walkingSound;

    [Header("Settings")]
    public float movementThreshold = 0.1f;
    [Range(0f, 1f)]
    public float volume = 0.7f;

    private CharacterController controller;
    private AudioSource audioSource;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        audioSource.clip = walkingSound;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
        audioSource.spatialBlend = 1f;
    }

    private void Update()
    {
        if (walkingSound == null)
            return;

        Vector3 horizontalVelocity = controller.velocity;
        horizontalVelocity.y = 0f;

        bool isMoving = horizontalVelocity.magnitude > movementThreshold;

        if (isMoving && controller.isGrounded)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }
    }
}
