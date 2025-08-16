using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ItemController : MonoBehaviour
{
    private ItemData itemData;
    private Vector2Int cell;

    public ItemData ItemData => itemData;
    public Vector2Int Cell => cell;

    private Game game;
    private bool attachedToGrid = false;

    private void Awake()
    {
        game = FindObjectOfType<Game>();
    }

    /// <summary>
    /// Returns true if it was able to move left
    /// </summary>
    /// <returns></returns>
    public bool MoveLeft()
    {
        if (!attachedToGrid)
            return false;

        if (cell.x == 0)
            return false;

        cell.x--;
        transform.localPosition = new Vector3(transform.localPosition.x - game.Grid.CellSize, transform.localPosition.y, transform.localPosition.z);

        return true;
    }

    public void SetGridLocation(Vector2Int loc)
    {
        cell = loc;
        attachedToGrid = true;
    }

    public void SetData(ItemData itemData)
    {
        this.itemData = itemData;
    }

    public bool ContainsCell(Vector2Int gridCell)
    {
        if (!attachedToGrid)
            return false;

        var containsItemBounds =
            gridCell.x >= cell.x && gridCell.x < cell.x + itemData.Width && gridCell.y >= cell.y && gridCell.y < cell.y + itemData.Height;

        if (containsItemBounds)
            return true;

        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!itemData)
            return;

        var cellSize = game.Grid.CellSize;
        var itemCells = itemData.GetCellStructure();

        var cubeSize = cellSize * Vector3.one;
        if (itemData.IsMunchie || itemData.IsCoin)
            cubeSize *= 0.5f;

        for (int x = 0; x < itemData.Width; x++)
        {
            for (int y = 0; y < itemData.Height; y++)
            {
                var cellPosition = transform.position + new Vector3(x * cellSize, y * cellSize);
                var cellType = itemCells[x][y];
                if (cellType != ItemData.CellType.Empty && (!attachedToGrid || !game.Snake.ContainsCell(cell + new Vector2Int(x, y))))
                {
                    Gizmos.color = itemData.debugColor;
                    Gizmos.DrawCube(cellPosition + new Vector3(cellSize / 2, cellSize / 2), cubeSize);
                    if (cellType == ItemData.CellType.Middle)
                    {
                        Gizmos.color = Color.black;
                        Gizmos.DrawLine(cellPosition, cellPosition + cellSize * Vector3.one);
                    }
                }
            }
        }

        // if (ItemData.IsCoin)
        //     Handles.Label(transform.position, $"{game.CoinSpawnCountdown}");
    }
#endif
}
