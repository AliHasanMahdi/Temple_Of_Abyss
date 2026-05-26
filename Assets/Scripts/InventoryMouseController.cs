using UnityEngine;

// Legacy compatibility component. The current runtime inventory uses UI slots
// and does not rely on the old draggable world-item inventory workflow.
public class InventoryMouseController : MonoBehaviour
{
    public float holdDistance = 10f;
    public Transform inventoryGridTransform;
}
