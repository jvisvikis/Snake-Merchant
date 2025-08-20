using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Accessor for ItemData that automatically returns values as though it were rotated. This has
/// every field/method of ItemData so is a drop-in replacement.
/// </summary>
public class RotatedItemData
{
    public ItemData ItemData { get; private set; }
    public ItemRotation Rotation { get; private set; }

    public string Name => ItemData.name;
    public int Value => ItemData.Value;
    public int Width => GetWidth();
    public int Height => GetHeight();
    public Sprite Sprite => ItemData.sprite;
    public bool IsApple => ItemData.IsApple;
    public bool IsMushroom => ItemData.IsMushroom;
    public bool IsCoin => ItemData.IsCoin;
    public Color DebugColor => ItemData.debugColor;
    public int CellCount => ItemData.GetCellCount();
    public bool IsMunchie => ItemData.IsMunchie;
    public bool IsCollectible => ItemData.IsCollectible;
    public bool IsConsumable => ItemData.IsConsumable;
    public bool HasLeftEntryOrExit => GetHasLeftEntryOrExit();
    public bool HasRightEntryOrExit => GetHasRightEntryOrExit();
    public bool HasUpEntryOrExit => GetHasUpEntryOrExit();
    public bool HasDownEntryOrExit => GetHasDownEntryOrExit();

    private ItemData.CellType[][] cellStructure;

    public RotatedItemData(ItemData itemData, ItemRotation rotation)
    {
        ItemData = itemData;
        Rotation = rotation;
    }

    public int GetCellCount()
    {
        return ItemData.GetCellCount();
    }

    public Quaternion RotationQuaternion()
    {
        switch (Rotation)
        {
            case ItemRotation.Up:
                return Quaternion.Euler(0, 0, 0);
            case ItemRotation.Down:
                return Quaternion.Euler(0, 0, 180);
            case ItemRotation.Left:
                return Quaternion.Euler(0, 0, 90);
            case ItemRotation.Right:
                return Quaternion.Euler(0, 0, -90);
        }
        Debug.Assert(false);
        return Quaternion.identity;
    }

    public ItemData.CellType[][] GetCellStructure()
    {
        if (cellStructure != null)
            return cellStructure;

        var originalCellStructure = ItemData.GetCellStructure();
        var originalWidth = ItemData.Width;
        var originalHeight = ItemData.Height;

        cellStructure = new ItemData.CellType[Width][];

        for (int i = 0; i < Width; i++)
            cellStructure[i] = new ItemData.CellType[Height];

        for (int originalY = 0; originalY < originalHeight; originalY++)
        {
            for (int originalX = 0; originalX < originalWidth; originalX++)
            {
                int rotatedX;
                int rotatedY;

                if (Rotation == ItemRotation.Up)
                {
                    rotatedX = originalX;
                    rotatedY = originalY;
                }
                else if (Rotation == ItemRotation.Down)
                {
                    rotatedX = originalWidth - originalX - 1;
                    rotatedY = originalHeight - originalY - 1;
                }
                else if (Rotation == ItemRotation.Right)
                {
                    rotatedX = originalY;
                    rotatedY = originalWidth - originalX - 1;
                }
                else // Rotation == ItemRotation.Left)
                {
                    rotatedX = originalHeight - originalY - 1;
                    rotatedY = originalX;
                }

                cellStructure[rotatedX][rotatedY] = GetRotatedCellType(originalCellStructure[originalX][originalY]);
            }
        }

        return cellStructure;
    }

    private int GetWidth()
    {
        return IsLeftOrRight() ? ItemData.Height : ItemData.Width;
    }

    private int GetHeight()
    {
        return IsLeftOrRight() ? ItemData.Width : ItemData.Height;
    }

    private bool IsLeftOrRight()
    {
        return Rotation == ItemRotation.Right || Rotation == ItemRotation.Left;
    }

    private ItemData.CellType GetRotatedCellType(ItemData.CellType originalCellType)
    {
        var cellTypeOrder = new Dictionary<ItemData.CellType, int>
        {
            { ItemData.CellType.UpEntry, 0 },
            { ItemData.CellType.RightEntry, 1 },
            { ItemData.CellType.DownEntry, 2 },
            { ItemData.CellType.LeftEntry, 3 },
        };
        var rotationOrder = new Dictionary<ItemRotation, int>
        {
            { ItemRotation.Up, 0 },
            { ItemRotation.Right, 1 },
            { ItemRotation.Down, 2 },
            { ItemRotation.Left, 3 },
        };

        if (!cellTypeOrder.ContainsKey(originalCellType))
            return originalCellType;

        var originalOrder = cellTypeOrder[originalCellType];
        var rotatedOrder = (originalOrder + rotationOrder[Rotation]) % 4;

        foreach (var kv in cellTypeOrder)
        {
            if (kv.Value == rotatedOrder)
                return kv.Key;
        }

        Debug.Assert(false);
        return originalCellType;
    }

    private bool GetHasLeftEntryOrExit()
    {
        switch (Rotation)
        {
            case ItemRotation.Up:
                return ItemData.HasLeftEntryOrExit;
            case ItemRotation.Down:
                return ItemData.HasRightEntryOrExit;
            case ItemRotation.Left:
                return ItemData.HasUpEntryOrExit;
            case ItemRotation.Right:
                return ItemData.HasDownEntryOrExit;
        }
        Debug.Assert(false);
        return false;
    }

    private bool GetHasRightEntryOrExit()
    {
        switch (Rotation)
        {
            case ItemRotation.Up:
                return ItemData.HasRightEntryOrExit;
            case ItemRotation.Down:
                return ItemData.HasLeftEntryOrExit;
            case ItemRotation.Left:
                return ItemData.HasDownEntryOrExit;
            case ItemRotation.Right:
                return ItemData.HasUpEntryOrExit;
        }
        Debug.Assert(false);
        return false;
    }

    private bool GetHasUpEntryOrExit()
    {
        switch (Rotation)
        {
            case ItemRotation.Up:
                return ItemData.HasUpEntryOrExit;
            case ItemRotation.Down:
                return ItemData.HasDownEntryOrExit;
            case ItemRotation.Left:
                return ItemData.HasRightEntryOrExit;
            case ItemRotation.Right:
                return ItemData.HasLeftEntryOrExit;
        }
        Debug.Assert(false);
        return false;
    }

    private bool GetHasDownEntryOrExit()
    {
        switch (Rotation)
        {
            case ItemRotation.Up:
                return ItemData.HasDownEntryOrExit;
            case ItemRotation.Down:
                return ItemData.HasUpEntryOrExit;
            case ItemRotation.Left:
                return ItemData.HasLeftEntryOrExit;
            case ItemRotation.Right:
                return ItemData.HasRightEntryOrExit;
        }
        Debug.Assert(false);
        return false;
    }

    public override string ToString()
    {
        return $"RotatedItemData({Name} #{CellCount} @ {Rotation})";
    }
}
