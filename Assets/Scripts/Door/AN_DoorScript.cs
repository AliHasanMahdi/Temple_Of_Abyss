using UnityEngine;
using UnityEngine.InputSystem;

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

    // ── DOOR SOUNDS ───────────────────────────────────────────────
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
    // ─────────────────────────────────────────────────────────────

    private AN_HeroInteractive HeroInteractive;
    private Rigidbody rbDoor;
    private HingeJoint hinge;
    private JointLimits hingeLim;
    private float currentLim;
    private Camera mainCam;

    void Start()
    {
        rbDoor = GetComponent<Rigidbody>();
        hinge = GetComponent<HingeJoint>();
        mainCam = Camera.main;
        HeroInteractive = Object.FindAnyObjectByType<AN_HeroInteractive>();

        // Auto-create 3D AudioSource if not assigned
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

        // Restore unlocked state — checks both disk (past checkpoint) and session memory
        if (SaveSystem.Instance != null && SaveSystem.Instance.IsDoorUnlocked(doorID))
        {
            Locked = false;
            CanOpen = true;
            RedLocked = false;
            BlueLocked = false;
            isOpened = true;
            currentLim = 85f;
        }
    }

    void Update()
    {
        if (!Remote && UseLegacyInteraction() && Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame && NearView())
        {
            Action();
        }
    }

    public bool CanInteract(GameObject interactor)
    {
        return enabled &&
               gameObject.activeInHierarchy &&
               !Remote &&
               !HasAttachedCollectibleKey(interactor) &&
               NearViewFrom(interactor);
    }

    public string GetPromptText()
    {
        if (Locked || !CanOpen)
            return "Press E to use door";

        if (RedLocked || BlueLocked)
            return "Press E to unlock door";

        if (isOpened && CanClose)
            return "Press E to close door";

        return "Press E to open door";
    }

    public void Interact(GameObject interactor)
    {
        Action();
    }

    public void Action()
    {
        // Hard locked — play locked sound and stop
        if (Locked || !CanOpen)
        {
            PlayDoorSoundAudible(lockedSound);
            return;
        }

        // Try to unlock with keys
        if (HeroInteractive != null)
        {
            if (RedLocked && HasKeyAvailable(true))
            {
                RedLocked = false;
                ConsumeKey(true);
                PlayDoorSoundAudible(unlockSound);

                // Store in memory only — written to disk when player hits a checkpoint
                if (SaveSystem.Instance != null)
                    SaveSystem.Instance.PendingDoorUnlocked(doorID);
            }
            else if (BlueLocked && HasKeyAvailable(false))
            {
                BlueLocked = false;
                ConsumeKey(false);
                PlayDoorSoundAudible(unlockSound);

                // Store in memory only — written to disk when player hits a checkpoint
                if (SaveSystem.Instance != null)
                    SaveSystem.Instance.PendingDoorUnlocked(doorID);
            }
        }

        // Still locked by key — play locked sound
        if (RedLocked || BlueLocked)
        {
            PlayDoorSoundAudible(lockedSound);

            if (HUDManager.Instance != null)
                HUDManager.Instance.ShowTimedMessage("You need a key!", 2f);
            return;
        }

        // Open or close
        if (isOpened && CanClose)
        {
            isOpened = false;
            PlayDoorSound(closeSound);
        }
        else if (!isOpened)
        {
            isOpened = true;
            PlayDoorSound(openSound);

            if (rbDoor != null)
                rbDoor.AddRelativeTorque(new Vector3(0, 0, 20f));
        }
    }

    // ── DOOR SOUND HELPERS ────────────────────────────────────────
    AudioClip PlayDoorSound(AudioClip clip)
    {
        if (clip == null)
        {
            LoadDefaultSounds();
            clip = openSound;
        }
        if (doorAudioSource == null || clip == null) return clip;
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
    // ─────────────────────────────────────────────────────────────

    bool HasKeyAvailable(bool isRedKey)
    {
        if (InventoryManager.Instance != null)
        {
            return InventoryManager.Instance.HasKey(isRedKey);
        }

        return isRedKey ? HeroInteractive.RedKey : HeroInteractive.BlueKey;
    }

    void ConsumeKey(bool isRedKey)
    {
        if (InventoryManager.Instance != null &&
            InventoryManager.Instance.TryRemoveKey(isRedKey))
        {
            return;
        }

        if (isRedKey)
        {
            HeroInteractive.RedKey = false;
        }
        else
        {
            HeroInteractive.BlueKey = false;
        }
    }

    bool NearView()
    {
        if (mainCam == null) return false;
        return Vector3.Distance(transform.position, mainCam.transform.position) < 3f;
    }

    bool NearViewFrom(GameObject interactor)
    {
        if (interactor == null)
            return false;

        Transform origin = interactor.transform;
        PlayerMovement movement = interactor.GetComponent<PlayerMovement>()
            ?? interactor.GetComponentInChildren<PlayerMovement>();
        if (movement != null && movement.ViewTransform != null)
            origin = movement.ViewTransform;

        return Vector3.Distance(transform.position, origin.position) < 3f;
    }

    bool UseLegacyInteraction()
    {
        return Object.FindFirstObjectByType<PlayerInteraction>() == null;
    }

    bool HasAttachedCollectibleKey(GameObject interactor)
    {
        Transform parent = transform.parent;
        if (parent == null)
            return false;

        foreach (Transform child in parent)
        {
            if (child == null || child == transform || child.name != "Door_key")
                continue;

            AN_DoorKey doorKey = child.GetComponent<AN_DoorKey>();
            if (doorKey != null &&
                doorKey.enabled &&
                child.gameObject.activeInHierarchy &&
                doorKey.CanInteract(interactor))
                return true;

            Level05QuestItem questItem = child.GetComponent<Level05QuestItem>();
            if (questItem != null &&
                questItem.enabled &&
                child.gameObject.activeInHierarchy &&
                questItem.CanInteract(interactor))
                return true;
        }

        return false;
    }

    void FixedUpdate()
    {
        if (hinge == null) return;

        currentLim = isOpened
            ? 85f
            : Mathf.Max(0f, currentLim - 0.5f * OpenSpeed);

        hingeLim.max = currentLim;
        hingeLim.min = -currentLim;
        hinge.limits = hingeLim;
        hinge.useLimits = true;
    }
}
