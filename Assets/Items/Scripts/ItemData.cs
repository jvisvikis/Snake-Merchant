using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Items/Item", order = 0)]
public class ItemData : ScriptableObject
{
    public enum ItemType
    {
        None = 0,
        Apple = 1,
        Mushroom = 2,
        Boot = 3,
        Box = 4,
        Elbow = 5,
        Stick = 6,
    }

    public ItemType type;

    [Min(1)]
    public int Width = 1;

    [Min(1)]
    public int Height = 1;

    [TextArea(10, 10), SerializeField, Tooltip("Structure of the item, occupy-able bits use the '#' character and empty use '_'")]
    private string cells;

    [Header("Debug")]
    public Color debugColor = Color.white;

    private bool[][] cachedCellStructure;
    private int cellCount;

    public int CellCount => GetCellCount();
    public bool IsApple => type == ItemType.Apple;
    public bool IsMushroom => type == ItemType.Mushroom;
    public bool IsConsumable => IsApple || IsMushroom;
    public bool IsCollectible => !IsConsumable;

    private const string OccupiedCellChar = "#";
    private const string EmptyCellChar = "_";

    public int GetCellCount()
    {
        if (cellCount > 0)
            return cellCount;

        GetCellStructure();

        int count = 0;
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (cachedCellStructure[x][y])
                    count++;
            }
        }

        cellCount = count;
        return count;
    }

    /// <summary>
    /// The cell structure of the item as a 2D array of [width][height], i.e. cells[1][2] is the cell
    /// at column [1] and row [2]. Or think of it as [x][y].
    /// True means that the snake can occupy that cell, false means it's empty space.
    /// </summary>
    public bool[][] GetCellStructure()
    {
        if (cachedCellStructure == null)
        {
            cachedCellStructure = new bool[Width][];
            for (int i = 0; i < Width; i++)
                cachedCellStructure[i] = new bool[Height];

            var lines = cells.Split("\n", System.StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length != Height)
                Debug.LogWarning($"Item {type} has height {Height}, but its structure has {lines.Length}");

            for (int y = 0; y < Height; y++)
            {
                if (y >= lines.Length)
                    break;

                var cells = lines[y].Split(" ", System.StringSplitOptions.RemoveEmptyEntries);
                if (cells.Length != Width)
                    Debug.LogWarning($"Item {type} has width {Width}, but its structure has {cells.Length} entries on row {y}");

                for (int x = 0; x < Width; x++)
                {
                    if (x >= cells.Length)
                        break;

                    if (cells[x] != OccupiedCellChar && cells[x] != EmptyCellChar)
                        Debug.LogWarning($"Item {type} has invalid character {cells[x]}");

                    cachedCellStructure[x][y] = cells[x] == OccupiedCellChar;
                }
            }
        }

        return cachedCellStructure;
    }
}
