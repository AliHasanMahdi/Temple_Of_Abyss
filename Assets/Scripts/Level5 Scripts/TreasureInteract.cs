using UnityEngine;

public class TreasureInteract : MonoBehaviour
{
    private TreasureRoom treasureRoom;
    private bool playerNearby = false;

    void Start()
    {
        treasureRoom = FindObjectOfType<TreasureRoom>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            treasureRoom.PlayerNearTreasure(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            treasureRoom.PlayerNearTreasure(false);
        }
    }
}