using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ItemController : MonoBehaviour
{
    [Serializable]
    public struct ItemDataPrefab
    {
        public ItemData ItemData;
        public GameObject Prefab;
    }

    [SerializeField]
    private List<ItemDataPrefab> itemDataPrefabs = new();

    public RotatedItemData RItemData => itemData;

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

    private SpriteRenderer spriteRenderer;
    private Shaker shaker;
    private RotatedItemData itemData;
    private Game game;
    private bool attachedToGrid = false;
    private BoundsInt itemBounds;
    private BoundsInt borderBounds;
    private List<Vector2Int> itemGridCells;
    private List<ItemData.CellType> itemGridCellTypes;
    private List<Vector2Int> borderGridCells;
    private GameObject instantiatedPrefab;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        shaker = GetComponentInChildren<Shaker>();
        game = FindObjectOfType<Game>();
    }

    public Coroutine Shake()
    {
        return shaker.Shake();
    }

    public void SetData(RotatedItemData itemData)
    {
        this.itemData = itemData;

        if (itemData.Sprite)
        {
            spriteRenderer.sprite = itemData.Sprite;

            var spriteOffset = itemData.Sprite.bounds.size / 2;

            if (itemData.Rotation == ItemRotation.Right || itemData.Rotation == ItemRotation.Left)
                (spriteOffset.x, spriteOffset.y) = (spriteOffset.y, spriteOffset.x);

            spriteRenderer.transform.localPosition = spriteOffset * itemData.SpriteScale;
            spriteRenderer.transform.localRotation = itemData.RotationQuaternion();
            spriteRenderer.transform.localScale = Vector3.one * itemData.SpriteScale;

            foreach (var itemDataPrefab in itemDataPrefabs)
            {
                if (itemDataPrefab.ItemData == itemData.ItemData)
                {
                    Debug.Log($"Switching sprite renderer out on {itemData.Name}");
                    instantiatedPrefab = GameObject.Instantiate(itemDataPrefab.Prefab, spriteRenderer.transform);
                    break;
                }
            }
        }
    }

    public void SetGridLocation(BoundsInt itemBounds, BoundsInt borderBounds)
    {
        this.itemBounds = itemBounds;
        this.borderBounds = borderBounds;
        attachedToGrid = true;
        itemGridCells = CalcItemGridCells(out itemGridCellTypes);
        borderGridCells = GetBorderGridCells();

        spriteRenderer.transform.localPosition += (itemBounds.min - borderBounds.min);
        SetGridSquares(false);

        // Debug.Log(
        //     $"[{itemData.Name}] itemBounds: {itemBounds}, borderBounds: {borderBounds}, itemGridCells: {ListUtil.ListToString(itemGridCells)}, borderGridCells: {ListUtil.ListToString(borderGridCells)}",
        //     gameObject
        // );
    }

    public void ReRender()
    {
        for (int i = 0; i < itemGridCells.Count; i++)
        {
            var itemCell = itemGridCells[i];
            game.GridSquares[itemCell].Render(true);
        }
    }

    private void OnDestroy()
    {
        SetGridSquares(true);
        if (instantiatedPrefab != null)
            GameObject.Destroy(instantiatedPrefab);
    }

    public void SetFloating()
    {
        itemBounds = itemData.LocalBounds;
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

    public bool ItemRealContainsCell(Vector2Int gridCell)
    {
        if (!attachedToGrid)
            return false;

        var relativeBorderPosition = gridCell - (Vector2Int)itemBounds.min;

        if (relativeBorderPosition.x < 0 || relativeBorderPosition.x >= itemBounds.size.x)
            return false;

        if (relativeBorderPosition.y < 0 || relativeBorderPosition.y >= itemBounds.size.y)
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
                var cellType = cellStructure[x][y];
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

    private void SetGridSquares(bool clear)
    {
        for (int i = 0; i < itemGridCells.Count; i++)
        {
            var itemCell = itemGridCells[i];
            game.GridSquares[itemCell].SetItemData(clear ? null : itemData);

            // var relativeItemCell = itemCell - (Vector2Int)borderBounds.position;
            // var cellPosition = transform.position + new Vector3(relativeItemCell.x * cellSize, relativeItemCell.y * cellSize);

            // var clr = itemData.DebugColor;

            // if (!itemData.IsObstacle)
            //     clr.a = 0.5f;

            // Gizmos.color = clr;
            // Gizmos.DrawCube(cellPosition + new Vector3(halfCellSize, halfCellSize), cubeSize);
        }
    }
}
