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

    private void Awake()
    {
        game = FindObjectOfType<Game>();
    }

    public void SetGridLocation(Vector2Int loc)
    {
        cell = loc;
    }

    public void SetData(ItemData itemData)
    {
        this.itemData = itemData;
    }

    public bool ContainsCell(Vector2Int checkCell, bool checkItemStructure = false)
    {
        // TODO I've seen an apple spawn over a boot, is that because this logic is broken?
        var containsItemBounds =
            checkCell.x >= cell.x && checkCell.x < cell.x + itemData.Width && checkCell.y >= cell.y && checkCell.y < cell.y + itemData.Height;

        if (!containsItemBounds)
            return false;

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
        var itemCells = itemData.GetCells();

        for (int x = 0; x < itemData.Width; x++)
        {
            for (int y = 0; y < itemData.Height; y++)
            {
                var cellPosition = transform.position + new Vector3(x * cellSize, y * cellSize);
                if (itemCells[x][y])
                    Gizmos.DrawCube(cellPosition + new Vector3(cellSize / 2, cellSize / 2), cellSize * Vector3.one);
            }
        }
    }
}
