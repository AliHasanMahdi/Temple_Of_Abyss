using UnityEngine;

public class AN_Button : MonoBehaviour, IPlayerInteractable
{
    [Tooltip("True for rotation like valve (used for ramp/elevator only)")]
    public bool isValve = false;
    [Tooltip("SelfRotation speed of valve")]
    public float ValveSpeed = 10f;
    [Tooltip("If it isn't valve, it can be lever or button (animated)")]
    public bool isLever = false;
    [Tooltip("If it is false door can't be used")]
    public bool Locked = false;
    [Tooltip("The door for remote control")]
    public AN_DoorScript DoorObject;

    [Space]
    [Header("Spike Traps (optional)")]
    [Tooltip("Drag any LeverSpikeTarget GameObjects here to fire them when the lever is pulled")]
    public LeverSpikeTarget[] spikeTargets;

    [Space]
    [Tooltip("Any object for ramp/elevator baheviour")]
    public Transform RampObject;
    [Tooltip("Door can be opened")]
    public bool CanOpen = true;
    [Tooltip("Door can be closed")]
    public bool CanClose = true;
    [Tooltip("Current status of the door")]
    public bool isOpened = false;

    [Header("Interaction")]
    public float interactionDistance = 2f;

    [Space]
    [Tooltip("True for rotation by X local rotation by valve")]
    public bool xRotation = true;
    [Tooltip("True for vertical movenment by valve (if xRotation is false)")]
    public bool yPosition = false;
    public float max = 90f, min = 0f, speed = 5f;

    bool valveBool = true;
    float current;
    float startYPosition;
    Quaternion startQuat;
    Quaternion rampQuat;

    Animator anim;
    AudioSource audioSource;
    AudioClip buttonSound;
    AudioClip leverSound;

    float distance;
    float angleView;
    Vector3 direction;

    void Start()
    {
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 12f;
            audioSource.playOnAwake = false;
        }
        else
        {
            audioSource.maxDistance = Mathf.Max(audioSource.maxDistance, 12f);
        }

        LoadDefaultSound();
        if (RampObject != null)
            startYPosition = RampObject.position.y;

        startQuat = transform.rotation;
        if (RampObject != null)
            rampQuat = RampObject.rotation;
    }

    void Update()
    {
        if (Locked || !isValve || RampObject == null)
            return;

        if (Input.GetKey(KeyCode.E) && NearView())
        {
            if (!audioSource.isPlaying)
                PlayButtonSound();

            if (valveBool)
            {
                if (!isOpened && CanOpen && current < max) current += speed * Time.deltaTime;
                if (isOpened && CanClose && current > min) current -= speed * Time.deltaTime;

                if (current >= max)
                {
                    isOpened = true;
                    valveBool = false;
                }
                else if (current <= min)
                {
                    isOpened = false;
                    valveBool = false;
                }
            }
        }
        else
        {
            if (!isOpened && current > min) current -= speed * Time.deltaTime;
            if (isOpened && current < max) current += speed * Time.deltaTime;
            valveBool = true;
        }

        transform.rotation = startQuat * Quaternion.Euler(0f, 0f, current * ValveSpeed);
        if (xRotation)
            RampObject.rotation = rampQuat * Quaternion.Euler(current, 0f, 0f);
        else if (yPosition)
            RampObject.position = new Vector3(RampObject.position.x, startYPosition + current, RampObject.position.z);
    }

    public bool CanInteract(GameObject interactor)
    {
        return !Locked && !isValve && enabled && gameObject.activeInHierarchy && IsWithinRange(interactor);
    }

    public string GetPromptText()
    {
        return isLever ? "Press E to use lever" : "Press E to press button";
    }

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor))
            return;

        ActivateButton();
    }

    void ActivateButton()
    {
        PlayButtonSound();

        if (DoorObject != null && DoorObject.Remote)
        {
            DoorObject.Action();
            if (isLever && anim != null)
                anim.SetBool("LeverUp", DoorObject.isOpened);
            else if (anim != null)
                anim.SetTrigger("ButtonPress");
        }
        else if (isLever)
        {
            if (anim != null)
                anim.SetBool("LeverUp", !anim.GetBool("LeverUp"));
        }
        else if (anim != null)
        {
            anim.SetTrigger("ButtonPress");
        }

        if (spikeTargets == null)
            return;

        foreach (LeverSpikeTarget spike in spikeTargets)
        {
            if (spike != null)
                spike.Activate();
        }
    }

    bool NearView()
    {
        if (Camera.main == null)
            return false;

        distance = Vector3.Distance(transform.position, Camera.main.transform.position);
        direction = transform.position - Camera.main.transform.position;
        angleView = Vector3.Angle(Camera.main.transform.forward, direction);
        return angleView < 45f && distance < 2f;
    }

    bool IsWithinRange(GameObject interactor)
    {
        if (interactor == null)
            return false;

        Transform origin = interactor.transform;
        PlayerMovement movement = interactor.GetComponent<PlayerMovement>();
        if (movement != null && movement.ViewTransform != null)
            origin = movement.ViewTransform;

        return Vector3.Distance(transform.position, origin.position) <= interactionDistance;
    }

    void PlayButtonSound()
    {
        LoadDefaultSound();
        AudioClip clip = isLever ? leverSound : buttonSound;
        if (clip == null) clip = buttonSound;
        if (audioSource == null || clip == null) return;

        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(clip, 0.75f);
        TempleAudio.PlaySfx(clip, 0.75f);
    }

    void LoadDefaultSound()
    {
        if (buttonSound == null)
            buttonSound = TempleAudio.LoadClip("TempleAudio/SFX/Call Classic Old Lift Elevator Button With Ride 1");
        if (leverSound == null)
            leverSound = TempleAudio.LoadClip("TempleAudio/SFX/clank1");
    }
}
