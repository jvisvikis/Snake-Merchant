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

        for (int i = 0; i < Mathf.Min(ncid.Count, game.numItems); i++)
            SpawnItem(ncid[i]);
    }

    public void RespawnCoins()
    {
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (item.ItemData.IsCoin)
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

        ItemData spawnItem = null;

        foreach (var itemData in ncid)
        {
            spawnItem = itemData;

            foreach (var item in items)
            {
                if (item.ItemData == itemData)
                {
                    spawnItem = null;
                    break;
                }
            }

            if (spawnItem != null)
                break;
        }

        SpawnItem(spawnItem);
    }

    public ItemController GetRandomExistingCollectibleItem()
    {
        var itemsCopy = new List<ItemController>(items);
        ListUtil.Shuffle(itemsCopy);

        foreach (var item in itemsCopy)
        {
            if (item.ItemData.IsCollectible)
                return item;
        }

        Debug.Assert(false);
        return null;
    }

    private void SpawnItem(ItemData itemData)
    {
        // In order to be collectible, the item needs to have space on the entry side, and we need
        // to keep the rotation of the object in mind both for using the correct width/height and
        // for picking the side that the entry is on.

        // Use a (1, 1) start offset to leave a blank border around the spawn point of the snake so
        // that items never get in the way of returning, and so that the snake doesn't immediately
        // spawn in front of an item.
        var candidateCell = game.Grid.RandomCell(itemData.Width, itemData.Height, 1, 1);

        var itemBounds = new BoundsInt((Vector3Int)candidateCell, new Vector3Int(itemData.Width, itemData.Height));
        var borderBounds = new BoundsInt(itemBounds.position, itemBounds.size);

        if (itemData.HasLeftEntryOrExit)
            borderBounds.min += Vector3Int.left;

        if (itemData.HasRightEntryOrExit)
            borderBounds.max += Vector3Int.right;

        if (itemData.HasUpEntryOrExit)
            borderBounds.max += Vector3Int.up;

        if (itemData.HasDownEntryOrExit)
            borderBounds.min += Vector3Int.down;

        var rotation = ItemController.Rotation.Up;
        // var rotation = (ItemController.Rotation)Random.Range(0, 4);

        // if (rotation == ItemController.Rotation.Right || rotation == ItemController.Rotation.Left)
        // {
        //     (gridWidth, gridHeight) = (gridHeight, gridWidth);
        //     (xOffset, yOffset) = (yOffset, xOffset);
        // }

        var startCell = borderBounds.position;
        int bugCatcher = 0;

        while (!CanSpawnItemInCell((Vector2Int)borderBounds.position, borderBounds.size.x, borderBounds.size.y))
        {
            // linearly search to find a spot that fits.
            borderBounds.position += Vector3Int.right;
            itemBounds.position += Vector3Int.right;

            if (borderBounds.max.x > game.Grid.Width)
            {
                // next row (starts from 1 to leave empty column on left)
                var prevBorderPosition = borderBounds.position;
                borderBounds.position = new Vector3Int(1, prevBorderPosition.y + 1);

                if (borderBounds.max.y > game.Grid.Height)
                {
                    // wrap around the start (starts from 1 to leave empty row on bottom)
                    borderBounds.position = new Vector3Int(1, 1);
                }

                itemBounds.position += borderBounds.position - prevBorderPosition;
            }

            if (borderBounds.position == startCell)
            {
                Debug.LogWarning($"Can't find anywhere to spawn item: {itemData}");
                game.Die();
                return;
            }

            if (bugCatcher++ > 10000)
            {
                Debug.LogWarning("BUG BUG BUG");
                return;
            }
        }

        var item = GameObject.Instantiate(itemControllerPrefab, transform);
        item.transform.position = game.Grid.GetWorldPos((Vector2Int)borderBounds.min);
        item.SetData(itemData);
        item.SetGridLocation(itemBounds, borderBounds, rotation);
        items.Add(item);
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
            var itemData = item.ItemData;
            var canConsumeItem = itemData.IsMunchie || itemData.IsCoin || specificItem == null || itemData == specificItem;

            if (canConsumeItem && game.Snake.CanConsume(item))
            {
                game.Snake.Consume(item);
                didConsume = itemData;
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
            if (item.ItemData.IsApple || item.ItemData.IsMushroom)
            {
                Destroy(item.gameObject);
                items[i] = items[^1];
                items.RemoveAt(items.Count - 1);
                i--;
            }
        }
    }
}
