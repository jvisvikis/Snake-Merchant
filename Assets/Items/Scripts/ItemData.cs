using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Items/Item", order = 0)]
public class ItemData : ScriptableObject
{
    public enum CellType
    {
        Empty = 0,
        Entry = 1,
        Middle = 2,
    }

    [Min(1)]
    public int Width = 1;

    [Min(1)]
    public int Height = 1;

    public bool IsApple = false;

    public bool IsMushroom = false;

    public bool IsCoin = false;

    [TextArea(10, 10), SerializeField, Tooltip("Structure of the item, occupy-able bits use the '#' character and empty use '_'")]
    private string cells;

    [Header("Debug")]
    public Color debugColor = Color.white;

    private CellType[][] cachedCellStructure;
    private int cellCount;

    public int CellCount => GetCellCount();
    public bool IsConsumable => IsApple || IsMushroom || IsCoin;
    public bool IsCollectible => !IsConsumable;

    private const string MiddleCellChar = "#";
    private const string EntryCellChar = "@";
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
                if (cachedCellStructure[x][y] != CellType.Empty)
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
    public CellType[][] GetCellStructure()
    {
        if (cachedCellStructure == null)
        {
            int numEnds = 0;
            cachedCellStructure = new CellType[Width][];

            for (int i = 0; i < Width; i++)
                cachedCellStructure[i] = new CellType[Height];

            var lines = cells.Split("\n", System.StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length != Height)
                Debug.LogWarning($"Item {name} has height {Height}, but its structure has {lines.Length}");

            for (int y = 0; y < Height; y++)
            {
                if (y >= lines.Length)
                    break;

                var cells = lines[y].Split(" ", System.StringSplitOptions.RemoveEmptyEntries);
                if (cells.Length != Width)
                    Debug.LogWarning($"Item {name} has width {Width}, but its structure has {cells.Length} entries on row {y}");

                for (int x = 0; x < Width; x++)
                {
                    if (x >= cells.Length)
                        break;

                    cachedCellStructure[x][y] = CellType.Empty;

                    if (cells[x] == MiddleCellChar)
                    {
                        cachedCellStructure[x][y] = CellType.Middle;
                    }
                    else if (cells[x] == EntryCellChar)
                    {
                        cachedCellStructure[x][y] = CellType.Entry;
                        numEnds++;
                    }
                    else if (cells[x] != EmptyCellChar)
                    {
                        Debug.LogWarning($"Item {name} has invalid character {cells[x]}");
                    }
                }
            }

            if (cachedCellStructure.Length > 1 && numEnds < 2)
                Debug.LogWarning($"Item {name} has {numEnds} ends, but it should have at least 2!");
        }

        return cachedCellStructure;
    }
}
