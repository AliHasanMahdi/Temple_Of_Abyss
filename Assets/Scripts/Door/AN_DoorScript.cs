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

        // Restore unlocked state after player death
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
        if (Locked || !CanOpen) return;

        // Try to unlock with keys
        if (HeroInteractive != null)
        {
            if (RedLocked && HeroInteractive.RedKey)
            {
                RedLocked = false;
                HeroInteractive.RedKey = false;
                if (SaveSystem.Instance != null)
                {
                    SaveSystem.Instance.SaveDoorUnlocked(doorID);
                    SaveSystem.Instance.SaveKeys();
                }
            }
            else if (BlueLocked && HeroInteractive.BlueKey)
            {
                BlueLocked = false;
                HeroInteractive.BlueKey = false;
                if (SaveSystem.Instance != null)
                {
                    SaveSystem.Instance.SaveDoorUnlocked(doorID);
                    SaveSystem.Instance.SaveKeys();
                }
            }
        }

        // Still locked by key
        if (RedLocked || BlueLocked)
        {
            if (HUDManager.Instance != null)
                HUDManager.Instance.ShowTimedMessage("You need a key!", 2f);
            return;
        }

        // Open or close
        if (isOpened && CanClose)
            isOpened = false;
        else if (!isOpened)
        {
            isOpened = true;
            if (rbDoor != null)
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