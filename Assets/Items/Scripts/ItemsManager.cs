using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsManager : MonoBehaviour
{
    [SerializeField]
    private ItemDataCollection allItemData;

    [SerializeField]
    private ItemController itemControllerPrefab;

    private Game game;
    private ItemData appleItemData;
    private ItemData mushroomItemData;
    private ItemData coinItemData;
    private List<ItemData> collectibleItemData = new();

    private List<ItemController> items = new();

    private void Awake()
    {
        game = GetComponent<Game>();

        if (allItemData != null)
        {
            foreach (var itemData in allItemData.Items)
            {
                if (itemData.IsApple)
                    appleItemData = itemData;
                else if (itemData.IsMushroom)
                    mushroomItemData = itemData;
                else if (itemData.IsCoin)
                    coinItemData = itemData;
                else
                    collectibleItemData.Add(itemData);
            }
        }
    }

    private void Start()
    {
        SpawnMunchies();

        // weird copy/shuffle logic so that we (a) spawn random items and (b) don't spawn the same
        // item more than once.
        var ncid = new List<ItemData>(collectibleItemData);
        ListUtil.Shuffle(ncid);

        int numSpawned = 0;

        for (int i = 0; i < ncid.Count; i++)
        {
            if (SpawnItem(ncid[i]))
                numSpawned++;
            if (numSpawned >= game.numItems)
                break;
        }
    }

    public void RespawnCoins()
    {
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (item.RItemData.IsCoin)
            {
                Destroy(item.gameObject);
                items[i] = items[^1];
                items.RemoveAt(items.Count - 1);
                i--;
            }
        }

        for (int i = 0; i < game.numCoins; i++)
            SpawnItem(coinItemData);
    }

    public void SpawnRandomNonExistentCollectibleItem()
    {
        var ncid = new List<ItemData>(collectibleItemData);
        ListUtil.Shuffle(ncid);

        foreach (var itemData in ncid)
        {
            var spawnItem = itemData;

            foreach (var item in items)
            {
                if (item.RItemData.ItemData == itemData)
                {
                    spawnItem = null;
                    break;
                }
            }

            if (spawnItem != null && SpawnItem(spawnItem))
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

        Debug.Assert(false);
        return null;
    }

    private bool SpawnItem(ItemData originalItemData)
    {
        var rotation = (ItemRotation)Random.Range(0, 4);
        var itemData = new RotatedItemData(originalItemData, rotation);

        // In order to be collectible, the item needs to have space on the entry side, and we need
        // to keep the rotation of the object in mind both for using the correct width/height and
        // for picking the side that the entry is on.
        //
        // It's OK to spawn apples and coins around the border of the grid, since those can always
        // be consumed. But don't spawn items since that can create impossible-to-solve boards.
        var spawnBorderOk = itemData.IsApple || itemData.IsCoin;
        var spawnBorder = spawnBorderOk ? 0 : 1;
        var candidateCell = game.Grid.RandomSpawnCell(itemData.Width, itemData.Height, spawnBorderOk);

        var itemBounds = new BoundsInt((Vector3Int)candidateCell, new Vector3Int(itemData.Width, itemData.Height));
        var borderBounds = new BoundsInt(itemBounds.position, itemBounds.size);

        if (itemData.HasLeftEntryOrExit)
        {
            if (borderBounds.min.x > spawnBorder)
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

        if (itemData.HasRightEntryOrExit)
        {
            if (xExtent(borderBounds) < game.Grid.Width - spawnBorder - 1)
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

        if (itemData.HasUpEntryOrExit)
        {
            if (yExtent(borderBounds) < game.Grid.Height - spawnBorder - 1)
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

        if (itemData.HasDownEntryOrExit)
        {
            if (borderBounds.min.y > spawnBorder)
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

            if (xExtent(borderBounds) >= game.Grid.Width - spawnBorder)
            {
                var prevBorderPosition = borderBounds.position;
                borderBounds.position = new Vector3Int(spawnBorder, borderBounds.position.y + 1);

                if (yExtent(borderBounds) >= game.Grid.Height - spawnBorder)
                {
                    borderBounds.position = new Vector3Int(spawnBorder, spawnBorder);
                    if (borderBounds.position == Vector3Int.zero)
                    {
                        // oops. this is the delivery point.
                        borderBounds.position = new Vector3Int(spawnBorder, 1);
                    }
                }

                itemBounds.position += borderBounds.position - prevBorderPosition;
            }

            if (borderBounds.position == startCell)
            {
                // Debug.LogWarning($"Can't find anywhere to spawn item: {itemData}");
                return false;
            }

            if (dontInfiniteLoop++ > 10000)
            {
                Debug.LogWarning($"BUG BUG BUG: {itemData} never wrapped around to {startCell}");
                return false;
            }
        }

        var item = GameObject.Instantiate(itemControllerPrefab, transform);
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

                foreach (var item in items)
                {
                    if (item.ItemBorderContainsCell(checkCell))
                        return false;
                }

                if (game.Snake.ContainsCell(checkCell))
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

    /// <summary>
    /// Returns true if a collectible item was consumed.
    /// </summary>
    public bool SnakeMoved(ItemData specificItem)
    {
        ItemData didConsume = null;

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var itemData = item.RItemData;
            var canConsumeItem = itemData.IsMunchie || itemData.IsCoin || !game.onlyCollectSpecificItem || itemData.ItemData == specificItem;

            if (canConsumeItem && game.Snake.CanConsumeOrCollect(item))
            {
                game.Snake.ConsumeOrCollect(item);
                didConsume = itemData.ItemData;
                Destroy(item.gameObject);
                items[i] = items[^1];
                items.RemoveAt(items.Count - 1);
                break; // can only consume 1 item
            }
        }

        if (didConsume != null)
        {
            if (didConsume.IsCollectible)
            {
                game.Snake.CarryItem(didConsume);
                DespawnMunchies();
            }
            else if (didConsume.IsMunchie)
            {
                SpawnItem(didConsume);
            }
        }

        return false;
    }

    public void SpawnMunchies()
    {
        SpawnItem(appleItemData);
        if (game.mushrooms)
            SpawnItem(mushroomItemData);
    }

    private void DespawnMunchies()
    {
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (item.RItemData.IsApple || item.RItemData.IsMushroom)
            {
                Destroy(item.gameObject);
                items[i] = items[^1];
                items.RemoveAt(items.Count - 1);
                i--;
            }
        }
    }
}
