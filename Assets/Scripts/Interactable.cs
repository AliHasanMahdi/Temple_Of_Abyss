using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Interaction")]
    public string displayName = "object";
    public string promptVerb = "interact with";

    public virtual bool CanInteract(GameObject interactor)
    {
        return enabled && gameObject.activeInHierarchy;
    }

    public virtual string GetPromptText()
    {
        return "Press E to " + promptVerb + " " + displayName;
    }

    public abstract void Interact(GameObject interactor);
}
