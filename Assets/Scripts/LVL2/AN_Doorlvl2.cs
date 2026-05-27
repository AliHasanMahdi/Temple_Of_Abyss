using UnityEngine;
using UnityEngine.InputSystem;

public class AN_Doorlvl2 : MonoBehaviour
{
    public float openSpeed = 3f;
    public float openAngle = -90f;
    public AudioSource doorAudioSource;
    public AudioClip openSound;
    public AudioClip closeSound;

    private bool isOpened = false;
    private bool playerInside = false;

    private Quaternion startRotation;
    private Quaternion targetRotation;

    private PuzzleManager puzzleManager;

    void Start()
    {
        startRotation = transform.localRotation;
        targetRotation = startRotation;

        puzzleManager = GetComponentInParent<PuzzleManager>();

        if (puzzleManager == null)
            Debug.LogError("PuzzleManager NOT found in parent!");
    }

    void Update()
    {
        if (playerInside && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (isOpened)
            {
                ToggleDoor();
                return;
            }

            if (puzzleManager != null && puzzleManager.CheckCombinationOnDoorInteract())
            {
                ToggleDoor();
            }
        }

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * openSpeed
        );
    }

    public void SetPlayerInside(bool state)
    {
        playerInside = state;
    }

    void ToggleDoor()
    {
        isOpened = !isOpened;

        targetRotation = isOpened
            ? startRotation * Quaternion.Euler(0, 0, openAngle)
            : startRotation;

        if (doorAudioSource != null)
            doorAudioSource.PlayOneShot(isOpened ? openSound : closeSound, TempleAudio.GetSfxVolume());
    }
}
