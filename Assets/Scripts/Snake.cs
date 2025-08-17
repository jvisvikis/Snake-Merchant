using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    [SerializeField]
    private SnakePart partPrefab;

    public Vector2Int Head => ToVector2Int(parts[0].transform.localPosition);
    public int Length => parts.Count;

    private Game game;
    private Vector2Int dir = Vector2Int.right;
    private List<SnakePart> parts;
    private int targetParts = 0;
    private Vector2Int newDirOnNextMove = Vector2Int.zero;
    private Vector2Int queuedDir = Vector2Int.zero;
    private ItemData carryingItem = null;
    private ItemController insideItem = null;

    private void Awake()
    {
        game = FindObjectOfType<Game>();
        parts = new(GetComponentsInChildren<SnakePart>());
        Debug.Assert(parts.Count == 1);
        targetParts = game.startNumParts;
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

        while (parts.Count < targetParts)
            Move();
    }

    public bool ContainsCell(Vector2Int cell)
    {
        foreach (var part in parts)
        {
            if (cell == ToVector2Int(part.transform.localPosition))
                return true;
        }
        return false;
    }

    public bool CanConsume(ItemController item)
    {
        if (carryingItem && !item.ItemData.IsCoin)
            return false;

        if (game.mustHaveExactLengthToCollectItem && parts.Count != item.ItemData.CellCount)
            return false;

        if (parts.Count < item.ItemData.CellCount)
            return false;

        for (int i = 0; i < item.ItemGridCells.Count; i++)
        {
            if (!ContainsCell(item.ItemGridCells[i]))
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

    private static bool CanExit(ItemData.CellType cellType)
    {
        return cellType == ItemData.CellType.EntryOrExit || cellType == ItemData.CellType.Exit;
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

        if (ContainsCell(newPos))
            return false;

        var moveInsideItem = game.ItemsManager.GetItemAtCell(newPos, out var moveInsideCellType);

        if (insideItem == null && moveInsideItem != null)
        {
            // Moving from outside => inside an item.
            if (!CanEnterFromDirection(moveInsideCellType, dir))
                return false;
            if (carryingItem != null)
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

        if (targetParts > parts.Count)
        {
            var part = GameObject.Instantiate(partPrefab, transform);
            part.transform.localPosition = tailPosition;
            parts.Add(part);
        }

        if (Head == Vector2Int.zero && carryingItem != null)
        {
            // Reached goal while carrying an item.
            game.OnItemCollected();
            foreach (var snakePart in parts)
                snakePart.ResetColor();
            if (game.snakeGetsSmallerOnDelivery)
                SetNumParts(Mathf.Max(game.startNumParts, parts.Count - carryingItem.CellCount));
            carryingItem = null;
            game.ItemsManager.SpawnRandomNonExistentCollectibleItem();
            game.ItemsManager.SpawnMunchies(); // they were despawned when the item was picked up
        }

        return true;
    }

    private void SetNumParts(int numParts)
    {
        targetParts = numParts;

        for (int i = parts.Count - 1; i >= numParts; i--)
        {
            Destroy(parts[i].gameObject);
            parts.RemoveAt(i);
        }
    }

    private static Vector2Int ToVector2Int(Vector3 v)
    {
        return new Vector2Int((int)Mathf.Round(v.x), (int)Mathf.Round(v.y));
    }

    private static Vector3 FromVector2Int(Vector2Int v)
    {
        return new Vector3(v.x, v.y, 0);
    }

    public void Consume(ItemController item)
    {
        Debug.Assert(insideItem == null || item == insideItem);
        var itemData = item.ItemData;

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
        carryingItem = itemData;

        foreach (var snakePart in parts)
            snakePart.SetColor(itemData.debugColor);
    }
}
