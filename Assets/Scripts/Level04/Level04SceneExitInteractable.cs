using UnityEngine;
using UnityEngine.SceneManagement;

public class Level04SceneExitInteractable : Interactable
{
    [SerializeField] string nextSceneName = "Level05_Chamber";
    [SerializeField] float interactionDistance = 4f;
    [SerializeField] bool saveBeforeTransition = true;

    bool isTransitioning;

    public override bool CanInteract(GameObject interactor)
    {
        if (!base.CanInteract(interactor) || isTransitioning)
            return false;

        return IsWithinDistance(interactor, interactionDistance);
    }

    public override void Interact(GameObject interactor)
    {
        if (isTransitioning || string.IsNullOrWhiteSpace(nextSceneName))
            return;

        isTransitioning = true;

        if (saveBeforeTransition && SaveSystem.Instance != null)
            SaveSystem.Instance.SaveGame();

        SceneManager.LoadScene(nextSceneName);
    }
}
