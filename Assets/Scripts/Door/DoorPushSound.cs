using UnityEngine;

/// <summary>
/// Attach this to any door that is a Rigidbody the player walks into.
/// It listens for collision impacts and plays an open/creak sound when
/// the door starts moving, and a close/thud sound when it stops.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class DoorPushSound : MonoBehaviour
{
    [Header("Door Sounds")]
    [Tooltip("Played the first time the door is hit and starts moving (e.g. Closing Door With Creak Latch Shut 3.wav)")]
    public AudioClip openCreakSound;

    [Tooltip("Played when the door bangs to a stop / hits a wall (e.g. Close Door 12.wav)")]
    public AudioClip bangsStopSound;

    [Range(0f, 1f)]
    public float volume = 0.85f;

    [Tooltip("Minimum collision impulse needed to trigger a sound ? stops tiny nudges firing it")]
    public float minImpulse = 0.8f;

    [Tooltip("Seconds the door must be still before it can trigger the 'bang stop' sound")]
    public float stopDelay = 0.4f;

    private AudioSource audioSource;
    private Rigidbody rb;

    private bool isMoving = false;
    private float stillTimer = 0f;
    private bool openSoundPlayed = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1f;          // full 3D sound
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 0.5f;
        audioSource.maxDistance = 10f;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        float speed = rb.linearVelocity.magnitude;

        if (speed > 0.05f)
        {
            // Door is moving
            isMoving = true;
            stillTimer = 0f;
        }
        else if (isMoving)
        {
            // Door just stopped ? wait a moment then play bang sound
            stillTimer += Time.deltaTime;

            if (stillTimer >= stopDelay)
            {
                isMoving = false;
                openSoundPlayed = false; // reset so next push works again
                PlaySound(bangsStopSound);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Only react to the player pushing the door
        if (!collision.gameObject.CompareTag("Player")) return;

        float impulse = collision.impulse.magnitude;
        if (impulse < minImpulse) return;

        // Play the open/creak sound only once per push sequence
        if (!openSoundPlayed)
        {
            openSoundPlayed = true;
            PlaySound(openCreakSound);
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.pitch = Random.Range(0.93f, 1.07f);
<<<<<<< Updated upstream
        audioSource.PlayOneShot(clip, volume);
    }
}
=======
        audioSource.PlayOneShot(clip, TempleAudio.ScaleSfxVolume(volume));
    }
}
>>>>>>> Stashed changes
