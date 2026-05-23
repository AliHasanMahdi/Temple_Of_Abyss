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
    public float unlockDelay = 1.5f;   // pause before door opens (dramatic effect)

    private int totalEnemies;
    private int deadEnemies;
    private bool roomCleared = false;

    void Start()
    {
        // Auto-collect enemies if list is empty — finds all EnemyHealth children
        if (enemies.Count == 0)
        {
            EnemyHealth[] found = GetComponentsInChildren<EnemyHealth>();
            foreach (EnemyHealth e in found)
                enemies.Add(e);
        }

        totalEnemies = enemies.Count;
        deadEnemies = 0;

        // Assign this room to every enemy that doesn't have one set
        foreach (EnemyHealth e in enemies)
        {
            if (e != null && e.enemyRoom == null)
                e.enemyRoom = this;
        }

        // Make sure door starts locked
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

    // Called by EnemyHealth.Die()
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
        // Show message
        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowTimedMessage("Room Cleared!", 3f);

        Debug.Log("[EnemyRoom] All enemies dead — unlocking door in " + unlockDelay + "s");

        yield return new WaitForSeconds(unlockDelay);

        // Unlock and open the door
        if (doorToUnlock != null)
        {
            doorToUnlock.Locked = false;
            doorToUnlock.CanOpen = true;
            doorToUnlock.PlayUnlockSound();
            doorToUnlock.Action(); // auto-open it
            Debug.Log("[EnemyRoom] Door unlocked: " + doorToUnlock.name);
        }

        if (secondDoor != null)
        {
            secondDoor.Locked = false;
            secondDoor.CanOpen = true;
            secondDoor.PlayUnlockSound();
            secondDoor.Action();
            Debug.Log("[EnemyRoom] Second door unlocked: " + secondDoor.name);
        }

        // Save door state so it stays unlocked after death
        if (SaveSystem.Instance != null)
        {
            if (doorToUnlock != null)
                SaveSystem.Instance.SaveDoorUnlocked(doorToUnlock.doorID);
            if (secondDoor != null)
                SaveSystem.Instance.SaveDoorUnlocked(secondDoor.doorID);
        }
    }

    // Draw a gizmo so you can see the room in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = roomCleared ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(10, 4, 10));
    }
}
