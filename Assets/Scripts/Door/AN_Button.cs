using UnityEngine;

public class AN_Button : MonoBehaviour, IPlayerInteractable
{
    public static event System.Action<AN_Button> LeverInteractionTriggered;

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
    [Space]
    [Tooltip("True for rotation by X local rotation by valve")]
    public bool xRotation = true;
    [Tooltip("True for vertical movenment by valve (if xRotation is false)")]
    public bool yPosition = false;
    public float max = 90f, min = 0f, speed = 5f;
    bool valveBool = true;
    float current, startYPosition;
    Quaternion startQuat, rampQuat;

    Animator anim;
    AudioSource audioSource;
    AudioClip buttonSound;
    AudioClip leverSound;

    // NearView()
    float distance;
    float angleView;
    Vector3 direction;

    public bool IsPressed
    {
        get
        {
            if (isValve)
                return isOpened;

            if (anim == null)
                anim = GetComponent<Animator>();

            if (isLever && anim != null)
                return anim.GetBool("LeverUp");

            return isOpened;
        }
    }

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
        if (!Locked)
        {
            if (UseLegacyInteraction() && Input.GetKeyDown(KeyCode.E) && !isValve && NearView()) // 1.lever and 2.button
            {
                TriggerDiscreteInteraction();
            }
            else if (isValve && RampObject != null) // 3.valve
            {
                // changing value in script
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

                // using value on object
                transform.rotation = startQuat * Quaternion.Euler(0f, 0f, current * ValveSpeed);
                if (xRotation) RampObject.rotation = rampQuat * Quaternion.Euler(current, 0f, 0f);
                else if (yPosition) RampObject.position = new Vector3(RampObject.position.x, startYPosition + current, RampObject.position.z);
            }
        }
    }

    public bool CanInteract(GameObject interactor)
    {
        return enabled && gameObject.activeInHierarchy && !Locked && !isValve && NearViewFrom(interactor);
    }

    public string GetPromptText()
    {
        if (isLever)
            return "Press E to pull lever";

        return "Press E to press button";
    }

    public void Interact(GameObject interactor)
    {
        if (isValve || Locked)
            return;

        TriggerDiscreteInteraction();
    }

    bool NearView() // it is true if you near interactive object
    {
        distance = Vector3.Distance(transform.position, Camera.main.transform.position);
        direction = transform.position - Camera.main.transform.position;
        angleView = Vector3.Angle(Camera.main.transform.forward, direction);
        if (angleView < 45f && distance < 2f) return true;
        else return false;
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

        float promptDistance = Vector3.Distance(transform.position, origin.position);
        Vector3 promptDirection = transform.position - origin.position;
        float promptAngle = Vector3.Angle(origin.forward, promptDirection);
        return promptAngle < 45f && promptDistance < 2f;
    }

    bool UseLegacyInteraction()
    {
        return Object.FindFirstObjectByType<PlayerInteraction>() == null;
    }

    void TriggerDiscreteInteraction()
    {
        PlayButtonSound();

        if (DoorObject != null && DoorObject.Remote)
        {
            DoorObject.Action();
            if (isLever)
            {
                if (DoorObject.isOpened) anim.SetBool("LeverUp", true);
                else anim.SetBool("LeverUp", false);
            }
            else anim.SetTrigger("ButtonPress");
        }
        else if (isLever)
        {
            anim.SetBool("LeverUp", !anim.GetBool("LeverUp"));
        }
        else
        {
            anim.SetTrigger("ButtonPress");
        }

        if (spikeTargets != null)
        {
            foreach (LeverSpikeTarget spike in spikeTargets)
            {
                if (spike != null) spike.Activate();
            }
        }

        if (isLever)
            LeverInteractionTriggered?.Invoke(this);
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
