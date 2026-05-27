using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class PuzzleTorch : MonoBehaviour
{
    public int torchID;
    public GameObject torchLight;
    public GameObject runeQuad;

    public GameObject torchPrompt;

    [Header("Audio")]
    public AudioSource torchAudioSource;
    public AudioClip igniteSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.8f;

    private TextMeshProUGUI promptText;
    private bool isLit = false;
    private bool playerNear = false;

    private Coroutine hideCoroutine;
    private PuzzleManager manager;

    void Start()
    {
        manager = Object.FindFirstObjectByType<PuzzleManager>();

        if (torchLight != null)
            torchLight.SetActive(false);

        if (torchPrompt != null)
        {
            promptText = torchPrompt.GetComponent<TextMeshProUGUI>();
            torchPrompt.SetActive(false);
        }

        if (torchAudioSource == null)
            torchAudioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (playerNear && !isLit &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            LightTorch();
        }
    }

    void LightTorch()
    {
        isLit = true;

        if (torchLight != null)
            torchLight.SetActive(true);

        if (runeQuad != null)
            runeQuad.SetActive(true);

        if (torchPrompt != null)
            torchPrompt.SetActive(false);

        // ✅ Play ignite sound
        if (torchAudioSource != null && igniteSound != null)
            torchAudioSource.PlayOneShot(igniteSound, soundVolume);

        if (manager != null)
            manager.TorchActivated(torchID, this);
    }

    public void ResetTorch()
    {
        isLit = false;

        if (torchLight != null)
            torchLight.SetActive(false);

        if (runeQuad != null)
            runeQuad.SetActive(false);
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

        if (!isLit && torchPrompt != null)
        {
            torchPrompt.SetActive(true);
            if (promptText != null)
                promptText.text = "Press [E]";
        }
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

        if (!playerNear && torchPrompt != null)
            torchPrompt.SetActive(false);

        hideCoroutine = null;
    }
}