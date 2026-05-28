using UnityEngine;

public class SpearTrap : MonoBehaviour
{
    [Header("Spear Settings")]
    public GameObject spearPrefab;
    // Lower interval = more frequent shooting
    public float shootInterval = 1.5f;
    // Lower speed = slower moving spear
    public float spearSpeed = 8f;
    public float spearDamage = 40f;
    public float spearLifetime = 4f;

    [Header("Direction")]
    // Change this to (1, 0, 0) or (-1, 0, 0) for side-shooting
    public Vector3 shootDirection = new Vector3(1, 0, 0);

    [Header("Sound")]
    public AudioClip arrowSound;
    [Range(0f, 1f)]
    public float arrowVolume = 0.35f;
    public float arrowAudibleDistance = 7f;
    public AudioSource audioSource;

    const float MaxAudibleDistance = 7f;
    const float VolumeScale = 0.45f;

    private float timer = 0f;

    void Start()
    {
        ConfigureAudioSource();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= shootInterval)
        {
            timer = 0f;
            ShootSpear();
        }
    }

    void ShootSpear()
    {
        if (spearPrefab == null) return;
        PlayArrowSound();

        // Spawn spear at launcher position
        GameObject spear = Instantiate(
            spearPrefab,
            transform.position,
            transform.rotation
        );

        // Add rigidbody for physics movement
        Rigidbody rb = spear.GetComponent<Rigidbody>();
        if (rb == null)
            rb = spear.AddComponent<Rigidbody>();

        rb.useGravity = false;
        rb.linearVelocity = transform.TransformDirection(shootDirection.normalized) * spearSpeed;

        // Add damage script to spear
        SpearDamage dmg = spear.GetComponent<SpearDamage>();
        if (dmg == null)
            dmg = spear.AddComponent<SpearDamage>();

        dmg.damage = spearDamage;

        // Destroy after lifetime
        Destroy(spear, spearLifetime);
    }

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

    void PlayArrowSound()
    {
        if (arrowSound == null) return;
        if (!PlayerIsCloseEnough()) return;

        if (audioSource == null)
            ConfigureAudioSource();

        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(arrowSound, EffectiveVolume());
    }

    bool PlayerIsCloseEnough()
    {
        Transform listener = Camera.main != null ? Camera.main.transform : null;
        if (listener == null) return true;

        return Vector3.Distance(transform.position, listener.position) <= EffectiveAudibleDistance();
    }

    float EffectiveAudibleDistance()
    {
        return Mathf.Min(arrowAudibleDistance, MaxAudibleDistance);
    }

    float EffectiveVolume()
    {
        return TempleAudio.ScaleSfxVolume(arrowVolume * VolumeScale);
    }

    // Draw shoot direction in scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position,
            transform.TransformDirection(shootDirection.normalized) * 5f);
    }
}
