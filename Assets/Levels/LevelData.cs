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
    public ItemDataCollection RandomObstacles;

    public List<Vector2Int> Obstacles => GetObstacles();
    public bool HasSpawnPoint => spawnPoint.x != -1;
    public Vector2Int SpawnPoint => spawnPoint;

    [TextArea(20, 40), SerializeField]
    private string layout;

    private const string EmptyCellChar = "_";
    private const string ObstacleCellChar = "O";
    private const string SpawnCellChar = "+";

    [System.NonSerialized]
    private List<Vector2Int> obstacles;

    [System.NonSerialized]
    private Vector2Int spawnPoint = new Vector2Int(-1, 0);

    private List<Vector2Int> GetObstacles()
    {
        return obstacles;
    }

    public void ParseLayout()
    {
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
                if (cells[x] == SpawnCellChar)
                {
                    spawnPoint = new Vector2Int(x, y);
                }
                else if (cells[x] != EmptyCellChar)
                {
                    Debug.LogWarning($"Level {name} has invalid character {cells[x]}");
                }
            }
        }
    }
}
