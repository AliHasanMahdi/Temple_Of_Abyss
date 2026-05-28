using UnityEngine;

public class TreasureInteract : MonoBehaviour
{
    private TreasureRoom treasureRoom;

    void Start()
    {
        treasureRoom = FindFirstObjectByType<TreasureRoom>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            treasureRoom.PlayerNearTreasure(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            treasureRoom.PlayerNearTreasure(false);
        }
    }
}
