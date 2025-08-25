using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    [SerializeField]
    private SnakePart partPrefab;

    [Header("Body rendering")]
    [SerializeField, Range(0.5f, 1)]
    private float snakeWidth = 0.8f;

    [SerializeField, Range(0f, 0.5f)]
    private float borderWidth = 0.2f;

    [SerializeField]
    private SnakeLineRenderer borderRenderer;

    [SerializeField]
    private SnakeLineRenderer fillRenderer;

    [Header("Eye rendering")]
    [SerializeField]
    private GameObject eyes;

    [SerializeField]
    private SnakeEye leftEye;

    [SerializeField]
    private SnakeEye rightEye;

    public Vector2Int Head => ToVector2Int(parts[0].transform.localPosition);
    public int Length => parts.Count;

    private Game game;
    private Vector2Int dir = Vector2Int.right;
    private List<SnakePart> parts;
    private int targetParts = 0;
    private Vector2Int newDirOnNextMove = Vector2Int.zero;
    private Vector2Int queuedDir = Vector2Int.zero;
    private List<ItemData> carryingItems = new();
    private ItemController insideItem = null;
    private List<SnakeLineRenderer> lineRenderers;

    private void Awake()
    {
        game = FindObjectOfType<Game>();
        parts = new(GetComponentsInChildren<SnakePart>());
        Debug.Assert(parts.Count == 1);
        targetParts = game.startNumParts;

        // Rendering
        borderRenderer.SetWidth(snakeWidth);
        fillRenderer.SetWidth(snakeWidth - borderWidth);
        lineRenderers = new List<SnakeLineRenderer> { borderRenderer, fillRenderer };
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

        foreach (var lineRenderer in lineRenderers)
            lineRenderer.Init(game.Grid, pos, 0);

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
        foreach (var lineRenderer in lineRenderers)
            lineRenderer.SetRenderOffset(offset);
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

        if (game.mustHaveExactLengthToCollectItem && parts.Count != item.RItemData.CellCount)
            return false;

        if (parts.Count < item.RItemData.CellCount)
            return false;

        // can only consume if all squares of the item are filled.
        for (int i = 0; i < item.ItemGridCells.Count; i++)
        {
            if (!ContainsCell(item.ItemGridCells[i], out var _))
                return false;
        }

        // can only consume if it's an exit square, otherwise snake must have filled item but be
        // somewhere in the middle of it. (unless canExitAtAnyCell flag is set).
        game.ItemsManager.GetItemAtCell(Head, out var cellType);

        if (!game.canExitAtAnyCell && !ItemData.IsAnyExit(cellType))
            return false;

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

    private static bool CanEnterFromDirection(ItemData.CellType cellType, Vector2Int direction)
    {
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

        if (insideItem == null && moveInsideItem != null)
        {
            // Moving from outside => inside an item.
            if (!CanEnterFromDirection(moveInsideCellType, dir))
                return false;
            if (!game.canCarryMultipleItems && carryingItems.Count > 0)
                return false;
            insideItem = moveInsideItem;
        }
        else if (insideItem != null && moveInsideItem == null)
        {
            // Moving from inside item => outside, but only if the item is incomplete. If the item
            // was completed then the snake would be holding it and insideItem would be null.
            var itemAtCell = game.ItemsManager.GetItemAtCell(Head, out var currentyInsideCellType);
            Debug.Assert(itemAtCell == insideItem);
            if (!CanExit(currentyInsideCellType))
                return false;
            insideItem = null;
        }
        else if (insideItem != null && moveInsideItem != null && insideItem != moveInsideItem)
        {
            var itemAtCell = game.ItemsManager.GetItemAtCell(Head, out var currentyInsideCellType);
            Debug.Assert(itemAtCell == insideItem);
            // Moving between items, err, this would be pretty rare but handle it anyway?
            if (!CanEnterFromDirection(moveInsideCellType, dir) || !CanExit(currentyInsideCellType))
                return false;
            insideItem = moveInsideItem;
        }

        // From here on, move must have been successful (i.e. cannot return false from here).

        var tailPosition = parts[^1].transform.localPosition;

        for (int i = parts.Count - 1; i > 0; i--)
        {
            parts[i].transform.localPosition = parts[i - 1].transform.localPosition;
        }

        parts[0].transform.localPosition += FromVector2Int(dir);

        foreach (var lineRenderer in lineRenderers)
            lineRenderer.MoveForward(newPos);

        if (targetParts > parts.Count)
        {
            var part = GameObject.Instantiate(partPrefab, transform);
            part.transform.localPosition = tailPosition;
            parts.Add(part);

            foreach (var lineRenderer in lineRenderers)
                lineRenderer.AddTail(game.Grid.GetCellPos(part.transform.position));
        }

        if (Head == Vector2Int.zero && carryingItems.Count > 0)
        {
            // Reached goal while carrying an item.
            game.OnItemsSold(carryingItems);
            foreach (var snakePart in parts)
                snakePart.ResetColor();

            foreach (var lr in lineRenderers)
                lr.SetCarryItemPercent(0);
            carryingItems.Clear();
        }

        return true;
    }

    private int CarryingItemsCellCount()
    {
        int allCellCount = 0;
        foreach (var carryingItem in carryingItems)
        {
            allCellCount += carryingItem.CellCount;
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

        carryingItems.Add(itemData);

        for (int i = currentCarryingCellCount; i < parts.Count && i < currentCarryingCellCount + itemData.CellCount; i++)
            parts[i].SetColor(itemData.debugColor);

        foreach (var lr in lineRenderers)
            lr.SetCarryItemPercent((float)CarryingItemsCellCount() / (float)parts.Count);
    }

    private void UpdateEyePosition()
    {
        var headPosition = borderRenderer.GetHeadPosition(out var headDirection);
        eyes.transform.position = headPosition;
        eyes.transform.rotation = Quaternion.LookRotation(Vector3.forward, headDirection.normalized);

        var nextMovePosition = game.Grid.GetWorldPos(NextMoveCell()) + game.Grid.CellCenterOffset();
        leftEye.SetLookDirection(nextMovePosition - headPosition);
        rightEye.SetLookDirection(nextMovePosition - headPosition);
    }

    private void Update()
    {
        UpdateEyePosition();
    }
}
