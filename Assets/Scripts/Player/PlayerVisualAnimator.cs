using UnityEngine;

public class PlayerVisualAnimator : MonoBehaviour
{
    enum VisualState
    {
        Idle,
        Walk,
        Run,
        Jump,
        Death
    }

    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private Transform bodyVisual;
    private Renderer bodyRenderer;
    private Vector3 baseScale;
    private Vector3 basePosition;
    private Quaternion baseRotation;
    private VisualState currentState;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (bodyVisual == null)
            CacheBodyVisual();

        if (bodyVisual == null || playerMovement == null)
            return;

        currentState = ResolveState();
        AnimateState();
    }

    void CacheBodyVisual()
    {
        bodyVisual = transform.Find("BodyVisual");
        if (bodyVisual == null)
            return;

        bodyRenderer = bodyVisual.GetComponent<Renderer>();
        baseScale = bodyVisual.localScale;
        basePosition = bodyVisual.localPosition;
        baseRotation = bodyVisual.localRotation;
    }

    VisualState ResolveState()
    {
        if (playerHealth != null && playerHealth.IsDead)
            return VisualState.Death;

        if (!playerMovement.IsGrounded)
            return VisualState.Jump;

        if (playerMovement.IsRunning)
            return VisualState.Run;

        if (playerMovement.IsMoving)
            return VisualState.Walk;

        return VisualState.Idle;
    }

    void AnimateState()
    {
        float time = Time.time;

        switch (currentState)
        {
            case VisualState.Idle:
                bodyVisual.localPosition = basePosition + new Vector3(0f, Mathf.Sin(time * 1.5f) * 0.03f, 0f);
                bodyVisual.localScale = Vector3.Lerp(bodyVisual.localScale, baseScale, 8f * Time.deltaTime);
                bodyVisual.localRotation = Quaternion.Lerp(bodyVisual.localRotation, baseRotation, 8f * Time.deltaTime);
                SetColor(new Color(0.72f, 0.72f, 0.78f, 1f));
                break;

            case VisualState.Walk:
                bodyVisual.localPosition = basePosition + new Vector3(0f, Mathf.Abs(Mathf.Sin(time * 7f)) * 0.08f, 0f);
                bodyVisual.localScale = Vector3.Lerp(bodyVisual.localScale, baseScale + new Vector3(0.03f, -0.02f, 0.03f), 8f * Time.deltaTime);
                bodyVisual.localRotation = Quaternion.Lerp(bodyVisual.localRotation, Quaternion.Euler(0f, 0f, Mathf.Sin(time * 7f) * 3f), 10f * Time.deltaTime);
                SetColor(new Color(0.6f, 0.76f, 0.88f, 1f));
                break;

            case VisualState.Run:
                bodyVisual.localPosition = basePosition + new Vector3(0f, Mathf.Abs(Mathf.Sin(time * 10f)) * 0.12f, 0f);
                bodyVisual.localScale = Vector3.Lerp(bodyVisual.localScale, baseScale + new Vector3(0.05f, -0.05f, 0.05f), 10f * Time.deltaTime);
                bodyVisual.localRotation = Quaternion.Lerp(bodyVisual.localRotation, Quaternion.Euler(0f, 0f, Mathf.Sin(time * 10f) * 6f), 12f * Time.deltaTime);
                SetColor(new Color(0.9f, 0.66f, 0.3f, 1f));
                break;

            case VisualState.Jump:
                bodyVisual.localPosition = Vector3.Lerp(bodyVisual.localPosition, basePosition + new Vector3(0f, 0.08f, 0f), 8f * Time.deltaTime);
                bodyVisual.localScale = Vector3.Lerp(bodyVisual.localScale, baseScale + new Vector3(-0.08f, 0.12f, -0.08f), 8f * Time.deltaTime);
                bodyVisual.localRotation = Quaternion.Lerp(bodyVisual.localRotation, Quaternion.Euler(-10f, 0f, 0f), 8f * Time.deltaTime);
                SetColor(new Color(0.48f, 0.9f, 0.74f, 1f));
                break;

            case VisualState.Death:
                bodyVisual.localPosition = Vector3.Lerp(bodyVisual.localPosition, basePosition + new Vector3(0f, -0.5f, 0f), 4f * Time.deltaTime);
                bodyVisual.localScale = Vector3.Lerp(bodyVisual.localScale, baseScale, 4f * Time.deltaTime);
                bodyVisual.localRotation = Quaternion.Lerp(bodyVisual.localRotation, Quaternion.Euler(0f, 0f, 90f), 4f * Time.deltaTime);
                SetColor(new Color(0.24f, 0.12f, 0.12f, 1f));
                break;
        }
    }

    void SetColor(Color color)
    {
        if (bodyRenderer != null)
            bodyRenderer.material.color = color;
    }
}
