using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class PuzzleManager : MonoBehaviour
{
    [Header("Correct Order")]
    public List<int> correctOrder = new List<int>();

    [Header("Door UI")]
    public GameObject doorPrompt;
    public float messageDuration = 3f;

    private TextMeshProUGUI promptText;

    private List<int> playerInput = new List<int>();
    private List<PuzzleTorch> activatedTorches = new List<PuzzleTorch>();

    private bool puzzleSolved = false;
    private bool showingMessage = false;

    void Start()
    {
        if (doorPrompt != null)
        {
            promptText = doorPrompt.GetComponent<TextMeshProUGUI>();
            doorPrompt.SetActive(false);
        }
    }

    public void TorchActivated(int id, PuzzleTorch torch)
    {
        if (puzzleSolved) return;

        playerInput.Add(id);
        activatedTorches.Add(torch);
    }

    public bool CheckCombinationOnDoorInteract()
    {
        if (puzzleSolved)
            return true;

        if (showingMessage)
            return false;

        if (playerInput.Count == 0)
        {
            ShowMessage("Light the torches to open the door");
            return false;
        }

        if (playerInput.Count < correctOrder.Count)
        {
            ShowMessage("Light all torches to open the door");
            return false;
        }

        for (int i = 0; i < correctOrder.Count; i++)
        {
            if (playerInput[i] != correctOrder[i])
            {
                ShowMessage("Light the torches in the correct order");
                ResetTorches();
                return false;
            }
        }

        puzzleSolved = true;
        ShowMessage("");
        return true;
    }

    void ShowMessage(string message)
    {
        if (doorPrompt == null) return;

        doorPrompt.SetActive(true);
        promptText.text = message;

        StopAllCoroutines();
        StartCoroutine(MessageRoutine());
    }

    IEnumerator MessageRoutine()
    {
        showingMessage = true;

        yield return new WaitForSeconds(messageDuration);

        showingMessage = false;
        promptText.text = "Press [E]";
    }

    public void ShowPressE()
    {
        if (doorPrompt == null || showingMessage) return;

        doorPrompt.SetActive(true);
        promptText.text = "Press [E]";
    }

    public void HidePrompt()
    {
        if (doorPrompt != null)
            doorPrompt.SetActive(false);
    }

    void ResetTorches()
    {
        foreach (var torch in activatedTorches)
        {
            if (torch != null)
                torch.ResetTorch();
        }

        activatedTorches.Clear();
        playerInput.Clear();
    }
}