using UnityEngine;

public class InventoryManager : MonoBehaviour {
    public GameObject inventoryUI;
    public GridGenerator gridGen;
    private bool isOpen = false;

    void Start() { inventoryUI.SetActive(false); }

    void Update() {
        if (Input.GetKeyDown(KeyCode.E)) {
            isOpen = !isOpen;
            inventoryUI.SetActive(isOpen);
            if (isOpen) {
                gridGen.GenerateGrid();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            } else {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}