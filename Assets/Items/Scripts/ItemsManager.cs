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

    // Maps item that is walking off screen => delay until start walking.
    private Dictionary<ItemController, int> walkingItems = new();

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

        // TODO: Spawn multiple coins, make them only last a set number of rounds before expiring,
        // and only respawn on expiration not when collected.
        for (int i = 0; i < game.numCoins; i++)
            SpawnItem(coinItemData);

        // weird copy/shuffle logic so that we (a) spawn random items and (b) don't spawn the same
        // item more than once.
        var ncid = new List<ItemData>(collectibleItemData);
        ListUtil.Shuffle(ncid);

        for (int i = 0; i < Mathf.Min(ncid.Count, game.numItems); i++)
            SpawnItem(ncid[i]);
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
        var cell = game.Grid.RandomCell(itemData.Width, itemData.Height);
        var startCell = cell;

        while (CellIsOccupied(cell, itemData.Width, itemData.Height))
        {
            // linearly search to find a spot that fits.
            cell.x++;

            if (cell.x + itemData.Width >= game.Grid.Width)
            {
                cell.y++;
                cell.x = 0;
            }

            if (cell.y + itemData.Height >= game.Grid.Height)
                cell.y = 0;

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

    private bool CellIsOccupied(Vector2Int cell, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var checkCell = cell + new Vector2Int(x, y);

                foreach (var item in items)
                {
                    if (item.ContainsCell(checkCell))
                        return true;
                }

                foreach (var walkingItem in walkingItems.Keys)
                {
                    if (walkingItem.ContainsCell(checkCell))
                        return true;
                }

                if (game.Snake.ContainsCell(checkCell))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns true if a collectible item was consumed.
    /// </summary>
    public bool SnakeMoved(ItemData specificItem)
    {
        var walkingItemKeys = new List<ItemController>(walkingItems.Keys);

        foreach (var walkingItem in walkingItemKeys)
        {
            if (walkingItems[walkingItem] == 0)
            {
                if (!walkingItem.MoveLeft())
                    Destroy(walkingItem);
            }
            else
            {
                walkingItems[walkingItem]--;
            }
        }

        ItemData didConsume = null;

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var itemData = item.ItemData;

            var canConsumeItem = itemData.IsConsumable || specificItem == null || itemData == specificItem;

            if (canConsumeItem && game.Snake.CanConsume(item))
            {
                game.Snake.Consume(itemData);
                didConsume = itemData;
                if (itemData.IsCollectible && game.collectionWalksOffScreen)
                    walkingItems[item] = game.collectionWalkDelay;
                else
                    Destroy(item.gameObject);
                items[i] = items[^1];
                items.RemoveAt(items.Count - 1);
            }
        }

        if (didConsume != null)
        {
            if (didConsume.IsCollectible)
            {
                SpawnRandomNonExistentCollectibleItem();
                return true;
            }
            if (game.snakeCarriesItemOnCollection)
                game.Snake.CarryItem(didConsume);
            SpawnItem(didConsume);
        }

        return false;
    }

    public void FinishedWalking(ItemController item)
    {
        walkingItems.Remove(item);
        Destroy(item);
    }
}
