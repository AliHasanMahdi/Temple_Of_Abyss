using UnityEngine;

public class FallingRocks : MonoBehaviour
{
    public GameObject rockPrefab;
    public float spawnInterval = 1.5f;
    public float damage = 40f;
    public float spawnRadius = 2f;
<<<<<<< Updated upstream
=======

    [Header("Sound")]
    public AudioClip fallRockSound;
    [Range(0f, 1f)]
    public float fallRockVolume = 0.35f;
    public float fallRockAudibleDistance = 7f;
    public AudioSource audioSource;

    const float MaxAudibleDistance = 7f;
    const float VolumeScale = 0.45f;
>>>>>>> Stashed changes

    private float timer = 0f;

    void Start()
    {
        ConfigureAudioSource();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnRock();
            timer = 0f;
        }
    }

    void SpawnRock()
    {
        if (rockPrefab == null)
        {
            Debug.LogWarning("FallingRocks has no rock prefab assigned: " + name);
            return;
        }

        // Spawn rock at random position within radius
        Vector3 randomOffset = new Vector3(
            Random.Range(-spawnRadius, spawnRadius),
            0f,
            Random.Range(-spawnRadius, spawnRadius)
        );

        Vector3 spawnPosition = transform.position + randomOffset;
        GameObject rock = Instantiate(rockPrefab, spawnPosition, Random.rotation);
        PlayFallRockSound();

        // Destroy rock after 5 seconds so scene doesn't fill up
        Destroy(rock, 5f);
    }
<<<<<<< Updated upstream
}
=======

    void ConfigureAudioSource()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 0.5f;
        audioSource.maxDistance = EffectiveAudibleDistance();
        audioSource.playOnAwake = false;
    }

    void PlayFallRockSound()
    {
        if (fallRockSound == null) return;
        if (!PlayerIsCloseEnough()) return;

        if (audioSource == null)
            ConfigureAudioSource();

        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(fallRockSound, EffectiveVolume());
    }

    bool PlayerIsCloseEnough()
    {
        Transform listener = Camera.main != null ? Camera.main.transform : null;
        if (listener == null) return true;

        return Vector3.Distance(transform.position, listener.position) <= EffectiveAudibleDistance();
    }

    float EffectiveAudibleDistance()
    {
        return Mathf.Min(fallRockAudibleDistance, MaxAudibleDistance);
    }

    float EffectiveVolume()
    {
        return TempleAudio.ScaleSfxVolume(fallRockVolume * VolumeScale);
    }
}
>>>>>>> Stashed changes
