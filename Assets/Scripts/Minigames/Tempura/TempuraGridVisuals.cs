using UnityEngine;

public class TempuraGridVisuals : MonoBehaviour
{
    [Header("Grid Configuration")]
    public float spacing = 1.1f;
    public GameObject cubePrefab;

    [Header("Materials")]
    public Material matEmpty;
    public Material matPath;
    public Material matStart;
    public Material matEnd;
    public Material matPassed;

    [Header("Arrow Indicator")]
    public GameObject arrowPrefab;
    public float arrowHeightOffset = 1.0f;

    [Header("Tempura Settings")]
    public GameObject tempuraPrefab;
    public float tempuraHoverHeight = 0.8f;
    public float tempuraMoveSpeed = 12f;
    public float tempuraRotationSpeed = 360f; 
    public Vector3 rotationAxis = Vector3.right;

    private GameObject[,] gridTiles;
    private GameObject arrowInstance;
    private GameObject tempuraInstance;
    private Vector3 tempuraTargetPosition;

    public void SpawnGrid(TempuraGridLogic.TileType[,] logic, int w, int h)
    {
        ClearGrid();
        gridTiles = new GameObject[w, h];

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Vector3 spawnPos = GetTileWorldPosition(x, y);
                GameObject cube = Instantiate(cubePrefab, spawnPos, Quaternion.identity, transform);
                gridTiles[x, y] = cube;

                UpdateTile(x, y, logic[x, y]);
            }
        }
    }

    private Vector3 GetTileWorldPosition(int x, int y)
    {
        return transform.position + new Vector3(x * spacing, 0, y * spacing);
    }

    public void UpdateTile(int x, int y, TempuraGridLogic.TileType type)
    {
        if (gridTiles == null || gridTiles[x, y] == null) return;

        Material targetMat = matEmpty;
        switch (type)
        {
            case TempuraGridLogic.TileType.Path: targetMat = matPath; break;
            case TempuraGridLogic.TileType.Start: targetMat = matStart; break;
            case TempuraGridLogic.TileType.End: targetMat = matEnd; break;
            case TempuraGridLogic.TileType.Passed: targetMat = matPassed; break;
        }

        gridTiles[x, y].GetComponent<MeshRenderer>().material = targetMat;
    }

    public void SpawnArrow()
    {
        if (arrowPrefab != null && arrowInstance == null)
        {
            arrowInstance = Instantiate(arrowPrefab, transform);
        }
    }

    public void UpdateArrowIndicator(int currentX, int currentY, int nextX, int nextY, bool hide)
    {
        if (arrowInstance == null) return;

        if (hide)
        {
            arrowInstance.SetActive(false);
            return;
        }

        arrowInstance.SetActive(true);

        Vector3 current3DPos = GetTileWorldPosition(currentX, currentY) + Vector3.up * arrowHeightOffset;
        Vector3 next3DPos = GetTileWorldPosition(nextX, nextY) + Vector3.up * arrowHeightOffset;

        arrowInstance.transform.position = Vector3.Lerp(current3DPos, next3DPos, 0.5f);

        Vector3 dir3D = (next3DPos - current3DPos).normalized;
        if (dir3D != Vector3.zero)
        {
            arrowInstance.transform.rotation = Quaternion.LookRotation(dir3D);
        }
    }

    public void SpawnTempura(int startX, int startY)
    {
        if (tempuraPrefab != null && tempuraInstance == null)
        {
            tempuraInstance = Instantiate(tempuraPrefab, transform);
        }

        if (tempuraInstance != null)
        {
            Vector3 startPos = GetTileWorldPosition(startX, startY) + Vector3.up * tempuraHoverHeight;
            tempuraInstance.transform.position = startPos;
            tempuraTargetPosition = startPos;
        }
    }

    public void UpdateTempuraTarget(int destX, int destY)
    {
        tempuraTargetPosition = GetTileWorldPosition(destX, destY) + Vector3.up * tempuraHoverHeight;
    }

    private void Update()
    {
        if (tempuraInstance != null)
        {
            tempuraInstance.transform.position = Vector3.Lerp(
                tempuraInstance.transform.position,
                tempuraTargetPosition,
                Time.deltaTime * tempuraMoveSpeed
            );

            if (Vector3.Distance(tempuraInstance.transform.position, tempuraTargetPosition) > 0.01f)
            {
                tempuraInstance.transform.Rotate(rotationAxis, tempuraRotationSpeed * Time.deltaTime, Space.Self);
            }
        }
    }

    public void ClearGrid()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        gridTiles = null;
        arrowInstance = null;
        tempuraInstance = null;
    }
}