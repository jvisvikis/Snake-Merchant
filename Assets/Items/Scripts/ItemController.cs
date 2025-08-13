using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public bool ContainsCell(Vector2Int checkCell, bool checkItemStructure = false)
    {
        if (!attachedToGrid)
            return false;

        var containsItemBounds =
            checkCell.x >= cell.x && checkCell.x < cell.x + itemData.Width && checkCell.y >= cell.y && checkCell.y < cell.y + itemData.Height;

        if (containsItemBounds)
            return true;

        if (checkItemStructure)
        {
            // TODO implement for things like boots and irregularly shaped objects if needed
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        if (!itemData)
            return;

        Gizmos.color = itemData.debugColor;

        var cellSize = game.Grid.CellSize;
        var itemCells = itemData.GetCellStructure();

        for (int x = 0; x < itemData.Width; x++)
        {
            for (int y = 0; y < itemData.Height; y++)
            {
                var cellPosition = transform.position + new Vector3(x * cellSize, y * cellSize);
                if (itemCells[x][y] && (!attachedToGrid || !game.Snake.ContainsCell(cell + new Vector2Int(x, y))))
                    Gizmos.DrawCube(cellPosition + new Vector3(cellSize / 2, cellSize / 2), cellSize * Vector3.one);
            }
        }
    }
}
