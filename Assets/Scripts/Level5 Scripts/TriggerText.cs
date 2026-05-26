using UnityEngine;
using TMPro;
using System.Collections;

public class TriggerText : MonoBehaviour
{
    public TMP_Text displayText;
    public string message = "Find keys somewhere within the boxes.";
    public float displayDuration = 5f;
    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !triggered)
        {
            triggered = true;
            StartCoroutine(ShowText());
        }
    }

    IEnumerator ShowText()
    {
        displayText.text = message;
        yield return new WaitForSeconds(displayDuration);
        displayText.text = "";
    }
}