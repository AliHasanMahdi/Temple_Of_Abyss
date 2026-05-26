using UnityEngine;

public class AN_Plug : MonoBehaviour, IPlayerInteractable
{
    public bool OneTime = false;
    public Transform HeroHandsPosition;
    public Transform Socket;
    public AN_DoorScript DoorObject;

    [Header("Interaction")]
    public float interactionDistance = 3f;
    public float socketSnapDistance = 1.25f;

    [Header("Carry Pose")]
    public Vector3 carryLocalPosition = new Vector3(0.35f, -0.25f, 1f);
    public Vector3 carryLocalEulerAngles = new Vector3(20f, 0f, 0f);

    private Rigidbody rb;
    private Collider[] allColliders;
    private bool isCarried;
    private bool isInserted;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        allColliders = GetComponentsInChildren<Collider>(true);
    }

    void Start()
    {
        if (Socket == null)
            Socket = FindNearestSocket();
    }

    public bool CanInteract(GameObject interactor)
    {
        if (!enabled || !gameObject.activeInHierarchy || isInserted)
            return false;

        if (isCarried)
            return true;

        return IsWithinRange(interactor);
    }

    public string GetPromptText()
    {
        if (isInserted)
            return string.Empty;

        return isCarried ? "Press E to place plug" : "Press E to pick up plug";
    }

    public void Interact(GameObject interactor)
    {
        if (isInserted)
            return;

        if (isCarried)
        {
            if (!TryInsertIntoSocket())
                Drop(interactor);

            return;
        }

        Pickup(interactor);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isCarried || isInserted || other == null)
            return;

        if (Socket == null && IsSocketObject(other.transform))
            Socket = other.transform;

        TryInsertIntoSocket();
    }

    void Pickup(GameObject interactor)
    {
        Transform carryAnchor = ResolveCarryAnchor(interactor);
        if (carryAnchor == null)
            return;

        transform.SetParent(carryAnchor, false);
        transform.localPosition = carryLocalPosition;
        transform.localRotation = Quaternion.Euler(carryLocalEulerAngles);

        SetPhysicsState(carried: true, inserted: false);
        isCarried = true;
    }

    void Drop(GameObject interactor)
    {
        Transform origin = GetInteractionOrigin(interactor);
        transform.SetParent(null, true);

        if (origin != null)
            transform.position = origin.position + (origin.forward * 1.2f) - (origin.up * 0.2f);

        SetPhysicsState(carried: false, inserted: false);
        isCarried = false;
    }

    bool TryInsertIntoSocket()
    {
        Transform socketTarget = ResolveSocketTarget();
        if (socketTarget == null)
            return false;

        if (Vector3.Distance(transform.position, socketTarget.position) > socketSnapDistance)
            return false;

        transform.SetParent(socketTarget, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        SetPhysicsState(carried: false, inserted: true);
        isCarried = false;
        isInserted = true;

        ActivateDoor();

        if (OneTime)
            enabled = false;

        return true;
    }

    void ActivateDoor()
    {
        if (DoorObject == null)
            return;

        DoorObject.Locked = false;

        if (!DoorObject.isOpened || DoorObject.Remote)
            DoorObject.Action();
    }

    Transform ResolveCarryAnchor(GameObject interactor)
    {
        if (HeroHandsPosition != null)
            return HeroHandsPosition;

        AN_HeroInteractive hero = interactor != null
            ? interactor.GetComponent<AN_HeroInteractive>() ?? interactor.GetComponentInChildren<AN_HeroInteractive>()
            : null;

        if (hero != null && hero.GoalPosition != null)
        {
            HeroHandsPosition = hero.GoalPosition;
            return HeroHandsPosition;
        }

        PlayerMovement movement = interactor != null
            ? interactor.GetComponent<PlayerMovement>() ?? interactor.GetComponentInChildren<PlayerMovement>()
            : null;

        if (movement != null && movement.ViewTransform != null)
        {
            Transform anchor = movement.ViewTransform.Find("CarryAnchor");
            if (anchor == null)
            {
                GameObject anchorObject = new GameObject("CarryAnchor");
                anchor = anchorObject.transform;
                anchor.SetParent(movement.ViewTransform, false);
                anchor.localPosition = carryLocalPosition;
                anchor.localRotation = Quaternion.Euler(carryLocalEulerAngles);
            }

            if (hero != null && hero.GoalPosition == null)
                hero.GoalPosition = anchor;

            HeroHandsPosition = anchor;
            return anchor;
        }

        return interactor != null ? interactor.transform : null;
    }

    Transform ResolveSocketTarget()
    {
        if (Socket == null)
            Socket = FindNearestSocket();

        if (Socket == null)
            return null;

        if (Socket.name.Contains("Socket_Zone"))
            return Socket.parent != null ? Socket.parent : Socket;

        return Socket;
    }

    Transform FindNearestSocket()
    {
        Transform best = null;
        float bestDistance = float.MaxValue;

        Collider[] colliders = FindObjectsByType<Collider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (Collider candidate in colliders)
        {
            if (candidate == null || !candidate.isTrigger)
                continue;

            Transform candidateTransform = candidate.transform;
            if (!IsSocketObject(candidateTransform))
                continue;

            float distance = Vector3.Distance(transform.position, candidateTransform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = candidateTransform;
            }
        }

        return best;
    }

    bool IsSocketObject(Transform candidate)
    {
        if (candidate == null)
            return false;

        string objectName = candidate.name;
        if (objectName.Contains("Socket_Zone") || objectName.Contains("Socket_Prefab"))
            return true;

        return candidate.parent != null && candidate.parent.name.Contains("Socket_Prefab");
    }

    Transform GetInteractionOrigin(GameObject interactor)
    {
        if (interactor == null)
            return null;

        PlayerMovement movement = interactor.GetComponent<PlayerMovement>() ?? interactor.GetComponentInChildren<PlayerMovement>();
        if (movement != null && movement.ViewTransform != null)
            return movement.ViewTransform;

        return interactor.transform;
    }

    bool IsWithinRange(GameObject interactor)
    {
        Transform origin = GetInteractionOrigin(interactor);
        if (origin == null)
            return false;

        return Vector3.Distance(transform.position, origin.position) <= interactionDistance;
    }

    void SetPhysicsState(bool carried, bool inserted)
    {
        if (rb != null)
        {
            rb.isKinematic = carried || inserted;
            rb.useGravity = !(carried || inserted);
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (allColliders == null)
            return;

        foreach (Collider collider in allColliders)
        {
            if (collider == null)
                continue;

            if (inserted)
            {
                collider.enabled = false;
            }
            else
            {
                collider.enabled = !carried || collider.isTrigger;
            }
        }
    }
}
