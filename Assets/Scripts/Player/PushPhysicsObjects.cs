using UnityEngine;

public class PushPhysicsObjects : MonoBehaviour
{
    public float pushPower = 2.0f;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // Does the object have a Rigidbody and is it NOT kinematic?
        if (body == null || body.isKinematic) return;

        // We don't want to push objects below us
        if (hit.moveDirection.y < -0.3) return;

        // Calculate push direction from move direction
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // Apply the push
        body.linearVelocity = pushDir * pushPower;
    }
}