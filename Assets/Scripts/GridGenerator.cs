using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public GameObject cellPrefab;
    public int columns = 7;
    public int rows = 5;
    public float spacing = 0.45f;

    private bool isGenerated;

    public void GenerateGrid()
    {
        if (isGenerated)
        {
            return;
        }

        GameObject prefab = cellPrefab != null ? cellPrefab : CreateRuntimeCellPrefab();

        for (int i = 0; i < columns * rows; i++)
        {
            GameObject cell = Instantiate(prefab, transform);
            int row = i / columns;
            int col = i % columns;
            cell.name = $"Cell_{col}_{row}";
            cell.transform.localPosition = new Vector3(
                (col - (columns - 1) * 0.5f) * spacing,
                ((rows - 1) * 0.5f - row) * spacing,
                0f
            );
            cell.transform.localRotation = Quaternion.identity;
            cell.transform.localScale = Vector3.one * 0.38f;
        }

        if (cellPrefab == null)
        {
            Destroy(prefab);
        }

        isGenerated = true;
    }

    private GameObject CreateRuntimeCellPrefab()
    {
        GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cell.name = "RuntimeInventoryCell";
        cell.transform.localScale = new Vector3(1f, 1f, 0.08f);

        BoxCollider collider = cell.GetComponent<BoxCollider>();
        collider.isTrigger = true;

        cell.AddComponent<CellTrigger>();

        MeshRenderer renderer = cell.GetComponent<MeshRenderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        renderer.material = shader != null ? new Material(shader) : new Material(renderer.material);
        renderer.material.color = new Color(0.08f, 0.1f, 0.13f, 0.88f);

        return cell;
    }
}
