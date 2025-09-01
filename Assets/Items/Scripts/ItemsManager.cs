using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    public void LoadLevel(Game game)
    {
        this.game = game;
        DespawnItems();
        SpawnObstacles();
        SpawnCollectibles();
        SpawnMunchies();
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
        foreach (var obstacleCell in game.CurrentLevel.Obstacles)
        {
            var obstacleItem = GameObject.Instantiate(itemControllerPrefab, transform);
            var bounds = new BoundsInt((Vector3Int)obstacleCell, new Vector3Int(1, 1));
            obstacleItem.name = "Item.Obstacle";
            obstacleItem.transform.position = game.Grid.GetWorldPos(obstacleCell);
            obstacleItem.SetData(new RotatedItemData(obstacleItemData, ItemRotation.Up));
            obstacleItem.SetGridLocation(bounds, bounds);
            items.Add(obstacleItem);
        }

        for (int i = 0; i < game.CurrentLevel.NumRandomObstacles; i++)
        {
            var randomObstacleItemData = obstacleItemData;
            if (game.CurrentLevel.RandomObstacles && game.CurrentLevel.RandomObstacles.Items.Count > 0)
                randomObstacleItemData = ListUtil.Random(game.CurrentLevel.RandomObstacles.Items);
            SpawnItem(randomObstacleItemData);
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
            SpawnItem(coinItemData);
    }

    public void SpawnCollectibles()
    {
        var shuffledItems = new List<ItemData>(game.CurrentLevel.Items.Items);
        ListUtil.Shuffle(shuffledItems);

        foreach (var itemData in shuffledItems)
        {
            if (!itemData.IsCollectible)
                continue;

            var spawnItem = itemData;

            foreach (var item in items)
            {
                if (item.RItemData.ItemData == itemData)
                {
                    Debug.Log($"already spawned: {itemData.name}");
                    spawnItem = null;
                    break;
                }
            }

            if (spawnItem != null)
            {
                Debug.Log($"spawning: {spawnItem.name}");
                SpawnItem(spawnItem);
            }

            if (NumCollectibleItems() >= game.CurrentLevel.NumItems)
                break;
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

    private bool SpawnItem(ItemData originalItemData)
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
        var candidateCell = game.Grid.RandomSpawnCell(itemData.Width, itemData.Height);

        var itemBounds = new BoundsInt((Vector3Int)candidateCell, new Vector3Int(itemData.Width, itemData.Height));
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

        while (!CanSpawnItemInCell((Vector2Int)borderBounds.position, borderBounds.size.x, borderBounds.size.y))
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
                Debug.LogWarning($"Can't find anywhere to spawn item: {itemData}");
                return false;
            }

            if (dontInfiniteLoop++ > 10000)
            {
                Debug.LogWarning($"BUG BUG BUG: {itemData} never wrapped around to {startCell}");
                return false;
            }
        }

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
    /// </summary>
    private bool CanSpawnItemInCell(Vector2Int gridCell, int itemWidth, int itemHeight)
    {
        for (int x = 0; x < itemWidth; x++)
        {
            for (int y = 0; y < itemHeight; y++)
            {
                var checkCell = gridCell + new Vector2Int(x, y);

                // // don't spawn in the same row as the spawn point
                // if (
                //     checkCell.x == game.CurrentLevelSpawn.x /* || checkCell.y == game.CurrentLevelSpawn.y */
                // )
                //     return false;

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
                    if (item.ItemBorderContainsCell(checkCell))
                        return false;
                }

                if (game.Snake.ContainsCell(checkCell, out var _))
                    return false;

                if (checkCell == game.Snake.NextMoveCell())
                    return false;
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

            if (game.Snake.CanConsumeOrCollect(item))
            {
                game.Snake.ConsumeOrCollect(item);
                didConsume = itemData.ItemData;
                DestroyImmediate(item.gameObject);
                items[i] = items[^1];
                items.RemoveAt(items.Count - 1);
                if (!itemData.IsConsumable)
                {
                    CameraController.Instance.ClearFocus(game.focusItem);
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
                SpawnItem(didConsume);

            SpawnCollectibles();
        }
    }

    public void SpawnMunchies()
    {
        SpawnItem(appleItemData);
        if (game.CurrentLevel.Mushrooms)
            SpawnItem(mushroomItemData);
    }

    private void DespawnMunchies()
    {
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (item.RItemData.IsApple || item.RItemData.IsMushroom)
            {
                DestroyImmediate(item.gameObject);
                items[i] = items[^1];
                items.RemoveAt(items.Count - 1);
                i--;
            }
        }
    }
}
