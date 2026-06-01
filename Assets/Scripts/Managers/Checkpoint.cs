using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    const int CheckpointScoreReward = 5;
    private bool isActivated = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            isActivated = true;

            if (HUDManager.Instance != null)
                HUDManager.Instance.AddScore(CheckpointScoreReward);

            // Save everything — scene, position, inventory, score
            if (SaveSystem.Instance != null)
                SaveSystem.Instance.SaveGame();

            // Show HUD message
            if (HUDManager.Instance != null)
                HUDManager.Instance.ShowCheckpointMessage();

            // Change colour to green
            Renderer r = GetComponent<Renderer>();
            if (r != null)
                r.material.color = Color.green;
        }
    }
}
