using UnityEngine;

public class AN_DoorScript : MonoBehaviour, IPlayerInteractable
{
    [Header("Door Settings")]
    public bool Locked = false;
    public bool Remote = false;
    public bool CanOpen = true;
    public bool CanClose = true;

    [Header("Key Settings")]
    public bool RedLocked = false;
    public bool BlueLocked = false;

    [Header("Door ID — unique per door")]
    public string doorID = "Door_01";

    [Header("Animation")]
    public bool isOpened = false;
    [Range(0f, 4f)]
    public float OpenSpeed = 3f;

    [Header("Door Sounds")]
    [Tooltip("AudioSource on this door GameObject (3D)")]
    public AudioSource doorAudioSource;

    [Tooltip("Sound played when the door opens (e.g. Closing Door With Creak Latch Shut 3.wav used reversed, or Open Door X.wav)")]
    public AudioClip openSound;

    [Tooltip("Sound played when the door closes (e.g. Close Door 12.wav)")]
    public AudioClip closeSound;

    [Tooltip("Sound played when player tries to open a locked door (e.g. Close Metal Door Locker Cabinet Box 1.wav)")]
    public AudioClip lockedSound;

    [Tooltip("Sound played when a key unlocks the door")]
    public AudioClip unlockSound;

    [Range(0f, 1f)]
    public float doorVolume = 0.8f;

    [Header("Interaction")]
    public float interactionDistance = 3f;

    public bool RequiresKey => RedLocked || BlueLocked;

    private AN_HeroInteractive HeroInteractive;
    private Rigidbody rbDoor;
    private HingeJoint hinge;
    private JointLimits hingeLim;
    private float currentLim;

    void Start()
    {
        rbDoor = GetComponent<Rigidbody>();
        hinge = GetComponent<HingeJoint>();
        HeroInteractive = Object.FindAnyObjectByType<AN_HeroInteractive>();

        if (doorAudioSource == null)
        {
            doorAudioSource = gameObject.AddComponent<AudioSource>();
            doorAudioSource.spatialBlend = 1f;
            doorAudioSource.rolloffMode = AudioRolloffMode.Linear;
            doorAudioSource.minDistance = 1f;
            doorAudioSource.maxDistance = 25f;
            doorAudioSource.playOnAwake = false;
        }
        else
        {
            doorAudioSource.maxDistance = Mathf.Max(doorAudioSource.maxDistance, 25f);
        }

        LoadDefaultSounds();
        NormalizeLockState();

        if (SaveSystem.Instance != null && SaveSystem.Instance.IsDoorUnlocked(doorID))
        {
            Locked = false;
            CanOpen = true;
            RedLocked = false;
            BlueLocked = false;
            isOpened = true;
            currentLim = 85f;
        }

        NormalizeLockState();
    }

    public bool CanInteract(GameObject interactor)
    {
        return !Remote && enabled && gameObject.activeInHierarchy && IsWithinRange(interactor);
    }

    public string GetPromptText()
    {
        if (Locked && !RequiresKey)
            return "Door is locked";

        if (RequiresKey)
            return "Press E to unlock door";

        return "Press E to " + (isOpened ? "close door" : "open door");
    }

    public void Interact(GameObject interactor)
    {
        if (interactor != null)
            HeroInteractive = interactor.GetComponent<AN_HeroInteractive>() ?? interactor.GetComponentInChildren<AN_HeroInteractive>();

        Action();
    }

    public void Action()
    {
        NormalizeLockState();
        bool needsKey = RequiresKey;

        if (Locked && !needsKey)
        {
            PlayDoorSoundAudible(lockedSound);
            return;
        }

        if (HeroInteractive != null)
        {
            if (RedLocked && HeroInteractive.UseKey(true))
            {
                RedLocked = false;
                Locked = false;
                PlayDoorSoundAudible(unlockSound);
                InventoryManager.Instance?.SyncKeysFromPlayer();

                if (SaveSystem.Instance != null)
                    SaveSystem.Instance.PendingDoorUnlocked(doorID);
            }
            else if (BlueLocked && HeroInteractive.UseKey(false))
            {
                BlueLocked = false;
                Locked = false;
                PlayDoorSoundAudible(unlockSound);
                InventoryManager.Instance?.SyncKeysFromPlayer();

                if (SaveSystem.Instance != null)
                    SaveSystem.Instance.PendingDoorUnlocked(doorID);
            }
        }

        if (RequiresKey)
        {
            PlayDoorSoundAudible(lockedSound);

            if (HUDManager.Instance != null)
                HUDManager.Instance.ShowTimedMessage("You need a key!", 2f);
            return;
        }

        NormalizeLockState();

        if (isOpened && CanClose)
        {
            isOpened = false;
            PlayDoorSound(closeSound);
            return;
        }

        if (!CanOpen)
        {
            PlayDoorSoundAudible(lockedSound);
            return;
        }

        if (!isOpened)
        {
            isOpened = true;
            PlayDoorSound(openSound);

            if (rbDoor != null)
                rbDoor.AddRelativeTorque(new Vector3(0, 0, 20f));
        }
    }

    void NormalizeLockState()
    {
        Locked = RequiresKey;
    }

    AudioClip PlayDoorSound(AudioClip clip)
    {
        if (clip == null)
        {
            LoadDefaultSounds();
            clip = openSound;
        }

        if (doorAudioSource == null || clip == null)
            return clip;

        doorAudioSource.pitch = Random.Range(0.95f, 1.05f);
        doorAudioSource.PlayOneShot(clip, doorVolume);
        return clip;
    }

    void PlayDoorSoundAudible(AudioClip clip)
    {
        clip = PlayDoorSound(clip);
        TempleAudio.PlaySfx(clip, doorVolume);
    }

    public void PlayUnlockSound()
    {
        LoadDefaultSounds();
        PlayDoorSoundAudible(unlockSound);
    }

    void LoadDefaultSounds()
    {
        if (openSound == null)
            openSound = TempleAudio.LoadClip("TempleAudio/SFX/Open Door 13");
        if (closeSound == null)
            closeSound = TempleAudio.LoadClip("TempleAudio/SFX/Close Door 12");
        if (lockedSound == null)
            lockedSound = TempleAudio.LoadClip("TempleAudio/SFX/Locked Door 2");
        if (unlockSound == null)
            unlockSound = TempleAudio.LoadClip("TempleAudio/SFX/Unlock 1");
    }

    bool IsWithinRange(GameObject interactor)
    {
        if (interactor == null)
            return false;

        Transform origin = interactor.transform;
        PlayerMovement movement = interactor.GetComponent<PlayerMovement>();
        if (movement != null && movement.ViewTransform != null)
            origin = movement.ViewTransform;

        return Vector3.Distance(transform.position, origin.position) < interactionDistance;
    }

    void FixedUpdate()
    {
        if (hinge == null)
            return;

        currentLim = isOpened
            ? 85f
            : Mathf.Max(0f, currentLim - 0.5f * OpenSpeed);

        hingeLim.max = currentLim;
        hingeLim.min = -currentLim;
        hinge.limits = hingeLim;
        hinge.useLimits = true;
    }
}
