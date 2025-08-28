using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    [SerializeField]
    private SnakePart partPrefab;

    [SerializeField]
    private SnakeRenderer itemRendererPrefab;

    [SerializeField]
    private SnakeRenderer snakeRenderer;

    [Header("Eye rendering")]
    [SerializeField]
    private GameObject eyes;

    [SerializeField]
    private SnakeEye leftEye;

    [SerializeField]
    private SnakeEye rightEye;

    private struct CarryingItem
    {
        public ItemData ItemData;
        public SnakeRenderer Renderer;
        public int SnakePartIndex;
    }

    public Vector2Int Head => ToVector2Int(parts[0].transform.localPosition);
    public int Length => parts.Count;

    private Game game;
    private Vector2Int dir = Vector2Int.right;
    private List<SnakePart> parts;
    private Vector2Int behindTailCell;
    private int targetParts = 0;
    private Vector2Int newDirOnNextMove = Vector2Int.zero;
    private Vector2Int queuedDir = Vector2Int.zero;
    private List<CarryingItem> carryingItems = new();
    private ItemController insideItem = null;

    private void Awake()
    {
        game = FindObjectOfType<Game>();
        parts = new(GetComponentsInChildren<SnakePart>());
        Debug.Assert(parts.Count == 1);
        targetParts = game.startNumParts;

        // allLineRenderers = new List<SnakeLineRenderer> { borderRenderer, fillRenderer, itemCarryFillRenderer };
    }

    private void OnDestroy()
    {
        foreach (var part in parts)
            Destroy(part.gameObject);
    }

    public void SetSize(float size)
    {
        this.transform.localScale = new Vector3(size, size);
    }

    public void Init(Vector2Int pos)
    {
        parts[0].transform.localPosition = new Vector3(pos.x, pos.y);
        snakeRenderer.Init(game.Grid, pos, pos);

        while (parts.Count < targetParts)
        {
            if (!Move())
            {
                // the snake was so big there is nowhere for it to spawn.
                // uhh, at least don't freeze.
                break;
            }
        }
    }

    public void SetMoveOffset(float offset)
    {
        snakeRenderer.SetRenderOffset(offset);

        foreach (var carryingItem in carryingItems)
            carryingItem.Renderer.SetRenderOffset(offset);
    }

    public bool ContainsCell(Vector2Int cell, out SnakePart containsPart)
    {
        foreach (var part in parts)
        {
            if (cell == ToVector2Int(part.transform.localPosition))
            {
                containsPart = part;
                return true;
            }
        }
        containsPart = null;
        return false;
    }

    public Vector2Int NextMoveCell()
    {
        if (newDirOnNextMove == Vector2Int.zero)
            return Head + dir;

        return Head + newDirOnNextMove;
    }

    public bool CanConsumeOrCollect(ItemController item)
    {
        if (item.RItemData.IsConsumable)
            return Head == item.ItemGridCells[0];

        if (!game.canCarryMultipleItems && carryingItems.Count > 0)
            return false;

        if (game.canCarryMultipleItems)
        {
            if (CarryingItemsCellCount() + item.RItemData.CellCount > parts.Count)
                return false;
        }

        if (parts.Count < item.RItemData.CellCount)
            return false;

        // can only consume if all squares of the item are filled.
        for (int i = 0; i < item.ItemGridCells.Count; i++)
        {
            if (!ContainsCell(item.ItemGridCells[i], out var _))
                return false;
        }

        if (!game.canExitAtAnyCell)
        {
            // can only consume if it's an exit square, otherwise snake must have filled item but be
            // somewhere in the middle of it. (unless canExitAtAnyCell flag is set).
            game.ItemsManager.GetItemAtCell(Head, out var cellType);

            if (!ItemData.IsAnyExit(cellType))
                return false;
        }

        return true;
    }

    public void QueueDirection(Vector2Int dir)
    {
        if (newDirOnNextMove == Vector2Int.zero)
        {
            newDirOnNextMove = dir;
            return;
        }

        if (Mathf.Abs(newDirOnNextMove.x) == 0 && Mathf.Abs(dir.x) == 0)
        {
            // New move is in the same axis as the old move. This move replaces it.
            newDirOnNextMove = dir;
            queuedDir = Vector2Int.zero;
        }
        else
        {
            // New move is in a different axis. Queue it up to run after the current move.
            queuedDir = dir;
        }
    }

    private bool CanEnterFromDirection(ItemData.CellType cellType, Vector2Int direction)
    {
        if (game.canEnterAtAnyCell)
            return true;

        switch (cellType)
        {
            case ItemData.CellType.EntryOrExit:
                return true;
            case ItemData.CellType.LeftEntry:
                return direction == Vector2Int.right;
            case ItemData.CellType.RightEntry:
                return direction == Vector2Int.left;
            case ItemData.CellType.UpEntry:
                return direction == Vector2Int.down;
            case ItemData.CellType.DownEntry:
                return direction == Vector2Int.up;
        }
        return false;
    }

    private bool CanExit(ItemData.CellType cellType)
    {
        return game.canExitAtAnyCell || ItemData.IsAnyExit(cellType);
    }

    public bool Move()
    {
        if (newDirOnNextMove != Vector2Int.zero)
        {
            if (dir != -newDirOnNextMove)
                dir = newDirOnNextMove;
            newDirOnNextMove = queuedDir;
            queuedDir = Vector2Int.zero;
        }

        var newPos = Head + dir;

        if (!game.Grid.InGrid(newPos.x, newPos.y))
            return false;

        if (ContainsCell(newPos, out var containsPart) && containsPart != parts[^1])
            return false;

        var moveInsideItem = game.ItemsManager.GetItemAtCell(newPos, out var moveInsideCellType);

        if (moveInsideItem && moveInsideItem.RItemData.IsConsumable)
        {
            // The snake doesn't move inside apples, coins etc, it will immediately eat them later.
            moveInsideItem = null;
        }

        if (moveInsideItem && moveInsideItem.RItemData.IsObstacle)
            return false;

        if (insideItem == null && moveInsideItem != null)
        {
            // Moving from outside => inside an item.
            if (!CanEnterFromDirection(moveInsideCellType, dir))
                return false;
            if (!game.canCarryMultipleItems && carryingItems.Count > 0)
                return false;
            insideItem = moveInsideItem;
            CameraController.Instance.Focus(game.Grid.GetWorldPos(newPos));
        }
        else if (insideItem != null && moveInsideItem == null)
        {
            // Moving from inside item => outside, but only if the item is incomplete. If the item
            // was completed then the snake would be holding it and insideItem would be null.
            if (game.mustCompleteItemAfterEntering)
                return false;
            var itemAtCell = game.ItemsManager.GetItemAtCell(Head, out var currentyInsideCellType);
            Debug.Assert(itemAtCell == insideItem);
            if (!CanExit(currentyInsideCellType))
                return false;
            insideItem = null;
        }
        else if (insideItem != null && moveInsideItem != null && insideItem != moveInsideItem)
        {
            // Moving between items, err, this would be pretty rare but handle it anyway?
            if (game.mustCompleteItemAfterEntering)
                return false;
            var itemAtCell = game.ItemsManager.GetItemAtCell(Head, out var currentyInsideCellType);
            Debug.Assert(itemAtCell == insideItem);
            if (!CanEnterFromDirection(moveInsideCellType, dir) || !CanExit(currentyInsideCellType))
                return false;
            insideItem = moveInsideItem;
        }

        // From here on, move must have been successful (i.e. cannot return false from here).

        var tailPosition = parts[^1].transform.position;
        behindTailCell = game.Grid.GetCell(tailPosition);

        for (int i = parts.Count - 1; i > 0; i--)
        {
            parts[i].transform.localPosition = parts[i - 1].transform.localPosition;
        }

        parts[0].transform.localPosition += FromVector2Int(dir);
        snakeRenderer.MoveForward(newPos);

        foreach (var carryingItem in carryingItems)
        {
            var nextIndex = carryingItem.SnakePartIndex - carryingItem.ItemData.CellCount + 1;
            carryingItem.Renderer.MoveForward(GetPartCellPos(nextIndex));
        }

        if (targetParts > parts.Count)
        {
            var part = GameObject.Instantiate(partPrefab, transform);
            part.transform.position = tailPosition;
            parts.Add(part);
            snakeRenderer.AddTail(game.Grid.GetCell(part.transform.position));
        }

        if (Head == game.CurrentLevelSpawn && carryingItems.Count > 0)
        {
            // Reached goal while carrying an item.
            var carryingItemsData = new List<ItemData>();

            foreach (var carryingItem in carryingItems)
            {
                carryingItemsData.Add(carryingItem.ItemData);
                GameObject.Destroy(carryingItem.Renderer.gameObject);
            }

            carryingItems.Clear();

            game.OnItemsSold(carryingItemsData);

            foreach (var snakePart in parts)
                snakePart.ResetColor();
        }

        return true;
    }

    private int CarryingItemsCellCount()
    {
        int allCellCount = 0;
        foreach (var carryingItem in carryingItems)
        {
            allCellCount += carryingItem.ItemData.CellCount;
        }
        return allCellCount;
    }

    private static Vector2Int ToVector2Int(Vector3 v)
    {
        return new Vector2Int((int)Mathf.Round(v.x), (int)Mathf.Round(v.y));
    }

    private static Vector3 FromVector2Int(Vector2Int v)
    {
        return new Vector3(v.x, v.y, 0);
    }

    public void ConsumeOrCollect(ItemController item)
    {
        Debug.Assert(insideItem == null || item == insideItem);
        var itemData = item.RItemData;

        if (itemData.IsApple)
        {
            targetParts++;
        }
        else if (itemData.IsMushroom)
        {
            if (parts.Count <= 2)
            {
                game.Die();
                return;
            }
            GameObject.Destroy(parts[^1].gameObject);
            parts.RemoveAt(parts.Count - 1);
            targetParts--;
        }
        else if (itemData.IsCoin)
        {
            game.AddCoin();
        }

        insideItem = null;
    }

    public void CarryItem(ItemData itemData)
    {
        int currentCarryingCellCount = CarryingItemsCellCount();
        Debug.Assert(currentCarryingCellCount + itemData.CellCount <= parts.Count);

        // Generate a new renderer after the next item. The renderer must be generated backwards to
        // forwards (i.e. same as the snake) so that they can be moved forward.
        var startIndex = CarryingItemsCellCount() + itemData.CellCount - 1;

        // Start rendering from the cell *before* the index so that the back of it stretches to the
        // previous tail, i.e. that "render offset" stuff.
        // At the end, we'll move the item forward into its correct place.
        var startCell = GetPartCellPos(startIndex + 1);

        Vector2Int behindStartCell;
        if (startIndex + 2 < parts.Count)
            behindStartCell = GetPartCellPos(startIndex + 2);
        else
            behindStartCell = snakeRenderer.BehindExtraTailCell;

        var carryingItem = new CarryingItem
        {
            ItemData = itemData,
            SnakePartIndex = startIndex,
            Renderer = GameObject.Instantiate(itemRendererPrefab, transform),
        };

        carryingItem.Renderer.Init(game.Grid, startCell, behindStartCell);
        carryingItem.Renderer.SetRenderOffset(snakeRenderer.RenderOffset);

        for (int i = 0; i < itemData.CellCount; i++)
        {
            var nextIndex = startIndex - i - 1;
            var nextCell = GetPartCellPos(nextIndex + 1);
            carryingItem.Renderer.AddTail(startCell);
            carryingItem.Renderer.MoveForward(nextCell);
        }

        carryingItems.Add(carryingItem);
    }

    private Vector2Int GetPartCellPos(int partIndex)
    {
        if (partIndex == -1)
            return Head + dir;
        if (partIndex == parts.Count)
            return behindTailCell;
        return game.Grid.GetCell(parts[partIndex].transform.position);
    }

    private void UpdateEyePosition()
    {
        var headPosition = snakeRenderer.GetHeadPosition(out var headDirection);

        if (headDirection != Vector3.zero && headPosition != eyes.transform.position)
        {
            eyes.transform.position = headPosition;
            eyes.transform.rotation = Quaternion.LookRotation(Vector3.forward, headDirection.normalized);
        }

        var nextMovePosition = game.Grid.GetWorldPos(NextMoveCell()) + game.Grid.CellCenterOffset();
        leftEye.SetLookDirection(nextMovePosition - headPosition);
        rightEye.SetLookDirection(nextMovePosition - headPosition);
    }

    private void Update()
    {
        UpdateEyePosition();
    }
}
