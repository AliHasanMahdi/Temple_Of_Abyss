using UnityEngine;

public class InventoryMouseController : MonoBehaviour 
{
    private InventoryItem selectedItem;
    public float holdDistance = 10f; 
    public Transform inventoryGridTransform; // Drag your InventoryGrid here in the Inspector

    void Update() 
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
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
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = holdDistance;
            selectedItem.transform.position = Camera.main.ScreenToWorldPoint(mousePos);

            if (Input.GetKeyDown(KeyCode.R)) selectedItem.Rotate();

            if (Input.GetMouseButtonUp(0)) 
            {
                if (selectedItem.hoveredCells.Count > 0) 
                {
                    selectedItem.transform.position = selectedItem.GetSnapPosition();
                    selectedItem.transform.position += new Vector3(0, 0, -0.1f);

                    // NEW: Store it inside the grid!
                    selectedItem.transform.SetParent(inventoryGridTransform);
                }
                selectedItem = null;
            }
        }
    }
}