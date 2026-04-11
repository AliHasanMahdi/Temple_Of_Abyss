using UnityEngine;

public class DoorInteractable : Interactable
{
    public Vector3 openOffset = new Vector3(0f, 4f, 0f);
    public float openSpeed = 3f;

    private Vector3 closedPosition;
    private Vector3 targetPosition;
    private bool isOpen;
    private bool initialized;

    void Awake()
    {
        Initialize();
    }

    void Update()
    {
        if (!initialized)
            Initialize();

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, openSpeed * Time.deltaTime);
    }

    void Initialize()
    {
        closedPosition = transform.localPosition;
        targetPosition = closedPosition;
        initialized = true;
    }

    public override string GetPromptText()
    {
        return "Press E to " + (isOpen ? "close " : "open ") + displayName;
    }

    public override void Interact(GameObject interactor)
    {
        if (!initialized)
            Initialize();

        isOpen = !isOpen;
        targetPosition = isOpen ? closedPosition + openOffset : closedPosition;
    }
}
