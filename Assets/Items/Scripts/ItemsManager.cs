using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsManager : MonoBehaviour
{
    [SerializeField]
    private List<ItemData> allItemData = new();

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

        foreach (var itemData in allItemData)
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

    private void Start()
    {
        SpawnItem(appleItemData);
        SpawnItem(mushroomItemData);

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

    private void SpawnRandomNonExistentCollectibleItem()
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

        return null;
    }

    private void SpawnItem(ItemData itemData)
    {
        var cell = game.Grid.RandomCell(itemData.Width, itemData.Height, 1, 1);
        var startCell = cell;

        while (CellIsOccupied(cell, itemData.Width, itemData.Height))
        {
            // linearly search to find a spot that fits.
            cell.x++;

            if (cell.x + itemData.Width >= game.Grid.Width)
            {
                cell.y++;
                cell.x = 1;
            }

            if (cell.y + itemData.Height >= game.Grid.Height)
                cell.y = 1;

            if (cell == startCell)
            {
                Debug.LogWarning($"Can't find anywhere to spawn item: {itemData}");
                game.Die();
                return;
            }
        }

        // TODO also a random orientation if it's not a 1-by-1 item, but note, not using unity
        // transform rotation but by using tricky maths and stuff.

        var item = GameObject.Instantiate(itemControllerPrefab, transform);
        item.transform.position = game.Grid.GetWorldPos(cell);
        item.SetGridLocation(cell);
        item.SetData(itemData);
        items.Add(item);
    }

    /// <summary>
    /// Rough check that gridCell is occupied. Doesn't check inside the structure of items, e.g. the
    /// middle of a donut will still return true.
    /// </summary>
    private bool CellIsOccupied(Vector2Int gridCell, int itemWidth, int itemHeight)
    {
        for (int x = 0; x < itemWidth; x++)
        {
            for (int y = 0; y < itemHeight; y++)
            {
                var checkCell = gridCell + new Vector2Int(x, y);

                foreach (var item in items)
                {
                    if (item.ContainsCell(checkCell))
                        return true;
                }

                if (game.Snake.ContainsCell(checkCell))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Finds the item at a grid cell, and returns the cell type at that grid cell if one is found.
    /// Checks the internal structure of the cell, e.g. the middle of a donut will return null.
    /// </summary>
    public ItemController GetItemAtCell(Vector2Int gridCell, out ItemData.CellType cellType)
    {
        foreach (var item in items)
        {
            if (item.ContainsCell(gridCell))
            {
                var cellOffset = gridCell - item.Cell;
                cellType = item.ItemData.GetCellStructure()[cellOffset.x][cellOffset.y];
                if (cellType != ItemData.CellType.Empty)
                    return item;
            }
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
            var canConsumeItem = itemData.IsConsumable || specificItem == null || itemData == specificItem;

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
                if (game.snakeCarriesItemOnCollection)
                    game.Snake.CarryItem(didConsume);

                SpawnRandomNonExistentCollectibleItem();
                return true;
            }

            if (!didConsume.IsCoin)
                SpawnItem(didConsume);
        }

        return false;
    }

    public void FinishedWalking(ItemController item)
    {
        Destroy(item);
    }
}
