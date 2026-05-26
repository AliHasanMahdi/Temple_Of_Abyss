using UnityEngine;
using System.Collections.Generic;

public class InventoryItem : MonoBehaviour 
{
    public List<GameObject> hoveredCells = new List<GameObject>();

    void OnTriggerEnter(Collider other) 
    {
        // Check for "Cell" in the name to avoid tag errors
        if (other.name.Contains("Cell") && !hoveredCells.Contains(other.gameObject)) 
        {
            hoveredCells.Add(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other) 
    {
        if (other.name.Contains("Cell")) 
        {
            hoveredCells.Remove(other.gameObject);
        }
    }

    // NEW: This calculates the middle point between all touched cells
    public Vector3 GetSnapPosition()
    {
        if (hoveredCells.Count == 0) return transform.position;

        Vector3 sumPosition = Vector3.zero;
        foreach (GameObject cell in hoveredCells)
        {
            sumPosition += cell.transform.position;
        }
        
        // Return the average center point of all cells
        return sumPosition / hoveredCells.Count;
    }

    public void Rotate() 
    {
        transform.localEulerAngles += new Vector3(0, 0, 90);
    }
}