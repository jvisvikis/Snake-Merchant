using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class Grid
{
    private int width;
    private int height;
    private float cellSize;
    private Vector3 orig;

    public float CellSize => cellSize;
    public int Width => width;
    public int Height => height;
    public Vector2 Orig => orig;

    public Grid(int width, int height, float cellSize, Vector3 orig)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.orig = orig;
    }

    public Vector3 CellCenterOffset()
    {
        return new Vector3(cellSize / 2, cellSize / 2);
    }

    public Vector3 GetWorldPos(int x, int y)
    {
        return new Vector3(x * cellSize, y * cellSize) + orig;
    }

    public Vector3 GetWorldPos(Vector2Int pos)
    {
        return GetWorldPos(pos.x, pos.y);
    }

    public Vector2Int GetCell(Vector3 worldPos)
    {
        worldPos -= orig;
        return new Vector2Int((int)Mathf.Round(worldPos.x / cellSize), (int)Mathf.Round(worldPos.y / cellSize));
    }

    public bool InGrid(int x, int y)
    {
        return x < width && x >= 0 && y < height && y >= 0;
    }

    /// <summary>
    /// Get a random cell coordinate in this grid that will fit an object of blockWidth and
    /// blockHeight, within the spawnable area. The position is returned as a Vector2Int of the
    /// minimum value i.e. the bottom-left corner conceptually in Unity.
    /// </summary>
    public Vector2Int RandomSpawnCell(int blockWidth, int blockHeight, bool borderOk = false)
    {
        Debug.Assert(blockWidth > 0);
        Debug.Assert(blockWidth <= width);
        Debug.Assert(blockHeight > 0);
        Debug.Assert(blockHeight <= height);
        var border = borderOk ? 0 : 1;
        return new Vector2Int(Random.Range(border, width - blockWidth - border), Random.Range(border, height - blockHeight - border));
    }

    public void DrawGrid()
    {
        for (int row = 0; row < width; row++)
        {
            for (int col = 0; col < height; col++)
            {
                Debug.DrawLine(GetWorldPos(row, col), GetWorldPos(row + 1, col), Color.white, Time.deltaTime);
                Debug.DrawLine(GetWorldPos(row, col), GetWorldPos(row, col + 1), Color.white, Time.deltaTime);
            }
        }
        Debug.DrawLine(GetWorldPos(0, height), GetWorldPos(width, height), Color.white, Time.deltaTime);
        Debug.DrawLine(GetWorldPos(width, height), GetWorldPos(width, 0), Color.white, Time.deltaTime);
    }
}
