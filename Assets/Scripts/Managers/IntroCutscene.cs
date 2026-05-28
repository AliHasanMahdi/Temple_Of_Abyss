using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.Animations;

public class IntroCutscene : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Transform demon;
    public Transform player;
    public DemonAI demonAI;
    public LookAtConstraint cameraConstraint;

    [Header("Subtitles UI")]
    public GameObject subtitlePanel;
    public TextMeshProUGUI subtitleText;

    [Header("Voice Lines")]
    [TextArea(5, 10)]
    public string demonLine = "What the fuck? You think you can sneak up on me? Huh? I've killed bigger. Bigger. Meaner. With my bare hands. But you're giving me twenty seconds? That's cute. Real cute. I'll use those seconds to figure which part of you I'll wear as a necklace. Now get the fuck away before I skip this countdown.";

    [TextArea(3, 5)]
    public string americanLine = "Oh shit... what the hell am I even doing here? I need to get the fuck out of here right now.";

    [Header("Audio")]
    public AudioClip demonVoiceClip;
    public AudioClip americanVoiceClip;
    [Range(0f, 1f)]
    public float voiceVolume = 1f;

    [Header("Timing")]
    public float demonLineDuration = 27f;
    public float americanLineDuration = 14f;

    public MonoBehaviour[] scriptsToDisable;

    void Start()
    {
        if (demonAI != null) demonAI.enabled = false;
        DisablePlayerControls();
        SetupLookAtConstraint();
        StartCoroutine(CutsceneSequence());
    }

    void SetupLookAtConstraint()
    {
        if (cameraConstraint == null)
            cameraConstraint = playerCamera.gameObject.AddComponent<LookAtConstraint>();

        cameraConstraint.AddSource(new ConstraintSource { sourceTransform = demon, weight = 1 });
        cameraConstraint.constraintActive = false;
        cameraConstraint.locked = true;
        cameraConstraint.rotationAtRest = playerCamera.transform.eulerAngles;
        cameraConstraint.rotationOffset = new Vector3(-40f, 0f, 0f);
    }

    IEnumerator CutsceneSequence()
    {
        yield return new WaitForSeconds(0.2f);

        // --- DEMON DIALOGUE (player frozen) ---
        if (subtitlePanel != null) subtitlePanel.SetActive(true);
        if (subtitleText != null) subtitleText.text = demonLine;
        if (demonVoiceClip != null)
            AudioSource.PlayClipAtPoint(demonVoiceClip, demon.position, voiceVolume);

        float blendTime = 0f;
        float blendDuration = 1.5f;
        cameraConstraint.constraintActive = true;
        while (blendTime < blendDuration)
        {
            blendTime += Time.deltaTime;
            cameraConstraint.weight = Mathf.Clamp01(blendTime / blendDuration);
            yield return null;
        }
        cameraConstraint.weight = 1f;

        yield return new WaitForSeconds(demonLineDuration);

        // --- SNAP BACK, UNFREEZE PLAYER, DEMON HUNTS ---
        cameraConstraint.constraintActive = false;
        cameraConstraint.weight = 0f;
        playerCamera.transform.rotation = Quaternion.Euler(cameraConstraint.rotationAtRest);

        EnablePlayerControls();

        if (demonAI != null)
        {
            demonAI.enabled = true;
            demonAI.StartHunting();   // instant chase, no warning
        }

        // --- AMERICAN DIALOGUE (player can move) ---
        if (subtitleText != null) subtitleText.text = americanLine;
        if (americanVoiceClip != null)
            AudioSource.PlayClipAtPoint(americanVoiceClip, player.position, voiceVolume);

        yield return new WaitForSeconds(americanLineDuration);

        if (subtitlePanel != null) subtitlePanel.SetActive(false);
    }

    void DisablePlayerControls()
    {
        foreach (var script in scriptsToDisable)
            if (script != null) script.enabled = false;
        Cursor.lockState = CursorLockMode.None;
    }

    void EnablePlayerControls()
    {
        foreach (var script in scriptsToDisable)
            if (script != null) script.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
    }
}