using UnityEngine;

public class AN_Button : MonoBehaviour
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
    [Header("Interaction Sounds")]
    [SerializeField] AudioClip buttonSound;
    [SerializeField] AudioClip leverSound;
    [Range(0f, 1f)]
    public float interactionVolume = 0.75f;

    // NearView()
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
            ConfigureAudioSource();
        }
        else
        {
            ConfigureAudioSource();
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
            if (Input.GetKeyDown(KeyCode.E) && !isValve && NearView()) // 1.lever and 2.button
            {
                PlayButtonSound();

                // Door logic (still works if DoorObject is assigned)
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
                    // No door assigned — just animate the lever toggle
                    anim.SetBool("LeverUp", !anim.GetBool("LeverUp"));
                }
                else
                {
                    anim.SetTrigger("ButtonPress");
                }

                // Fire all connected spike traps
                if (spikeTargets != null)
                {
                    foreach (LeverSpikeTarget spike in spikeTargets)
                    {
                        if (spike != null) spike.Activate();
                    }
                }
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

    bool NearView() // it is true if you near interactive object
    {
        distance = Vector3.Distance(transform.position, Camera.main.transform.position);
        direction = transform.position - Camera.main.transform.position;
        angleView = Vector3.Angle(Camera.main.transform.forward, direction);
        if (angleView < 45f && distance < 2f) return true;
        else return false;
    }

    void PlayButtonSound()
    {
        LoadDefaultSound();
        AudioClip clip = isLever ? leverSound : buttonSound;
        if (clip == null) clip = buttonSound;
        if (audioSource == null || clip == null) return;

        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(clip, TempleAudio.ScaleSfxVolume(interactionVolume));
        TempleAudio.PlaySfx(clip, interactionVolume);
    }

    void LoadDefaultSound()
    {
        if (buttonSound == null)
            buttonSound = TempleAudio.LoadClip("TempleAudio/SFX/Call Classic Old Lift Elevator Button With Ride 1");
        if (leverSound == null)
            leverSound = TempleAudio.LoadClip("TempleAudio/SFX/clank1");
    }

    void ConfigureAudioSource()
    {
        if (audioSource == null) return;

        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = Mathf.Max(audioSource.minDistance, 1f);
        audioSource.maxDistance = Mathf.Max(audioSource.maxDistance, 12f);
        audioSource.playOnAwake = false;
    }
}
