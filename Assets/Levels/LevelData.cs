using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelData", menuName = "Snake/Level", order = 0)]
public class LevelData : ScriptableObject
{
    [Min(1)]
    public int Width = 10;

    [Min(1)]
    public int Height = 10;

    public int Cost = 1;
    public int NumCoins = 5;
    public int NumItems = 3;
    public int NumRandomObstacles = 0;
    public bool Mushrooms = false;
    public ItemDataCollection Items;

    public List<Vector2Int> Obstacles => GetObstacles();

    [TextArea(20, 40), SerializeField]
    private string layout;

    private const string EmptyCellChar = "_";
    private const string ObstacleCellChar = "O";

    [System.NonSerialized]
    private List<Vector2Int> obstacles;

    private List<Vector2Int> GetObstacles()
    {
        Debug.Log($"getting obstacles: {layout}");
        if (obstacles == null)
            ParseLayout();
        return obstacles;
    }

    private void ParseLayout()
    {
        Debug.Log($"parsing layout: {layout}");
        obstacles = new();

        var lines = layout.Split("\n", System.StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length != Height)
            Debug.LogWarning($"Level {name} has height {Height}, but its layout has {lines.Length}");

        for (int y = 0; y < Height; y++)
        {
            if (y >= lines.Length)
                break;

            var cells = lines[y].Split(" ", System.StringSplitOptions.RemoveEmptyEntries);

            if (cells.Length != Width)
                Debug.LogWarning($"Level {name} has width {Width}, but its layout has {cells.Length} entries on row {y}");

            for (int x = 0; x < Width; x++)
            {
                if (x >= cells.Length)
                    break;

                if (cells[x] == ObstacleCellChar)
                {
                    obstacles.Add(new Vector2Int(x, y));
                }
                else if (cells[x] != EmptyCellChar)
                {
                    Debug.LogWarning($"Level {name} has invalid character {cells[x]}");
                }
            }
        }
    }
}
