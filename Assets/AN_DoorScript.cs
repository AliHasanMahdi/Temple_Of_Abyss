using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
        {
            Debug.LogWarning("AN_DoorScript: No Hero found in scene.");
        }
    }

    void Update()
    {
        if (!Remote &&
            WasInteractPressed() &&
            NearView() &&
            !AN_DoorKey.IsAnyCollectableKeyNearby() &&
            AN_DoorKey.LastCollectFrame != Time.frameCount)
        {
            Action();
        }
    }

    public void Action()
    {
        bool needsKey = RedLocked || BlueLocked;

        if (Locked && !needsKey)
        {
            return;
        }

        bool unlockedWithKey = false;
        if (needsKey && !TryUnlockWithKey(out unlockedWithKey))
        {
            return;
        }

        if (isOpened && CanClose)
        {
            isOpened = false;
            return;
        }

        if (!isOpened && CanOpen)
        {
            OpenDoor();
        }
    }

    private bool TryUnlockWithKey(out bool unlockedWithKey)
    {
        unlockedWithKey = false;

        if (HeroInteractive == null)
        {
            Debug.Log("Door is locked! You need a key.");
            return false;
        }

        if (RedLocked)
        {
            if (!HeroInteractive.UseKey(true))
            {
                Debug.Log("Door is locked! You need a key.");
                return false;
            }

            RedLocked = false;
            unlockedWithKey = true;
            Debug.Log("Red Door Unlocked");
        }

        if (BlueLocked)
        {
            if (!HeroInteractive.UseKey(false))
            {
                Debug.Log("Door is locked! You need a key.");
                return false;
            }

            BlueLocked = false;
            unlockedWithKey = true;
            Debug.Log("Blue Door Unlocked");
        }

        Locked = false;
        SyncInventoryKeys();
        return true;
    }

    private void OpenDoor()
    {
        isOpened = true;
        if (rbDoor != null)
        {
            rbDoor.AddRelativeTorque(new Vector3(0, 0, 20f));
        }
    }

    private void SyncInventoryKeys()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SyncKeysFromPlayer();
        }
    }

    bool NearView()
    {
        if (mainCam == null) return false;

        float distance = Vector3.Distance(transform.position, mainCam.transform.position);
        return distance < 3f;
    }

    private bool WasInteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.E);
#else
        return false;
#endif
    }

    private void FixedUpdate()
    {
        if (hinge == null)
        {
            return;
        }

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
