using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsManager : MonoBehaviour
{
    [SerializeField]
    private ItemController itemControllerPrefab;

    private ItemData obstacleItemData;

    [SerializeField]
    private ItemData appleItemData;

    [SerializeField]
    private ItemData mushroomItemData;

    [SerializeField]
    private ItemData coinItemData;

    private Game game;

    private List<ItemController> items = new();
    public List<ItemController> CollectibleItems
    {
        get
        {
            List<ItemController> collectables = new();
            foreach (ItemController item in items)
            {
                if (item.RItemData.IsCollectible)
                    collectables.Add(item);
            }
            return collectables;
        }
    }

    public void LoadLevel(Game game)
    {
        this.game = game;
        DespawnItems();
        SpawnObstacles();
        SpawnCollectibles();
        SpawnMunchies();
    }

    public void ReRenderItems()
    {
        foreach (var item in items)
        {
            item.ReRender();
        }
    }

    private void DespawnItems()
    {
        foreach (var item in items)
        {
            GameObject.DestroyImmediate(item.gameObject);
        }
        items.Clear();
    }

    private void SpawnObstacles()
    {
        for (int i = 0; i < game.CurrentLevel.NumRandomObstacles; i++)
        {
            var randomObstacleItemData = obstacleItemData;
            if (game.CurrentLevel.RandomObstacles && game.CurrentLevel.RandomObstacles.Items.Count > 0)
                randomObstacleItemData = ListUtil.Random(game.CurrentLevel.RandomObstacles.Items);
            SpawnItem(randomObstacleItemData, false);
        }
    }

    private int NumCollectibleItems()
    {
        int num = 0;
        foreach (var item in items)
        {
            if (item.RItemData.IsCollectible)
                num++;
        }
        return num;
    }

    public void RespawnCoins()
    {
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (item.RItemData.IsCoin)
            {
                DestroyImmediate(item.gameObject);
                items[i] = items[^1];
                items.RemoveAt(items.Count - 1);
                i--;
            }
        }

        for (int i = 0; i < game.CurrentLevel.NumCoins; i++)
            SpawnItem(coinItemData, true);
    }

    /// <summary>
    /// Spawns as many items as possible up to the number of items for the current level.
    /// </summary>
    public void SpawnCollectibles()
    {
        bool noItems = items.Count == 0;

        foreach (var itemData in game.Items)
        {
            if (!itemData.IsCollectible)
                continue;

            var spawnItem = itemData;

            foreach (var item in items)
            {
                if (item.RItemData.ItemData == itemData)
                {
                    spawnItem = null;
                    break;
                }
            }

            if (spawnItem != null)
                SpawnItem(spawnItem, true);

            if (NumCollectibleItems() >= game.CurrentLevel.NumItems)
                break;
        }

        if (noItems)
        {
            Debug.Log("there were no items, trying to spawn the first item again");
            game.SetFirstItem();
        }
    }

    public ItemController GetRandomExistingCollectibleItem()
    {
        var itemsCopy = new List<ItemController>(items);
        ListUtil.Shuffle(itemsCopy);

        foreach (var item in itemsCopy)
        {
            if (item.RItemData.IsCollectible)
                return item;
        }

        Debug.Assert(false, "no collectible items available");
        return null;
    }

    /// <summary>
    /// "okInFrontOfSnake" should be false if they're running around with no time to react to new
    /// items (as in normal play) vs true if they do have time (spawning and selling items).
    /// </summary>
    private bool SpawnItem(ItemData originalItemData, bool okInFrontOfSnake)
    {
        var rotation = ItemRotation.Up;

        if (originalItemData.CellCount > 1)
            rotation = (ItemRotation)Random.Range(0, 4);

        var itemData = new RotatedItemData(originalItemData, rotation);

        // In order to be collectible, the item needs to have space on the entry side, and we need
        // to keep the rotation of the object in mind both for using the correct width/height and
        // for picking the side that the entry is on.
        //
        // It's OK to spawn apples and coins around the border of the grid, since those can always
        // be consumed. But don't spawn items since that can create impossible-to-solve boards.
        var itemLocalBounds = itemData.LocalBounds;
        var candidateCell = game.Grid.RandomSpawnCell(itemLocalBounds.size.x, itemLocalBounds.size.y);

        var itemBounds = new BoundsInt((Vector3Int)candidateCell + itemLocalBounds.min, itemLocalBounds.size);
        var borderBounds = new BoundsInt(itemBounds.position, itemBounds.size);

        if (itemData.HasLeftEntryOrExit && !itemData.IsConsumable)
        {
            if (borderBounds.min.x > 0)
            {
                borderBounds.min += Vector3Int.left;
            }
            else
            {
                // expanding to left would push the border out of bounds, expand and move to the right instead.
                borderBounds.max += Vector3Int.right;
                itemBounds.position += Vector3Int.right;
            }
        }

        if (itemData.HasRightEntryOrExit && !itemData.IsConsumable)
        {
            if (xExtent(borderBounds) < game.Grid.Width - 1)
            {
                borderBounds.max += Vector3Int.right;
            }
            else
            {
                // expanding to right would push the border out of bounds, expand and move to the left instead.
                borderBounds.min += Vector3Int.left;
                itemBounds.position += Vector3Int.left;
            }
        }

        if (itemData.HasUpEntryOrExit && !itemData.IsConsumable)
        {
            if (yExtent(borderBounds) < game.Grid.Height - 1)
            {
                borderBounds.max += Vector3Int.up;
            }
            else
            {
                // expanding upwards would push the border out of bounds, expand and move downwards instead.
                borderBounds.min += Vector3Int.down;
                itemBounds.position += Vector3Int.down;
            }
        }

        if (itemData.HasDownEntryOrExit && !itemData.IsConsumable)
        {
            if (borderBounds.min.y > 0)
            {
                borderBounds.min += Vector3Int.down;
            }
            else
            {
                // expanding downwards would push the border out of bounds, expand and move upwards instead.
                borderBounds.max += Vector3Int.up;
                itemBounds.position += Vector3Int.up;
            }
        }

        var startCell = borderBounds.position;
        int dontInfiniteLoop = 0;

        Debug.Log(
            $"Looking for somewhere to spawn item: {itemData} (border bounds: {borderBounds} @ {borderBounds.position}, item bounds: {itemBounds} @ {itemBounds.position})"
        );

        while (!CanSpawnItemInCell((Vector2Int)borderBounds.position, borderBounds.size.x, borderBounds.size.y, okInFrontOfSnake, itemData.IsApple))
        {
            // linearly search to find a spot that fits.
            borderBounds.position += Vector3Int.right;
            itemBounds.position += Vector3Int.right;

            if (xExtent(borderBounds) >= game.Grid.Width)
            {
                var prevBorderPosition = borderBounds.position;
                borderBounds.position = new Vector3Int(0, borderBounds.position.y + 1);

                if (yExtent(borderBounds) >= game.Grid.Height)
                    borderBounds.position = Vector3Int.zero;

                itemBounds.position += borderBounds.position - prevBorderPosition;
            }

            if (borderBounds.position == startCell)
            {
                Debug.LogWarning($"Can't find anywhere to spawn item: {itemData} (border bounds: {borderBounds}, item bounds: {itemBounds})");
                return false;
            }

            if (dontInfiniteLoop++ > 10000)
            {
                Debug.LogWarning($"BUG BUG BUG: {itemData} never wrapped around to {startCell}");
                return false;
            }
        }

        Debug.Log($"Found somewhere to spawn item: {itemData} (border bounds: {borderBounds}, item bounds: {itemBounds})");

        var item = GameObject.Instantiate(itemControllerPrefab, transform);
        item.name = $"Item.{itemData.Name}@{rotation}";
        item.transform.position = game.Grid.GetWorldPos((Vector2Int)borderBounds.min);
        item.SetData(itemData);
        item.SetGridLocation(itemBounds, borderBounds);
        items.Add(item);

        return true;
    }

    /// <summary>
    /// The maximum x position within bounds. This is similar to bounds.xMax but inclusive of the
    /// bounds not exclusive. I.e. the extent of the bounds is WITHIN the bounds.
    /// </summary>
    private int xExtent(BoundsInt bounds)
    {
        Debug.Assert(bounds.size.x > 0);
        return bounds.xMax - 1;
    }

    /// <summary>
    /// The maximum y position within bounds. This is similar to bounds.yMax but inclusive of the
    /// bounds not exclusive. I.e. the extent of the bounds is WITHIN the bounds.
    /// </summary>
    private int yExtent(BoundsInt bounds)
    {
        Debug.Assert(bounds.size.y > 0);
        return bounds.yMax - 1;
    }

    /// <summary>
    /// Rough check that gridCell is a valid spawn location for an item of a given width/height.
    /// "okInFrontOfSnake" should be false if they're running around with no time to react to new
    /// items (as in normal play) vs true if they do have time (spawning and selling items).
    /// </summary>
    private bool CanSpawnItemInCell(Vector2Int gridCell, int itemWidth, int itemHeight, bool okInFrontOfSnake, bool isApple)
    {
        for (int x = 0; x < itemWidth; x++)
        {
            for (int y = 0; y < itemHeight; y++)
            {
                var checkCell = gridCell + new Vector2Int(x, y);

                // don't spawn next to the spawn point
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (checkCell.x == game.CurrentLevelSpawn.x + dx && checkCell.y == game.CurrentLevelSpawn.y + dy)
                            return false;
                    }
                }

                foreach (var item in items)
                {
                    if (!isApple && item.ItemBorderContainsCell(checkCell))
                        return false;
                    else if (isApple && item.ItemRealContainsCell(checkCell))
                        return false;
                }

                if (game.Snake.ContainsCell(checkCell, out var _))
                    return false;

                if (!okInFrontOfSnake)
                {
                    var dir = game.Snake.Dir;
                    if (dir.x == 1 && dir.y == checkCell.y && game.Snake.Head.x < checkCell.x)
                        return false;
                    else if (dir.x == -1 && dir.y == checkCell.y && game.Snake.Head.x > checkCell.x)
                        return false;
                    else if (dir.y == 1 && dir.x == checkCell.x && game.Snake.Head.y < checkCell.y)
                        return false;
                    else if (dir.y == -1 && dir.x == checkCell.x && game.Snake.Head.y > checkCell.y)
                        return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Finds the item at a grid cell, and returns the cell type at that grid cell if one is found.
    /// Checks the internal structure of the cell, e.g. the middle of a donut will return null.
    /// </summary>
    public ItemController GetItemAtCell(Vector2Int gridCell, out ItemData.CellType cellType)
    {
        foreach (var item in items)
        {
            if (item.ItemContainsCell(gridCell, out cellType))
                return item;
        }

        cellType = ItemData.CellType.Empty;
        return null;
    }

    public void SnakeMoved()
    {
        ItemData didConsume = null;

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var itemData = item.RItemData;
            if (game.Snake.CanConsumeOrCollect(item, out game.whyLastItemNotCollected))
            {
                game.Snake.ConsumeOrCollect(item);
                didConsume = itemData.ItemData;
                DestroyImmediate(item.gameObject);
                items[i] = items[^1];
                items.RemoveAt(items.Count - 1);
                game.ConsumeItem(didConsume);
                if (!itemData.IsConsumable)
                {
                    CameraController.Instance.ClearFocus(game.focusItem);
                    CameraController.Instance.LittleShake();
                    foreach (var gridSq in game.GridSquares.Values)
                        gridSq.SetInvertItemColor(false);
                }
                break; // can only consume 1 item
            }
        }

        if (didConsume != null)
        {
            if (didConsume.IsCollectible)
                game.Snake.CarryItem(didConsume);
            else if (didConsume.IsMunchie)
                SpawnItem(didConsume, false);

            SpawnCollectibles();
        }
    }

    public void SpawnMunchies()
    {
        SpawnItem(appleItemData, false);
        if (game.CurrentLevel.Mushrooms)
            SpawnItem(mushroomItemData, false);
    }
}
