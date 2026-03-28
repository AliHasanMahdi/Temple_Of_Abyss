using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool isActivated = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            isActivated = true;

            // Save the game
            if (SaveSystem.Instance != null)
                SaveSystem.Instance.SaveGame();

            // Show HUD message
            if (HUDManager.Instance != null)
                HUDManager.Instance.ShowCheckpointMessage();

            // Change colour to show it's activated
            GetComponent<Renderer>().material.color = Color.green;
        }
    }
}
