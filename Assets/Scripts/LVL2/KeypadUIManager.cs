using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class KeypadUIManager : MonoBehaviour
{
    [Header("Password Settings")]
    public string correctPassword = "1234";
    private const int maxDigits = 4;

    [Header("References")]
    public TextMeshProUGUI displayText;
    public GameObject keypadPanel;
    public PasswordDoor door;
    public GameObject doorPrompt;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip buttonClickSound;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip closeSound;

    [Range(0f, 1f)]
    public float volume = 1f;

    private string currentInput = "";
    private bool keypadOpen = false;

    void Start()
    {
        keypadPanel.SetActive(false);
        UpdateDisplay();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // ✅ Allow ESC to close keypad
        if (keypadOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseKeypad();
        }
    }

    public void OpenKeypad()
    {
        keypadPanel.SetActive(true);
        currentInput = "";
        UpdateDisplay();

        keypadOpen = true;

        if (doorPrompt != null)
            doorPrompt.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseKeypad()
    {
        keypadPanel.SetActive(false);

        keypadOpen = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        PlaySound(closeSound);
    }

    public void AddDigit(string digit)
    {
        if (currentInput.Length >= maxDigits)
            return;

        currentInput += digit;
        UpdateDisplay();
        PlaySound(buttonClickSound);
    }

    public void ClearInput()
    {
        currentInput = "";
        UpdateDisplay();
        PlaySound(buttonClickSound);
    }

    public void SubmitPassword()
    {
        if (currentInput.Length < maxDigits)
        {
            displayText.text = "Enter 4 digits";
            PlaySound(wrongSound);
            return;
        }

        if (currentInput == correctPassword)
        {
            displayText.text = "Access Granted";
            PlaySound(correctSound);

            door.OpenDoor();
            Invoke(nameof(CloseKeypad), 1f);
        }
        else
        {
            displayText.text = "Wrong Code";
            PlaySound(wrongSound);

            currentInput = "";
            Invoke(nameof(UpdateDisplay), 1f);
        }
    }

    void UpdateDisplay()
    {
        string visual = "";

        for (int i = 0; i < maxDigits; i++)
        {
            if (i < currentInput.Length)
                visual += currentInput[i] + " ";
            else
                visual += "_ ";
        }

        displayText.text = visual;
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip, volume);
    }
}