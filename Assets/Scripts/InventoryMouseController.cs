using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class InventoryMouseController : MonoBehaviour 
{
    private InventoryItem selectedItem;
    public float holdDistance = 10f; 
    public Transform inventoryGridTransform; // Drag your InventoryGrid here in the Inspector

    void Update() 
    {
        if (Camera.main == null)
        {
            return;
        }

        if (WasLeftMousePressed()) 
        {
            Ray ray = Camera.main.ScreenPointToRay(GetMousePosition());
            if (Physics.Raycast(ray, out RaycastHit hit)) 
            {
                selectedItem = hit.transform.GetComponentInParent<InventoryItem>();
                
                // When we pick it up, take it OUT of the grid folder 
                // so it doesn't disappear if the grid closes while moving
                if (selectedItem != null)
                {
                    selectedItem.transform.SetParent(null);
                }
            }
        }

        if (selectedItem != null) 
        {
            Vector3 mousePos = GetMousePosition();
            mousePos.z = holdDistance;
            selectedItem.transform.position = Camera.main.ScreenToWorldPoint(mousePos);

            if (WasRotatePressed()) selectedItem.Rotate();

            if (WasLeftMouseReleased()) 
            {
                if (selectedItem.hoveredCells.Count > 0) 
                {
                    selectedItem.transform.position = selectedItem.GetSnapPosition();
                    selectedItem.transform.position += new Vector3(0, 0, -0.1f);

                    // NEW: Store it inside the grid!
                    if (inventoryGridTransform != null)
                    {
                        selectedItem.transform.SetParent(inventoryGridTransform);
                    }
                }
                selectedItem = null;
            }
        }
    }

    private static Vector3 GetMousePosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.mousePosition;
#else
        return Vector3.zero;
#endif
    }

    private static bool WasLeftMousePressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetMouseButtonDown(0);
#else
        return false;
#endif
    }

    private static bool WasLeftMouseReleased()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetMouseButtonUp(0);
#else
        return false;
#endif
    }

    private static bool WasRotatePressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.R);
#else
        return false;
#endif
    }
}
