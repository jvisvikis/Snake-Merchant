using System;
using System.Collections.Generic;
using FMODUnity;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Snake/Item", order = 0)]
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

    public enum ItemMaterial
    {
        None = 0,
        Wood = 1,
        MetalBig = 2,
        MetalSmall = 3,
        Glass = 4,
        Paper = 5,
        Fabric = 6,
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
    public List<string> flavourText;

    [Header("Special items")]
    public bool IsApple = false;

    public bool IsMushroom = false;

    public bool IsCoin = false;

    public bool IsObstacle = false;

    [TextArea(10, 10), SerializeField]
    private string cells;

    [Header("Rendering")]
    public Sprite sprite;

    public float SpriteScale = 1f;

    [Header("Audio")]
    public EventReference ConsumeAudio;

    public ItemMaterial itemMaterial;

    [Header("Debug")]
    public Color debugColor = Color.white;

    public int CellCount => GetCellCount();
    public bool IsMunchie => IsApple || IsMushroom;
    public bool IsCollectible => !IsMunchie && !IsCoin && !IsObstacle;
    public bool IsConsumable => IsMunchie || IsCoin;
    public bool HasLeftEntryOrExit => hasLeftEntryOrExit;
    public bool HasRightEntryOrExit => hasRightEntryOrExit;
    public bool HasUpEntryOrExit => hasUpEntryOrExit;
    public bool HasDownEntryOrExit => hasDownEntryOrExit;

    [NonSerialized]
    private CellType[][] cellStructure;

    [NonSerialized]
    private int cellCount;

    [NonSerialized]
    private bool hasLeftEntryOrExit = false;

    [NonSerialized]
    private bool hasRightEntryOrExit = false;

    [NonSerialized]
    private bool hasUpEntryOrExit = false;

    [NonSerialized]
    private bool hasDownEntryOrExit = false;

    public static bool IsAnyExit(CellType cellType)
    {
        return cellType == CellType.EntryOrExit || cellType == CellType.Exit;
    }

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
                if (cellStructure[x][y] != CellType.Empty)
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
        if (cellStructure != null)
            return cellStructure;

        int numEnds = 0;
        cellStructure = new CellType[Width][];

        for (int i = 0; i < Width; i++)
            cellStructure[i] = new CellType[Height];

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

                cellStructure[x][cellY] = CellType.Empty;

                if (cells[x] == MiddleCellChar)
                {
                    cellStructure[x][cellY] = CellType.Middle;
                }
                else if (cells[x] == EntryOrExitCellChar)
                {
                    cellStructure[x][cellY] = CellType.EntryOrExit;
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
                    cellStructure[x][cellY] = CellType.LeftEntry;
                    hasLeftEntryOrExit = true;
                    numEnds++;
                }
                else if (cells[x] == RightEntryCellChar)
                {
                    cellStructure[x][cellY] = CellType.RightEntry;
                    hasRightEntryOrExit = true;
                    numEnds++;
                }
                else if (cells[x] == UpEntryCellChar)
                {
                    cellStructure[x][cellY] = CellType.UpEntry;
                    hasUpEntryOrExit = true;
                    numEnds++;
                }
                else if (cells[x] == DownEntryCellChar)
                {
                    cellStructure[x][cellY] = CellType.DownEntry;
                    hasDownEntryOrExit = true;
                    numEnds++;
                }
                else if (cells[x] == ExitCellChar)
                {
                    cellStructure[x][cellY] = CellType.Exit;
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

        // if (cellStructure.Length > 1 && numEnds < 2)
        //     Debug.LogWarning($"Item {name} has {numEnds} ends, but it should have at least 2!");

        return cellStructure;
    }
}
