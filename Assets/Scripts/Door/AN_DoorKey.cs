using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AN_DoorKey : MonoBehaviour
{
    public enum KeyType
    {
        Red,
        Blue
    }

    [Header("Key Settings")]
    public KeyType keyType;

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
        switch (keyType)
        {
            case KeyType.Red:
                hero.RedKey = true;
                Debug.Log("[AN_DoorKey] Red Key picked up!");
                break;

            case KeyType.Blue:
                hero.BlueKey = true;
                Debug.Log("[AN_DoorKey] Blue Key picked up!");
                break;
        }

        // Mark this specific key as collected
        PlayerPrefs.SetInt("KeyPickedUp_" + keyID, 1);
        PlayerPrefs.Save();

        // Save key state
        if (SaveSystem.Instance != null)
            SaveSystem.Instance.SaveKeys();

        // Show HUD message
        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowKeyMessage(keyType == KeyType.Red);

        Destroy(gameObject);
    }

    bool InRange()
    {
        if (Camera.main == null) return false;

        return Vector3.Distance(
            transform.position,
            Camera.main.transform.position
        ) < pickupRange;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = keyType == KeyType.Red ? Color.red : Color.blue;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}