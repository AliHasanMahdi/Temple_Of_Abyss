using UnityEngine;
using UnityEngine.AI;
using Debug = UnityEngine.Debug;

public class LavaResetHandler : MonoBehaviour
{
    [Header("Reset References")]
    public Transform playerRespawnPoint;
    public Transform demonResetPoint;
    public DemonAI demonAI;
    public GameObject player;

    void Start()
    {
        // Auto-find player if not assigned
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            Debug.LogError("No player found! Assign manually or ensure 'Player' tag.");

        // Ensure this collider is a trigger (warning if not)
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
            Debug.LogWarning("Lava collider is NOT a trigger. Set 'Is Trigger' to true, or use OnCollisionEnter instead.");
    }

    // Trigger detection (preferred)
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered by: " + other.name + " (tag: " + other.tag + ")");
        if (other.CompareTag("Player"))
        {
            Debug.Log("PLAYER TOUCHED LAVA TRIGGER – RESETTING");
            ResetGame();
        }
    }

    // Collision detection (fallback)
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision entered by: " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("PLAYER COLLIDED WITH LAVA – RESETTING");
            ResetGame();
        }
    }

    void ResetGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
}