using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRoom : MonoBehaviour
{
    [Header("Enemies in this room")]
    [Tooltip("Drag all enemy GameObjects in this room here")]
    public List<EnemyHealth> enemies = new List<EnemyHealth>();

    [Header("Door to unlock when all enemies die")]
    [Tooltip("Drag the door GameObject with AN_DoorScript here")]
    public AN_DoorScript doorToUnlock;

    [Header("Optional second door")]
    public AN_DoorScript secondDoor;

    [Header("Settings")]
    public float unlockDelay = 1.5f;

    private int totalEnemies;
    private int deadEnemies;
    private bool roomCleared = false;

    void Start()
    {
        if (enemies.Count == 0)
        {
            EnemyHealth[] found = GetComponentsInChildren<EnemyHealth>();
            foreach (EnemyHealth e in found)
                enemies.Add(e);
        }

        totalEnemies = enemies.Count;
        deadEnemies = 0;

        foreach (EnemyHealth e in enemies)
        {
            if (e != null && e.enemyRoom == null)
                e.enemyRoom = this;
        }

        if (doorToUnlock != null)
        {
            doorToUnlock.Locked = true;
            doorToUnlock.CanOpen = false;
        }
        if (secondDoor != null)
        {
            secondDoor.Locked = true;
            secondDoor.CanOpen = false;
        }

        Debug.Log("[EnemyRoom] Room has " + totalEnemies + " enemies. Door locked.");
    }

    public void OnEnemyDied()
    {
        if (roomCleared) return;

        deadEnemies++;
        Debug.Log("[EnemyRoom] Enemy died. " + deadEnemies + "/" + totalEnemies + " dead.");

        if (deadEnemies >= totalEnemies)
        {
            roomCleared = true;
            StartCoroutine(UnlockDoorRoutine());
        }
    }

    IEnumerator UnlockDoorRoutine()
    {
        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowTimedMessage("Room Cleared!", 3f);

        Debug.Log("[EnemyRoom] All enemies dead — unlocking door in " + unlockDelay + "s");

        yield return new WaitForSeconds(unlockDelay);

        if (doorToUnlock != null)
        {
            doorToUnlock.Locked = false;
            doorToUnlock.CanOpen = true;
            doorToUnlock.Action();
            Debug.Log("[EnemyRoom] Door unlocked: " + doorToUnlock.name);
        }

        if (secondDoor != null)
        {
            secondDoor.Locked = false;
            secondDoor.CanOpen = true;
            secondDoor.Action();
            Debug.Log("[EnemyRoom] Second door unlocked: " + secondDoor.name);
        }

        // Store in memory only — written to disk when player hits a checkpoint
        if (SaveSystem.Instance != null)
        {
            if (doorToUnlock != null)
                SaveSystem.Instance.PendingDoorUnlocked(doorToUnlock.doorID);
            if (secondDoor != null)
                SaveSystem.Instance.PendingDoorUnlocked(secondDoor.doorID);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = roomCleared ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(10, 4, 10));
    }
}