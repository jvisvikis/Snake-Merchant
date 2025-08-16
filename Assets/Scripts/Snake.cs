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

        var itemCells = item.ItemData.GetCellStructure();

        for (int x = 0; x < item.ItemData.Width; x++)
        {
            for (int y = 0; y < item.ItemData.Height; y++)
            {
                if (itemCells[x][y] != ItemData.CellType.Empty && !ContainsCell(item.Cell + new Vector2Int(x, y)))
                    return false;
            }
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

        // Already have a new direction, if this is in the same axis then override it, but if it's
        // in a different axis then queue it up. This allows nice 180 degree turns for example.
        if (newDirOnNextMove.x == 0 && dir.x == 0)
        {
            newDirOnNextMove = dir;
        }
        else
        {
            queuedDir = dir;
        }
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
            // Moving from outside item => inside item. Only allowed if is an entry.
            if (moveInsideCellType != ItemData.CellType.Entry)
                return false;
            insideItem = moveInsideItem;
        }
        else if (insideItem != null && moveInsideItem != insideItem)
        {
            // Moving from inside item => outside item, or between items. Only allowed if the
            // current square is an entry (aka exit). This "GetItemAtCell" call should return
            // "insideItem" or something weird is going on.
            //
            // Btw note will all this that this can only happen if insideItem wasn't completed,
            // because if it had, it would have been consumed.
            game.ItemsManager.GetItemAtCell(Head, out var currentyInsideCellType);
            if (currentyInsideCellType != ItemData.CellType.Entry)
                return false;
            if (moveInsideItem != null && moveInsideCellType != ItemData.CellType.Entry)
                return false;
            insideItem = null;
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
