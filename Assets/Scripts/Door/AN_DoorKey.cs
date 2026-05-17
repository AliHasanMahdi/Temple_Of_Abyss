using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AN_DoorKey : MonoBehaviour
{
    [Tooltip("True = Red Key,  False = Blue Key")]
    public bool isRedKey = true;

    [Tooltip("Unique ID for this key — used to stop it respawning after death")]
    public string keyID = "Key_01";

    [Tooltip("How close the player must be to pick up the key")]
    public float pickupRange = 2f;

    private AN_HeroInteractive hero;

    void Start()
    {
        hero = Object.FindAnyObjectByType<AN_HeroInteractive>();

        if (hero == null)
            Debug.LogError("[AN_DoorKey] No AN_HeroInteractive found! " + gameObject.name + " won't work.");

        // If this key was already picked up before the player died, destroy it immediately
        if (PlayerPrefs.GetInt("KeyPickedUp_" + keyID, 0) == 1)
        {
            Debug.Log("[AN_DoorKey] Key already collected, not respawning: " + keyID);
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (hero == null) return;
        if (!InRange()) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            PickUp();
        }
    }

    void PickUp()
    {
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

        // Mark this specific key as collected so it won't respawn after death
        PlayerPrefs.SetInt("KeyPickedUp_" + keyID, 1);
        PlayerPrefs.Save();

        // Save key state to PlayerPrefs so it survives death
        if (SaveSystem.Instance != null)
            SaveSystem.Instance.SaveKeys();

        // Show correct key message
        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowKeyMessage(isRedKey);

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