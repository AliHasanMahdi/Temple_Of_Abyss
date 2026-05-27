using UnityEngine;
using UnityEngine.InputSystem;

public class AN_DoorScript : MonoBehaviour
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

<<<<<<< Updated upstream
    [Tooltip("Sound played when the door opens (e.g. Closing Door With Creak Latch Shut 3.wav used reversed, or Open Door X.wav)")]
    public AudioClip openSound;

    [Tooltip("Sound played when the door closes (e.g. Close Door 12.wav)")]
=======
    [Tooltip("Optional. Door open sound is disabled by this script unless playOpenCloseSounds is enabled.")]
    public AudioClip openSound;

    [Tooltip("Optional. Door close sound is disabled by this script unless playOpenCloseSounds is enabled.")]
>>>>>>> Stashed changes
    public AudioClip closeSound;

    [Tooltip("Sound played when player tries to open a locked door (e.g. Close Metal Door Locker Cabinet Box 1.wav)")]
    public AudioClip lockedSound;

    [Tooltip("Sound played when a key unlocks the door")]
    public AudioClip unlockSound;

    [Range(0f, 1f)]
    public float doorVolume = 0.8f;
<<<<<<< Updated upstream
=======

    [Tooltip("Also play door sounds through the main SFX mixer so level doors stay audible when triggered.")]
    public bool playThroughSfxMixer = true;

    [Tooltip("Keep off when doors should only make locked/unlock sounds.")]
    public bool playOpenCloseSounds = false;
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
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
=======
        // Auto-create/configure a 3D AudioSource so scene-assigned sources cannot be silent by accident.
        if (doorAudioSource == null)
            doorAudioSource = gameObject.AddComponent<AudioSource>();

        doorAudioSource.spatialBlend = 1f;
        doorAudioSource.rolloffMode = AudioRolloffMode.Linear;
        doorAudioSource.minDistance = Mathf.Max(doorAudioSource.minDistance, 1f);
        doorAudioSource.maxDistance = Mathf.Max(doorAudioSource.maxDistance, 25f);
        doorAudioSource.playOnAwake = false;
>>>>>>> Stashed changes

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
        if (!Remote && Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame && NearView())
        {
            Action();
        }
    }

    public void Action()
    {
<<<<<<< Updated upstream
=======
        bool playedUnlockSound = false;

>>>>>>> Stashed changes
        // Hard locked — play locked sound and stop
        if (Locked || !CanOpen)
        {
            PlayDoorSoundAudible(lockedSound);
            return;
        }

        // Try to unlock with keys
        if (HeroInteractive != null)
        {
            if (RedLocked && HeroInteractive.RedKey)
            {
                RedLocked = false;
                HeroInteractive.RedKey = false;
                PlayDoorSoundAudible(unlockSound);
<<<<<<< Updated upstream
=======
                playedUnlockSound = true;
>>>>>>> Stashed changes

                // Store in memory only — written to disk when player hits a checkpoint
                if (SaveSystem.Instance != null)
                    SaveSystem.Instance.PendingDoorUnlocked(doorID);
            }
            else if (BlueLocked && HeroInteractive.BlueKey)
            {
                BlueLocked = false;
                HeroInteractive.BlueKey = false;
                PlayDoorSoundAudible(unlockSound);
<<<<<<< Updated upstream
=======
                playedUnlockSound = true;
>>>>>>> Stashed changes

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
<<<<<<< Updated upstream
            PlayDoorSound(closeSound);
=======
            if (playOpenCloseSounds)
                PlayDoorSoundAudible(closeSound);
>>>>>>> Stashed changes
        }
        else if (!isOpened)
        {
            isOpened = true;
<<<<<<< Updated upstream
            PlayDoorSound(openSound);
=======
            if (playOpenCloseSounds)
                PlayDoorSoundAudible(openSound);
            else if (!playedUnlockSound)
                PlayDoorSoundAudible(unlockSound);
>>>>>>> Stashed changes

            if (rbDoor != null)
                rbDoor.AddRelativeTorque(new Vector3(0, 0, 20f));
        }
    }

    // ── DOOR SOUND HELPERS ────────────────────────────────────────
    AudioClip PlayDoorSound(AudioClip clip)
    {
<<<<<<< Updated upstream
        if (clip == null)
        {
            LoadDefaultSounds();
            clip = openSound;
        }
        if (doorAudioSource == null || clip == null) return clip;
        doorAudioSource.pitch = Random.Range(0.95f, 1.05f);
        doorAudioSource.PlayOneShot(clip, doorVolume);
=======
        if (clip == null) return null;
        if (doorAudioSource == null || clip == null) return clip;
        doorAudioSource.pitch = Random.Range(0.95f, 1.05f);
        doorAudioSource.PlayOneShot(clip, TempleAudio.ScaleSfxVolume(doorVolume));
>>>>>>> Stashed changes
        return clip;
    }

    void PlayDoorSoundAudible(AudioClip clip)
    {
        clip = PlayDoorSound(clip);
<<<<<<< Updated upstream
        TempleAudio.PlaySfx(clip, doorVolume);
=======
        if (playThroughSfxMixer)
            TempleAudio.PlaySfx(clip, doorVolume);
>>>>>>> Stashed changes
    }

    public void PlayUnlockSound()
    {
        LoadDefaultSounds();
        PlayDoorSoundAudible(unlockSound);
    }

    void LoadDefaultSounds()
    {
<<<<<<< Updated upstream
        if (openSound == null)
            openSound = TempleAudio.LoadClip("TempleAudio/SFX/Open Door 13");
        if (closeSound == null)
=======
        if (playOpenCloseSounds && openSound == null)
            openSound = TempleAudio.LoadClip("TempleAudio/SFX/Open Door 13");
        if (playOpenCloseSounds && closeSound == null)
>>>>>>> Stashed changes
            closeSound = TempleAudio.LoadClip("TempleAudio/SFX/Close Door 12");
        if (lockedSound == null)
            lockedSound = TempleAudio.LoadClip("TempleAudio/SFX/Locked Door 2");
        if (unlockSound == null)
            unlockSound = TempleAudio.LoadClip("TempleAudio/SFX/Unlock 1");
    }
    // ─────────────────────────────────────────────────────────────

    bool NearView()
    {
        if (mainCam == null) return false;
        return Vector3.Distance(transform.position, mainCam.transform.position) < 3f;
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
<<<<<<< Updated upstream
}
=======
}
>>>>>>> Stashed changes
