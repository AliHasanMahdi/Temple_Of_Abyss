using UnityEngine;

public class SpikeDamage : MonoBehaviour
{
    public float damage = 50f;
<<<<<<< Updated upstream
=======
    public AudioClip enemyHitSound;
    [Range(0f, 1f)]
    public float enemyHitVolume = 1f;
    public float hitSoundAudibleDistance = 18f;
    public bool playBackup2DSound = true;
    public AudioSource spikeAudioSource;

    void Start()
    {
        ConfigureAudioSource();
        if (enemyHitSound == null)
            enemyHitSound = TempleAudio.LoadClip("TempleAudio/SFX/sword-slash");
    }
>>>>>>> Stashed changes

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health =
                other.GetComponent<PlayerHealth>() ??
                other.GetComponentInParent<PlayerHealth>() ??
                other.GetComponentInChildren<PlayerHealth>();

            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
        EnemyHealth enemy =
           other.GetComponent<EnemyHealth>() ??
           other.GetComponentInParent<EnemyHealth>() ??
           other.GetComponentInChildren<EnemyHealth>();

        if (enemy != null && !enemy.IsDead())
<<<<<<< Updated upstream
            enemy.TakeDamage(enemy.maxHealth * 2f);

    }
=======
        {
            enemy.TakeDamage(enemy.maxHealth * 2f);
            PlayEnemyHitSound();
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
>>>>>>> Stashed changes
}
