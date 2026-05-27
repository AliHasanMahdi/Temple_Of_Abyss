using UnityEngine;
using UnityEngine.SceneManagement; // This gives us access to scene-loading functions

public class LoadNewArea : MonoBehaviour
{
    // Type the exact name of the scene you want to load here.
    // We will add it in the Unity Inspector in the next step.
    public string sceneToLoad;

    // This function is called automatically when another object's collider enters this trigger zone.
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered is the Player.
        // This prevents enemies or other objects from accidentally triggering the level change.
        if (other.CompareTag("Player"))
        {
            // Load the scene we specified in the Inspector.
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}