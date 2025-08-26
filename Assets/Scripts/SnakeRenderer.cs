using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeRenderer : MonoBehaviour
{
    [SerializeField, Range(0.5f, 1)]
    private float snakeWidth = 0.8f;

    [SerializeField, Range(0f, 0.5f)]
    private float borderWidth = 0.2f;

    [SerializeField]
    private SnakeLineRenderer borderLine;

    [SerializeField]
    private SnakeLineRenderer bodyLine;

    public float RenderOffset => borderLine.RenderOffset;
    public Vector2Int BehindExtraTailCell => borderLine.BehindExtraTailCell;

    private void Start()
    {
        borderLine.SetWidth(snakeWidth);
        bodyLine.SetWidth(snakeWidth - borderWidth);
    }

    public void Init(Grid grid, Vector2Int startCell, Vector2Int behindStartCell)
    {
        borderLine.Init(grid, startCell, behindStartCell);
        bodyLine.Init(grid, startCell, behindStartCell);
    }

    public void MoveForward(Vector2Int startCell)
    {
        borderLine.MoveForward(startCell);
        bodyLine.MoveForward(startCell);
    }

    public void AddTail(Vector2Int tailCell)
    {
        borderLine.AddTail(tailCell);
        bodyLine.AddTail(tailCell);
    }

    public void SetRenderOffset(float offset)
    {
        borderLine.SetRenderOffset(offset);
        bodyLine.SetRenderOffset(offset);
    }

    public Vector3 GetHeadPosition(out Vector3 headDirection)
    {
        return borderLine.GetHeadPosition(out headDirection);
    }
}
