using System;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Items/Item", order = 0)]
public class ItemData : ScriptableObject
{
    public enum CellType
    {
        Empty = 0,
        EntryOrExit = 1,
        Middle = 2,
        LeftEntry = 3,
        RightEntry = 4,
        UpEntry = 5,
        DownEntry = 6,
        Exit = 7,
    }

    private const string MiddleCellChar = "#";
    private const string EntryOrExitCellChar = "@";
    private const string EmptyCellChar = "_";
    private const string LeftEntryCellChar = "L";
    private const string RightEntryCellChar = "R";
    private const string UpEntryCellChar = "U";
    private const string DownEntryCellChar = "D";
    private const string ExitCellChar = "X";

    public int Value = 0;

    /// <summary>
    /// This does not take into account item rotation.
    /// Use ItemController.GridWidth for that.
    /// </summary>
    [Min(1)]
    public int Width = 1;

    /// <summary>
    /// This does not take into account item rotation.
    /// Use ItemController.GridHeight for that.
    /// </summary>
    [Min(1)]
    public int Height = 1;

    public bool IsApple = false;

    public bool IsMushroom = false;

    public bool IsCoin = false;

    [TextArea(10, 10), SerializeField]
    private string cells;

    [Header("Debug")]
    public Color debugColor = Color.white;

    public int CellCount => GetCellCount();
    public bool IsMunchie => IsApple || IsMushroom;
    public bool IsCollectible => !IsMunchie && !IsCoin;
    public bool HasLeftEntryOrExit => hasLeftEntryOrExit;
    public bool HasRightEntryOrExit => hasRightEntryOrExit;
    public bool HasUpEntryOrExit => hasUpEntryOrExit;
    public bool HasDownEntryOrExit => hasDownEntryOrExit;

    private CellType[][] cachedCellStructure;
    private int cellCount;
    public bool hasLeftEntryOrExit = false;
    public bool hasRightEntryOrExit = false;
    public bool hasUpEntryOrExit = false;
    public bool hasDownEntryOrExit = false;

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

                var cellY = Height - y - 1;

                for (int x = 0; x < Width; x++)
                {
                    if (x >= cells.Length)
                        break;

                    cachedCellStructure[x][cellY] = CellType.Empty;

                    if (cells[x] == MiddleCellChar)
                    {
                        cachedCellStructure[x][cellY] = CellType.Middle;
                    }
                    else if (cells[x] == EntryOrExitCellChar)
                    {
                        cachedCellStructure[x][cellY] = CellType.EntryOrExit;
                        if (x == 0)
                            hasLeftEntryOrExit = true;
                        else if (x == Width - 1)
                            hasRightEntryOrExit = true;
                        else if (y == 0)
                            hasUpEntryOrExit = true;
                        else if (y == Height - 1)
                            hasDownEntryOrExit = true;
                        numEnds++;
                    }
                    else if (cells[x] == LeftEntryCellChar)
                    {
                        cachedCellStructure[x][cellY] = CellType.LeftEntry;
                        hasLeftEntryOrExit = true;
                        numEnds++;
                    }
                    else if (cells[x] == RightEntryCellChar)
                    {
                        cachedCellStructure[x][cellY] = CellType.RightEntry;
                        hasRightEntryOrExit = true;
                        numEnds++;
                    }
                    else if (cells[x] == UpEntryCellChar)
                    {
                        cachedCellStructure[x][cellY] = CellType.UpEntry;
                        hasUpEntryOrExit = true;
                        numEnds++;
                    }
                    else if (cells[x] == DownEntryCellChar)
                    {
                        cachedCellStructure[x][cellY] = CellType.DownEntry;
                        hasDownEntryOrExit = true;
                        numEnds++;
                    }
                    else if (cells[x] == ExitCellChar)
                    {
                        cachedCellStructure[x][cellY] = CellType.Exit;
                        if (x == 0)
                            hasLeftEntryOrExit = true;
                        else if (x == Width - 1)
                            hasRightEntryOrExit = true;
                        else if (y == 0)
                            hasUpEntryOrExit = true;
                        else if (y == Height - 1)
                            hasDownEntryOrExit = true;
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
