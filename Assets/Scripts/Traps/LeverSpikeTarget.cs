using System.Collections;
using UnityEngine;

public class LeverSpikeTarget : MonoBehaviour
{
    [Header("Damage")]
    [Tooltip("Damage dealt to each enemy touched.")]
    public float damage = 999f;

    [Tooltip("Kill enemies instantly regardless of health?")]
    public bool instantKill = false;

    [Header("Movement")]
    [Tooltip("How far (in world-Y) the spikes rise.")]
    public float riseHeight = 1.5f;

    [Tooltip("Speed at which spikes rise and fall (units/sec).")]
    public float moveSpeed = 4f;

    [Header("Sounds")]
    [Tooltip("Sound played when raised spikes hit an enemy.")]
    public AudioClip enemyHitSound;

    [Range(0f, 1f)]
    public float enemyHitVolume = 1f;

    [Tooltip("Spike hit sound only plays when the player is this close.")]
    public float hitSoundAudibleDistance = 18f;

    public bool playBackup2DSound = true;

    public AudioSource spikeAudioSource;

    private Vector3 downPos;
    private Vector3 upPos;
    private bool isUp = false;
    private bool moving = false;

    void Start()
    {
        downPos = transform.position;
        upPos = new Vector3(transform.position.x,
                            transform.position.y + riseHeight,
                            transform.position.z);

        ConfigureAudioSource();
        if (enemyHitSound == null)
            enemyHitSound = TempleAudio.LoadClip("TempleAudio/SFX/sword-slash");
    }

    public void Activate()
    {
        if (moving) return;
        if (!isUp)
            StartCoroutine(MoveToPos(upPos, up: true));
        else
            StartCoroutine(MoveToPos(downPos, up: false));
    }

    IEnumerator MoveToPos(Vector3 target, bool up)
    {
        moving = true;

        while (transform.position != target)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        isUp = up;
        moving = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isUp && !moving) return;

        EnemyHealth eh = other.GetComponent<EnemyHealth>()
                      ?? other.GetComponentInParent<EnemyHealth>()
                      ?? other.GetComponentInChildren<EnemyHealth>();

        if (eh != null && !eh.IsDead())
        {
            float dmg = instantKill ? eh.maxHealth * 2f : damage;
            eh.TakeDamage(dmg);
            PlayEnemyHitSound();
            Debug.Log("[LeverSpikeTarget] Hit " + other.name + " for " + dmg + " dmg.");
        }
    }

    void ConfigureAudioSource()
    {
        if (spikeAudioSource == null)
            spikeAudioSource = GetComponent<AudioSource>();
        if (spikeAudioSource == null)
            spikeAudioSource = gameObject.AddComponent<AudioSource>();

        spikeAudioSource.spatialBlend = 1f;
        spikeAudioSource.rolloffMode = AudioRolloffMode.Linear;
        spikeAudioSource.minDistance = 1f;
        spikeAudioSource.maxDistance = hitSoundAudibleDistance;
        spikeAudioSource.volume = 1f;
        spikeAudioSource.playOnAwake = false;
        spikeAudioSource.ignoreListenerPause = true;
    }

    void PlayEnemyHitSound()
    {
        if (enemyHitSound == null || spikeAudioSource == null) return;

        Transform listener = Camera.main != null ? Camera.main.transform : null;
        if (listener != null && Vector3.Distance(transform.position, listener.position) > hitSoundAudibleDistance)
            return;

        float pitch = Random.Range(0.95f, 1.05f);
        spikeAudioSource.pitch = pitch;
        spikeAudioSource.PlayOneShot(enemyHitSound, TempleAudio.ScaleSfxVolume(enemyHitVolume));

        if (playBackup2DSound)
            TempleAudio.PlaySfx(enemyHitSound, enemyHitVolume);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 up = Application.isPlaying ? upPos
                   : new Vector3(transform.position.x,
                                 transform.position.y + riseHeight,
                                 transform.position.z);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(up, Vector3.one * 0.4f);
        Gizmos.DrawLine(transform.position, up);
    }
}
