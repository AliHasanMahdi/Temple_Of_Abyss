using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AN_DoorKey : MonoBehaviour
{
    [Tooltip("True = Red Key,  False = Blue Key")]
    public bool isRedKey = true;

    [Tooltip("How close the player must be to pick up the key")]
    public float pickupRange = 2f;

    private AN_HeroInteractive hero;

    void Start()
    {
        hero = Object.FindAnyObjectByType<AN_HeroInteractive>();

        if (hero == null)
            Debug.LogError("[AN_DoorKey] No AN_HeroInteractive found in scene! " + gameObject.name + " won't work.");
    }

    void Update()
    {
        if (hero == null) return;
        if (!InRange()) return;

        // New Input System
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            PickUp();
        }
    }

    void PickUp()
    {
        // Give the key to the player
        if (isRedKey)
        {
            hero.RedKey = true;
            Debug.Log("[AN_DoorKey] Red Key picked up!");
        }
        else
        {
            hero.BlueKey = true;
            Debug.Log("[AN_DoorKey] Blue Key picked up!");
        }

        // Save keys to PlayerPrefs immediately so they survive death
        if (SaveSystem.Instance != null)
            SaveSystem.Instance.SaveKeys();

        // Show HUD message
        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowCheckpointMessage();

        Destroy(gameObject);
    }

    bool InRange()
    {
        if (Camera.main == null) return false;
        return Vector3.Distance(transform.position, Camera.main.transform.position) < pickupRange;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isRedKey ? Color.red : Color.blue;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}
