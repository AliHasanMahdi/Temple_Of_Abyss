using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AN_DoorKey : MonoBehaviour
{
    [Tooltip("True - red key object, false - blue key")]
    public bool isRedKey = true;

    // Using a private variable for the hero reference
    private AN_HeroInteractive hero;

    // NearView() variables cached for slight performance gain
    private float distance;
    private Vector3 direction;

    private void Start()
    {
        // OPINION: FindAnyObjectByType is the correct choice here 
        // because there is usually only one Hero. It is faster than FindFirst.
        hero = Object.FindAnyObjectByType<AN_HeroInteractive>();

        if (hero == null)
        {
            Debug.LogError($"AN_DoorKey: No AN_HeroInteractive found in the scene! {gameObject.name} won't work.");
        }
    }

    void Update()
    {
        // Check for null hero to prevent crashes if Find failed
        if (hero != null && NearView() && Input.GetKeyDown(KeyCode.E))
        {
            if (isRedKey) hero.RedKey = true;
            else hero.BlueKey = true;

            // Optional: Add a small sound or particle effect here before destroying
            Destroy(gameObject);
        }
    }

    bool NearView()
    {
        // Using the main camera reference
        Transform camTransform = Camera.main.transform;

        distance = Vector3.Distance(transform.position, camTransform.position);

        // We only return true if player is within 2 units
        return distance < 2f;
    }
}