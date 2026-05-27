using UnityEngine;

public class KeypadDoor : MonoBehaviour
{
    public float openSpeed = 3f;
    public float openAngle = -90f;
    public AudioSource audioSource;
    public AudioClip openSound;

    private bool isOpen = false;
    private Quaternion startRotation;
    private Quaternion targetRotation;

    void Start()
    {
        startRotation = transform.localRotation;
        targetRotation = startRotation;
    }

    void Update()
    {
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * openSpeed
        );
    }

    public void OpenDoor()
    {
        if (isOpen) return;

        isOpen = true;
        targetRotation = startRotation * Quaternion.Euler(0, 0, openAngle);

        if (audioSource != null && openSound != null)
            audioSource.PlayOneShot(openSound);
    }
}