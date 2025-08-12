using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Items/Item", order = 0)]
public class ItemData : ScriptableObject
{
    public enum ItemType
    {
        None,
        Apple,
        Mushroom,
        Boot
    }

    public ItemType type;

    [Min(1)]
    public int Width = 1;

    [Min(1)]
    public int Height = 1;

    [Min(1)]
    public int CellCount = 1;

    [TextArea, SerializeField, Tooltip("Structure of the item, occupy-able bits use the '#' character and empty use '_'")]
    private string cells;

    [Header("Debug")]
    public Color debugColor = Color.white;

    [DoNotSerialize]
    private bool[][] cachedCells;

    public bool IsApple => type == ItemType.Apple;
    public bool IsMushroom => type == ItemType.Mushroom;

    /// <summary>
    /// The cell structure of the item as a 2D array of [width][height], i.e. cells[1][2] is the cell
    /// at column [1] and row [2]. Or think of it as [x][y].
    /// True means that the snake can occupy that cell, false means it's empty space.
    /// </summary>
    public bool[][] GetCells()
    {
        if (cachedCells == null)
        {
            cachedCells = new bool[Width][];
            for (int i = 0; i < Width; i++)
                cachedCells[i] = new bool[Height];

            var lines = cells.Split("\n", System.StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length != Height)
                Debug.LogWarning($"Item {type} has height {Height}, but its structure has {lines.Length}");

            for (int row = 0; row < Height; row++)
            {
                if (row >= lines.Length)
                    break;

                var cells = lines[row].Split(" ", System.StringSplitOptions.RemoveEmptyEntries);
                if (cells.Length != Width)
                    Debug.LogWarning($"Item {type} has width {Width}, but its structure has {cells.Length} entries on row {row}");

                for (int column = 0; column < Width; column++)
                {
                    if (column >= cells.Length)
                        break;

                    if (cells[column] != "#" && cells[column] != "_")
                        Debug.LogWarning($"Item {type} has invalid character {cells[column]}");

                    cachedCells[row][column] = cells[column] == "#";
                }
            }
        }

        return cachedCells;
    }
}
