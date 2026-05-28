using UnityEngine;

public abstract class Interactable : MonoBehaviour, IPlayerInteractable
{
    [Header("Interaction")]
    public string displayName = "object";
    public string promptVerb = "interact with";
    public Transform interactionOrigin;

    public virtual bool CanInteract(GameObject interactor)
    {
        return enabled && gameObject.activeInHierarchy;
    }

    public virtual string GetPromptText()
    {
        return "Press E to " + promptVerb + " " + displayName;
    }

    protected bool IsWithinDistance(GameObject interactor, float maxDistance)
    {
        if (interactor == null)
            return false;

        Transform origin = interactionOrigin != null ? interactionOrigin : transform;
        Transform other = interactor.transform;
        return Vector3.Distance(origin.position, other.position) <= maxDistance;
    }

    public abstract void Interact(GameObject interactor);
}
