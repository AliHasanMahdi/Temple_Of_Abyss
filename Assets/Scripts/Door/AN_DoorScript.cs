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

    [Header("Door ID — must be unique per door in the scene")]
    [Tooltip("Give every door a unique name e.g. Door_RedLeft_01  Used to remember unlock state across death.")]
    public string doorID = "Door_01";

    [Header("Animation Settings")]
    public bool isOpened = false;
    [Range(0f, 4f)]
    public float OpenSpeed = 3f;

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

        if (HeroInteractive == null)
            Debug.LogWarning("AN_DoorScript: No AN_HeroInteractive found in scene.");

        // If this door was unlocked before the player died, restore that state
        if (SaveSystem.Instance != null && SaveSystem.Instance.IsDoorUnlocked(doorID))
        {
            RedLocked = false;
            BlueLocked = false;
            isOpened = true;
            currentLim = 85f;
            Debug.Log("[AN_DoorScript] Door restored as unlocked: " + doorID);
        }
    }

    void Update()
    {
        // New Input System
        if (!Remote && Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame && NearView())
        {
            Action();
        }
    }

    public void Action()
    {
        if (Locked) return;

        // Try to unlock with keys
        if (HeroInteractive != null)
        {
            if (RedLocked && HeroInteractive.RedKey)
            {
                RedLocked = false;
                HeroInteractive.RedKey = false;

                // Save: door is now unlocked permanently (survives death)
                if (SaveSystem.Instance != null)
                    SaveSystem.Instance.SaveDoorUnlocked(doorID);

                // Save: key is now consumed
                if (SaveSystem.Instance != null)
                    SaveSystem.Instance.SaveKeys();

                Debug.Log("[AN_DoorScript] Red Door unlocked: " + doorID);
            }
            else if (BlueLocked && HeroInteractive.BlueKey)
            {
                BlueLocked = false;
                HeroInteractive.BlueKey = false;

                if (SaveSystem.Instance != null)
                    SaveSystem.Instance.SaveDoorUnlocked(doorID);

                if (SaveSystem.Instance != null)
                    SaveSystem.Instance.SaveKeys();

                Debug.Log("[AN_DoorScript] Blue Door unlocked: " + doorID);
            }
        }

        // Still locked — player doesn't have the right key
        if (RedLocked || BlueLocked)
        {
            if (HUDManager.Instance != null)
                HUDManager.Instance.ShowTimedMessage("You need a key!", 2f);
            Debug.Log("[AN_DoorScript] Door locked — key required.");
            return;
        }

        // Open or close
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
        return Vector3.Distance(transform.position, mainCam.transform.position) < 3f;
    }

    void FixedUpdate()
    {
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

        hingeLim.max = currentLim;
        hingeLim.min = -currentLim;
        hinge.limits = hingeLim;
        hinge.useLimits = true;
    }
}