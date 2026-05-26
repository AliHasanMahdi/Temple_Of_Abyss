using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public TMP_Text introText;
    public TMP_Text keyCountText;
    public TMP_Text promptText;
    public GameObject winPanel;

    private int keysFound = 0;

    void Awake() { instance = this; }

    void Start()
    {
        winPanel.SetActive(false);
        keyCountText.text = "Keys: 0 / 5";
        promptText.text = "";
        StartCoroutine(ShowIntro());
    }

    IEnumerator ShowIntro()
    {
        introText.text = "You are trapped!\nFind 5 keys hidden in boxes.\nUnlock the doors...\nFind the Treasure Room to WIN!";
        yield return new WaitForSeconds(5f);
        introText.text = "";
    }

    public void KeyFound()
    {
        keysFound++;
        keyCountText.text = "Keys: " + keysFound + " / 5";
    }

    public bool HasKey() { return keysFound > 0; }

    public void UseKey()
    {
        keysFound--;
        keyCountText.text = "Keys: " + keysFound + " / 5";
    }

    public void Win() { winPanel.SetActive(true); }
}