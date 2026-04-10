using UnityEngine;

public class GridGenerator : MonoBehaviour {
    public GameObject cellPrefab;
    private const int Columns = 7;
    private const int Rows = 5;
    public float spacing = 1.05f;
    private bool isGenerated = false;

    public void GenerateGrid() {
        if (isGenerated) return;

        for (int i = 0; i < (Columns * Rows); i++) {
            GameObject cell = Instantiate(cellPrefab, transform);
            int row = i / Columns;
            int col = i % Columns;
            cell.transform.localPosition = new Vector3(col * spacing, -row * spacing, 0);
        }
        isGenerated = true;
    }
}