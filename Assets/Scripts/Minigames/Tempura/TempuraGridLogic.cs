using System.Collections.Generic;
using UnityEngine;

public class TempuraGridLogic : MonoBehaviour
{
    public enum TileType { Empty, Path, Start, End, Passed }

    [Header("Grid config")]
    public int width = 4;
    public int height = 4;

    public TileType[,] gridLogic { get; private set; }
    public List<Vector2Int> currentPath { get; private set; }

    public void GenerateNewGrid()
    {
        gridLogic = new TileType[width, height];
        currentPath = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                gridLogic[x, y] = TileType.Empty;

        Vector2Int current = new Vector2Int(0, Random.Range(0, height));
        gridLogic[current.x, current.y] = TileType.Start;
        currentPath.Add(current);

        int maxAttempts = 100;
        int attempts = 0;

        while (current.x < width - 1 && attempts < maxAttempts)
        {
            attempts++;
            List<Vector2Int> validNeighbors = GetValidNeighbors(current);

            if (validNeighbors.Count > 0)
            {
                Vector2Int next = validNeighbors[Random.Range(0, validNeighbors.Count)];
                gridLogic[next.x, next.y] = TileType.Path;
                currentPath.Add(next);
                current = next;
            }
            else
            {
                GenerateNewGrid();
                return;
            }
        }
        gridLogic[current.x, current.y] = TileType.End;
    }

    private List<Vector2Int> GetValidNeighbors(Vector2Int pos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] dirs = {
            new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 0),
            new Vector2Int(1, 1), new Vector2Int(1, -1)
        };

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