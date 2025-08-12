using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsManager : MonoBehaviour
{
    [SerializeField]
    private List<ItemData> itemData = new();

    [SerializeField]
    private ItemController itemControllerPrefab;

    [SerializeField]
    private int randomSpawnItemTries = 30;

    private Game game;
    private ItemData appleItemData;
    private ItemData mushroomItemData;
    private List<ItemController> items = new();

    private void Awake()
    {
        game = GetComponent<Game>();

        foreach (var i in itemData)
        {
            if (i.type == ItemData.ItemType.Apple)
                appleItemData = i;
            else if (i.type == ItemData.ItemType.Mushroom)
                mushroomItemData = i;
        }
    }

    private void Start()
    {
        foreach (var id in itemData)
            SpawnItem(id);

        // TODO: only spawn apple/mushroom, the other items spawn on a timer?
        // if (appleItemData)
        //     SpawnItem(appleItemData);
        // if (mushroomItemData)
        //     SpawnItem(mushroomItemData);
    }

    private void SpawnItem(ItemData itemData)
    {
        Vector2Int cell = Vector2Int.down; // arbitrary invalid value

        for (int i = 0; i < randomSpawnItemTries; i++)
        {
            cell = game.Grid.RandomCell(itemData.Width, itemData.Height);
            if (!CellIsOccupied(cell))
                break;
        }

        if (cell == Vector2Int.down)
        {
            // TODO do a linear search to find a spot rather than giving up
            Debug.LogWarning("Couldn't find anywhere random to spawn!");
            return;
        }

        // TODO also a random orientation if it's not a 1-by-1 item, but note, not using unity
        // transform rotation but by using tricky maths and stuff.

        var item = GameObject.Instantiate(itemControllerPrefab, transform);
        item.transform.position = game.Grid.GetWorldPos(cell);
        item.SetGridLocation(cell);
        item.SetData(itemData);
        items.Add(item);
    }

    private bool CellIsOccupied(Vector2Int cell)
    {
        foreach (var item in items)
        {
            if (item.ContainsCell(cell))
                return true;
        }

        return game.Snake.ContainsCell(cell);
    }

    public void SnakeMoved()
    {
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (game.Snake.ExactlyContainsItem(item))
            {
                game.Snake.Consume(item.ItemData);

                // TODO only spawn if an apple or a mushroom?
                SpawnItem(item.ItemData);

                Destroy(item.gameObject);
                items[i] = items[^1];
                items.RemoveAt(items.Count - 1);
                i--;
            }
        }
    }
}
