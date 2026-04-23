using UnityEngine;

public class AN_DoorScript : MonoBehaviour
{
    [Header("Door Settings")]
    [Tooltip("If it is false door can't be used")]
    public bool Locked = false;
    [Tooltip("It is true for remote control only")]
    public bool Remote = false;

    [Space]
    public bool CanOpen = true;
    public bool CanClose = true;

    [Header("Key Settings")]
    [Tooltip("Door locked by red key")]
    public bool RedLocked = false;
    public bool BlueLocked = false;

    [Header("Animation Settings")]
    public bool isOpened = false;
    [Range(0f, 4f)]
    public float OpenSpeed = 3f;

    // Internal References
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

        // Using the faster modern method
        HeroInteractive = Object.FindAnyObjectByType<AN_HeroInteractive>();

        if (HeroInteractive == null)
            Debug.LogWarning("AN_DoorScript: No Hero found in scene.");
    }

    void Update()
    {
        if (!Remote && Input.GetKeyDown(KeyCode.E) && NearView())
        {
            Action();
        }
    }

    public void Action()
    {
        if (Locked) return;

        // 1. Try to unlock with keys
        if (HeroInteractive != null)
        {
            if (RedLocked && HeroInteractive.RedKey)
            {
                RedLocked = false;
                HeroInteractive.RedKey = false; // Key used up
                Debug.Log("Red Door Unlocked");
            }
            else if (BlueLocked && HeroInteractive.BlueKey)
            {
                BlueLocked = false;
                HeroInteractive.BlueKey = false; // Key used up
                Debug.Log("Blue Door Unlocked");
            }
        }

        // 2. If door is still locked by a key requirement, stop here
        if (RedLocked || BlueLocked)
        {
            Debug.Log("Door is locked! You need a key.");
            return;
        }

        // 3. Handle Opening/Closing
        if (isOpened && CanClose)
        {
            isOpened = false;
        }
        else if (!isOpened && CanOpen)
        {
            isOpened = true;
            rbDoor.AddRelativeTorque(new Vector3(0, 0, 20f));
        }
    }

    bool NearView()
    {
        if (mainCam == null) return false;

        float distance = Vector3.Distance(transform.position, mainCam.transform.position);
        // If you want to use angleView logic, uncomment the lines below:
        // Vector3 direction = transform.position - mainCam.transform.position;
        // float angleView = Vector3.Angle(mainCam.transform.forward, direction);

        return distance < 3f;
    }

    private void FixedUpdate()
    {
        // Target limit based on state
        float targetLim = isOpened ? 85f : 0f;

        // Smoothly move the limit toward the target
        if (isOpened)
        {
            currentLim = 85f;
        }
        else
        {
            if (currentLim > 1f)
                currentLim -= 0.5f * OpenSpeed;
            else
                currentLim = 0f;
        }

        // Apply to Hinge
        hingeLim.max = currentLim;
        hingeLim.min = -currentLim;
        hinge.limits = hingeLim;
        hinge.useLimits = true; // Ensure limits are actually enabled
    }
}