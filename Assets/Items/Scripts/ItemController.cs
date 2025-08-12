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
        var containsItem =
            checkCell.x >= cell.x && checkCell.x < cell.x + itemData.Width && checkCell.y >= cell.y && checkCell.y < cell.y + itemData.Height;

        if (!containsItem)
            return false;

        if (checkItemStructure)
        {
            // TODO implement for things like boots and irregularly shaped objects
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        if (!itemData)
            return;

        var cellSize = game.Grid.CellSize;

        Gizmos.color = itemData.debugColor;
        Gizmos.DrawCube(transform.position + new Vector3(cellSize / 2, cellSize / 2), cellSize * Vector3.one);
    }
}
