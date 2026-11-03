using System.Collections.Generic;
using UnityEngine;

public class TempuraGridLogic : MonoBehaviour
{
    public enum TileType { Empty, Path, Start, End, Passed }

    private const int MaxAttempts = 100;

    [Header("Grid config")]
    public int width = 4;
    public int height = 4;

    [Header("Diagonals per pattern")]
    [SerializeField] private int minDiagonals = 1;
    [SerializeField] private int maxDiagonals = 2;

    public TileType[,] gridLogic { get; private set; }
    public List<Vector2Int> currentPath { get; private set; }

    public void GenerateNewGrid()
    {
        int diagonalTarget = Random.Range(minDiagonals, maxDiagonals + 1);

        for (int attempt = 0; attempt < MaxAttempts; attempt++)
        {
            if (TryGenerateGrid(diagonalTarget))
            {
                return;
            }
        }

        TryGenerateGrid(0);
    }

    private bool TryGenerateGrid(int diagonalTarget)
    {
        gridLogic = new TileType[width, height];
        currentPath = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                gridLogic[x, y] = TileType.Empty;

        Vector2Int current = new Vector2Int(0, Random.Range(0, height));
        gridLogic[current.x, current.y] = TileType.Start;
        currentPath.Add(current);

        int diagonalsUsed = 0;

        while (current.x < width - 1)
        {
            List<Vector2Int> validNeighbors = GetValidNeighbors(current, diagonalsUsed < diagonalTarget);

            if (validNeighbors.Count == 0)
            {
                return false;
            }

            Vector2Int next = validNeighbors[Random.Range(0, validNeighbors.Count)];

            if (next.x != current.x && next.y != current.y)
            {
                diagonalsUsed++;
            }

            gridLogic[next.x, next.y] = TileType.Path;
            currentPath.Add(next);
            current = next;
        }

        if (diagonalsUsed < diagonalTarget)
        {
            return false;
        }

        gridLogic[current.x, current.y] = TileType.End;

        return true;
    }

    private List<Vector2Int> GetValidNeighbors(Vector2Int pos, bool allowDiagonals)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        List<Vector2Int> dirs = new List<Vector2Int> { new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 0) };

        if (allowDiagonals)
        {
            dirs.Add(new Vector2Int(1, 1));
            dirs.Add(new Vector2Int(1, -1));
        }

        foreach (var dir in dirs)
        {
            Vector2Int check = pos + dir;
            if (check.x >= 0 && check.x < width && check.y >= 0 && check.y < height)
            {
                if (gridLogic[check.x, check.y] == TileType.Empty)
                {
                    neighbors.Add(check);
                }
            }
        }
        return neighbors;
    }
}
