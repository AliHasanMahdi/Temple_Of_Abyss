using UnityEngine;
using System.Collections;

public class DoorTriggerForwarder : MonoBehaviour
{
    private AN_Doorlvl2 door;
    private PuzzleManager puzzleManager;

    private bool playerNear = false;
    private Coroutine hideCoroutine;

    void Start()
    {
        Transform parent = transform.parent;

        if (parent != null)
        {
            door = parent.GetComponentInChildren<AN_Doorlvl2>();
            puzzleManager = parent.GetComponent<PuzzleManager>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerNear = true;

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (door != null)
            door.SetPlayerInside(true);

        if (puzzleManager != null)
            puzzleManager.ShowPressE();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerNear = false;
        hideCoroutine = StartCoroutine(DelayedHide());
    }

    IEnumerator DelayedHide()
    {
        yield return new WaitForSeconds(0.15f);

        if (!playerNear)
        {
            if (door != null)
                door.SetPlayerInside(false);

            if (puzzleManager != null)
                puzzleManager.HidePrompt();
        }

        hideCoroutine = null;
    }
}