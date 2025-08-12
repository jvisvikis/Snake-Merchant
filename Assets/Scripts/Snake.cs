using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    [SerializeField]
    private SnakePart partPrefab;

    public Vector2Int Head => ToVector2Int(parts[0].transform.localPosition);
    public Vector2Int Dir => dir;

    private Game game;
    private Vector2Int dir = Vector2Int.right;
    private List<SnakePart> parts;
    private bool newPartOnNextMove = false;
    private Vector2Int newDirOnNextMove = Vector2Int.zero;

    private void Awake()
    {
        game = FindObjectOfType<Game>();
        parts = new(GetComponentsInChildren<SnakePart>());
    }

    public void SetSize(float size)
    {
        this.transform.localScale = new Vector3(size, size);
    }

    public void SetInitialPos(Vector2Int pos)
    {
        parts[0].transform.localPosition = new Vector3(pos.x, pos.y);
        for (int i = 1; i < parts.Count; i++)
        {
            parts[i].transform.localPosition = parts[i - 1].transform.localPosition + Vector3.left;
        }
    }

    public bool ContainsCell(Vector2Int cell)
    {
        foreach (var part in parts)
        {
            if (cell == ToVector2Int(part.transform.localPosition))
                return true;
        }
        return false;
    }

    public void SetDirection(Vector2Int dir)
    {
        newDirOnNextMove = dir;
    }

    public bool Move()
    {
        if (newDirOnNextMove != Vector2Int.zero)
        {
            if (dir != -newDirOnNextMove)
                dir = newDirOnNextMove;
            newDirOnNextMove = Vector2Int.zero;
        }

        var newPos = Head + Dir;

        if (!game.Grid.InGrid(newPos.x, newPos.y))
            return false;

        var tailPosition = parts[^1].transform.localPosition;

        for (int i = parts.Count - 1; i > 0; i--)
        {
            parts[i].transform.localPosition = parts[i - 1].transform.localPosition;
        }

        parts[0].transform.localPosition += FromVector2Int(dir);

        if (newPartOnNextMove)
        {
            var part = GameObject.Instantiate(partPrefab, transform);
            part.transform.localPosition = tailPosition;
            parts.Add(part);
            newPartOnNextMove = false;
        }

        return true;
    }

    private static Vector2Int ToVector2Int(Vector3 v)
    {
        return new Vector2Int((int)Mathf.Round(v.x), (int)Mathf.Round(v.y));
    }

    private static Vector3 FromVector2Int(Vector2Int v)
    {
        return new Vector3(v.x, v.y, 0);
    }

    public bool Occupies(Vector2Int coord)
    {
        foreach (var part in parts)
        {
            if (coord == ToVector2Int(part.transform.localPosition))
                return true;
        }
        return false;
    }

    public void Consume(ItemData itemData)
    {
        if (itemData.IsApple)
        {
            newPartOnNextMove = true;
        }
        else if (itemData.IsMushroom)
        {
            if (parts.Count <= 2)
            {
                // TODO die?
                return;
            }
            GameObject.Destroy(parts[^1].gameObject);
            parts.RemoveAt(parts.Count - 1);
        }
    }
}
