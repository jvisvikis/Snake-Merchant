using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ItemController : MonoBehaviour
{
    public enum Rotation
    {
        Up,
        Down,
        Right,
        Left,
    }

    public ItemData ItemData => itemData;

    /// <summary>
    /// Every grid cell that this item takes up, taking into account its structure - e.g. the empty
    /// middle of a donut will not be in this list.
    /// </summary>
    public List<Vector2Int> ItemGridCells => itemGridCells;

    /// <summary>
    /// The CellType of each item in ItemGridCells. They are indexed the same.
    /// </summary>
    public List<ItemData.CellType> ItemGridCellTypes => itemGridCellTypes;

    /// <summary>
    /// Every grid cell that this item takes up including its border. Its structure is irrelevant
    /// i.e. even "Empty" cells will be in this list.
    /// </summary>
    public List<Vector2Int> BorderGridCells => borderGridCells;

    private ItemData itemData;
    private Game game;
    private bool attachedToGrid = false;
    private BoundsInt itemBounds;
    private BoundsInt borderBounds;
    private Rotation rotation = Rotation.Up;
    private List<Vector2Int> itemGridCells;
    private List<ItemData.CellType> itemGridCellTypes;
    private List<Vector2Int> borderGridCells;

    private void Awake()
    {
        game = FindObjectOfType<Game>();
    }

    public void SetData(ItemData itemData)
    {
        this.itemData = itemData;
    }

    public void SetGridLocation(BoundsInt itemBounds, BoundsInt borderBounds, Rotation rotation)
    {
        this.itemBounds = itemBounds;
        this.borderBounds = borderBounds;
        this.rotation = rotation;
        attachedToGrid = true;
        itemGridCells = CalcItemGridCells(out itemGridCellTypes);
        borderGridCells = GetBorderGridCells();

        // Debug.Log(
        //     $"[{itemData.name}] itemBounds: {itemBounds}, borderBounds: {borderBounds}, itemGridCells: {ListUtil.ListToString(itemGridCells)}, borderGridCells: {ListUtil.ListToString(borderGridCells)}",
        //     gameObject
        // );
    }

    public void SetFloating()
    {
        itemBounds = new BoundsInt(Vector3Int.zero, new Vector3Int(itemData.Width, itemData.Height));
        itemGridCells = CalcItemGridCells(out itemGridCellTypes);
    }

    /// <summary>
    /// Returns true if the item itself contains a cell. This also inspects the item's contents, so
    /// e.g. a donut will return false for its inside.
    /// </summary>
    public bool ItemContainsCell(Vector2Int gridCell, out ItemData.CellType cellType)
    {
        for (int i = 0; i < itemGridCells.Count; i++)
        {
            if (itemGridCells[i] == gridCell)
            {
                cellType = itemGridCellTypes[i];
                return true;
            }
        }
        cellType = ItemData.CellType.Empty;
        return false;
    }

    /// <summary>
    /// Rotates an x/y cell in an un-rotated ItemData into the coordinates of this item as if it
    /// were rotated. Pass in backwards=true to rotate in the opposite direction of rotation, in
    /// other words, rotating back from a rotated cell into the local item data's cell.
    /// </summary>
    /// <param name="itemCell"></param>
    /// <returns></returns>
    private Vector2Int RotateCell(Vector2Int itemCell, bool backwards)
    {
        var left = new Vector2Int(itemData.Height - itemCell.y - 1, itemCell.x);
        var right = new Vector2Int(itemCell.y, itemData.Width - itemCell.x - 1);

        switch (rotation)
        {
            case Rotation.Up:
                return itemCell;
            case Rotation.Down:
                return new Vector2Int(itemData.Width - itemCell.x - 1, itemData.Height - itemCell.y - 1);
            case Rotation.Left:
                return backwards ? right : left;
            case Rotation.Right:
                return backwards ? left : right;
        }

        Debug.Assert(false);
        return itemCell;
    }

    /// <summary>
    /// Returns true if the item plus any spacing for entry/exit contains this cell. Useful for
    /// determining whether that cell can have a different item spawned at it, for example.
    /// The structure of the item is not taken into account.
    /// </summary>
    public bool ItemBorderContainsCell(Vector2Int gridCell)
    {
        if (!attachedToGrid)
            return false;

        var relativeBorderPosition = gridCell - (Vector2Int)borderBounds.min;

        if (relativeBorderPosition.x < 0 || relativeBorderPosition.x >= borderBounds.size.x)
            return false;

        if (relativeBorderPosition.y < 0 || relativeBorderPosition.y >= borderBounds.size.y)
            return false;

        return true;
    }

    /// <summary>
    /// Returns every cell that this item takes up, taking into account its structure.
    /// </summary>
    private List<Vector2Int> CalcItemGridCells(out List<ItemData.CellType> cellTypes)
    {
        var cells = new List<Vector2Int>();
        cellTypes = new List<ItemData.CellType>();

        var cellStructure = itemData.GetCellStructure();

        for (int x = 0; x < itemBounds.size.x; x++)
        {
            for (int y = 0; y < itemBounds.size.y; y++)
            {
                var rotateXY = RotateCell(new Vector2Int(x, y), true);
                var cellType = cellStructure[rotateXY.x][rotateXY.y];
                if (cellType != ItemData.CellType.Empty)
                {
                    cells.Add((Vector2Int)itemBounds.position + new Vector2Int(x, y));
                    cellTypes.Add(cellType);
                }
            }
        }

        return cells;
    }

    /// <summary>
    /// Returns every cell that this item's border (and contents) takes up. This does not take into
    /// account its structure.
    /// </summary>
    private List<Vector2Int> GetBorderGridCells()
    {
        var cells = new List<Vector2Int>(borderBounds.size.x * borderBounds.size.y);

        for (int x = 0; x < borderBounds.size.x; x++)
        {
            for (int y = 0; y < borderBounds.size.y; y++)
                cells.Add((Vector2Int)borderBounds.position + new Vector2Int(x, y));
        }

        return cells;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!itemData)
            return;

        var cellSize = game.Grid.CellSize;
        var halfCellSize = cellSize / 2f;

        var cubeSize = cellSize * Vector3.one;
        if (itemData.IsMunchie || itemData.IsCoin)
            cubeSize *= 0.5f;

        // if (borderGridCells != null)
        // {
        //     foreach (var borderCell in borderGridCells)
        //     {
        //         var relativeBorderCell = borderCell - (Vector2Int)borderBounds.position;
        //         var cellPosition = transform.position + new Vector3(relativeBorderCell.x * cellSize, relativeBorderCell.y * cellSize);
        //         Gizmos.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        //         Gizmos.DrawCube(cellPosition + new Vector3(halfCellSize, halfCellSize), cubeSize);
        //     }
        // }

        for (int i = 0; i < itemGridCells.Count; i++)
        {
            var itemCell = itemGridCells[i];

            if (game.Snake.ContainsCell(itemCell) && attachedToGrid)
                continue;

            var relativeItemCell = itemCell - (Vector2Int)borderBounds.position;
            var cellPosition = transform.position + new Vector3(relativeItemCell.x * cellSize, relativeItemCell.y * cellSize);
            Gizmos.color = itemData.debugColor;
            Gizmos.DrawCube(cellPosition + new Vector3(halfCellSize, halfCellSize), cubeSize);

            Gizmos.color = Color.black;

            // TODO: NEED TO ROTATE THIS INTO COORDINATE SYSTEM
            switch (itemGridCellTypes[i])
            {
                case ItemData.CellType.Middle:
                    Gizmos.DrawLine(cellPosition, cellPosition + cellSize * Vector3.one);
                    break;
                case ItemData.CellType.LeftEntry:
                    DrawArrow(cellPosition + halfCellSize * Vector3.one, -90f, halfCellSize);
                    break;
                case ItemData.CellType.RightEntry:
                    DrawArrow(cellPosition + halfCellSize * Vector3.one, 90, halfCellSize);
                    break;
                case ItemData.CellType.UpEntry:
                    DrawArrow(cellPosition + halfCellSize * Vector3.one, 180, halfCellSize);
                    break;
                case ItemData.CellType.DownEntry:
                    DrawArrow(cellPosition + halfCellSize * Vector3.one, 0, halfCellSize);
                    break;
            }
        }
    }

    /// <summary>
    /// Draws a 2D arrow on the XY plane, centered at pos, rotated around Z by angleDeg.
    /// CHAT GPT WROTE THIS, DELETE BEFORE SUBMISSION
    /// </summary>
    public static void DrawArrow(Vector3 pos, float angleDeg, float length)
    {
        float half = length * 0.5f;

        // Forward direction in XY plane
        Vector3 dir = Quaternion.Euler(0, 0, angleDeg) * Vector3.up;

        // Shaft endpoints
        Vector3 start = pos - dir * half;
        Vector3 end = pos + dir * half;

        Gizmos.DrawLine(start, end);

        // Arrowhead
        float headSize = length * 0.5f;
        Vector3 right = Quaternion.Euler(0, 0, 150) * dir;
        Vector3 left = Quaternion.Euler(0, 0, -150) * dir;

        Gizmos.DrawLine(end, end + right * headSize);
        Gizmos.DrawLine(end, end + left * headSize);
    }

#endif
}
