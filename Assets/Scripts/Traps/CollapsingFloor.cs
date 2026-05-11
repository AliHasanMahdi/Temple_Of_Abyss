using System.Collections;
using UnityEngine;

public class CollapsingFloor : MonoBehaviour
{
    [Header("Settings")]
    public float timeToCollapse = 2f;
    public float shakeIntensity = 0.05f;
    public float shakeSpeed = 20f;
    public float respawnDelay = 5f;

    private float timer = 0f;
    private bool playerOnFloor = false;
    private bool isCollapsing = false;
    private Vector3 originalPosition;
    private Collider floorCollider;
    private Renderer floorRenderer;

    void Start()
    {
        originalPosition = transform.position;
        floorCollider = GetComponent<Collider>();
        floorRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (playerOnFloor && !isCollapsing)
        {
            timer += Time.deltaTime;

            float shakeOffset = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity * (timer / timeToCollapse);
            transform.position = originalPosition + new Vector3(shakeOffset, 0f, 0f);

            if (timer >= timeToCollapse)
            {
                StartCoroutine(Collapse());
            }
        }

        // Continuously check if player is still standing on this floor
        if (!isCollapsing)
            CheckPlayerStillOn();
    }

    void CheckPlayerStillOn()
    {
        // Cast a small box upward from the floor surface to detect the player
        Vector3 center = transform.position + Vector3.up * 0.1f;
        Vector3 halfExtents = new Vector3(transform.localScale.x * 0.4f, 0.15f, transform.localScale.z * 0.4f);
        Collider[] hits = Physics.OverlapBox(center, halfExtents);

        bool found = false;
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                found = true;
                break;
            }
        }

        if (!found && playerOnFloor && !isCollapsing)
        {
            // Player stepped off, reset
            playerOnFloor = false;
            timer = 0f;
            transform.position = originalPosition;
        }

        if (found && !playerOnFloor && !isCollapsing)
        {
            playerOnFloor = true;
        }
    }

    IEnumerator Collapse()
    {
        isCollapsing = true;
        playerOnFloor = false;

        // Drop the floor using Rigidbody
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;

        yield return new WaitForSeconds(0.2f);
        floorCollider.enabled = false;

        if (respawnDelay > 0f)
        {
            yield return new WaitForSeconds(respawnDelay);
            Respawn(rb);
        }
        else
        {
            floorRenderer.enabled = false;
        }
    }

    void Respawn(Rigidbody rb)
    {
        Destroy(rb);
        transform.position = originalPosition;
        floorCollider.enabled = true;
        floorRenderer.enabled = true;
        timer = 0f;
        playerOnFloor = false;
        isCollapsing = false;
    }
}