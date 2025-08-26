using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class SnakeLineRenderer : MonoBehaviour
{
    [SerializeField, Min(1)]
    private int cornerVertices = 3;

    [SerializeField]
    private LineRenderer lineRenderer;

    private Grid grid;
    private List<Vector2Int> cells = new();
    private NativeArray<Vector3> positions; // drawn from tail to head
    private float renderOffset;
    private bool hasExtraTail = false;

    public float RenderOffset => renderOffset;

    public void Init(Grid grid, Vector2Int startCell)
    {
        this.grid = grid;
        cells = new List<Vector2Int> { startCell };
        renderOffset = 0;

        // the snake renderer needs to know about its tail before moving forward so that the offset
        // can draw backwards. however it won't always have it - e.g. when adding a new tail.
        hasExtraTail = false;

        GenerateLineRendererPositions();
    }

    private void OnDestroy()
    {
        if (positions.IsCreated)
            positions.Dispose();
    }

    public void SetWidth(float width)
    {
        width = Mathf.Clamp01(width);
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
    }

    public void MoveForward(Vector2Int startCell)
    {
        var startDistance = Vector2Int.Distance(cells.First(), startCell);
        Debug.Assert(startDistance == 1, $"Forward distance of {cells.First()} to {startCell} is {startDistance}");

        if (hasExtraTail)
        {
            // move cells forward and drop the tail
            for (int i = cells.Count - 1; i > 0; i--)
                cells[i] = cells[i - 1];
            cells[0] = startCell;
        }
        else
        {
            // just add the head
            cells.Insert(0, startCell);
            hasExtraTail = true;
        }

        Draw();
    }

    public void AddTail(Vector2Int tailCell)
    {
        Debug.Assert(tailCell == cells[^1]);
        hasExtraTail = false;
        GenerateLineRendererPositions();
    }

    public void SetRenderOffset(float offset)
    {
        Debug.Assert(offset >= 0 && offset <= 1);
        renderOffset = Mathf.Clamp01(offset);
        Draw();
    }

    private void GenerateLineRendererPositions()
    {
        // Allow for every cell of the snake to be a turn, plus an extra at the end as a fence post,
        // and an extra for the "prev cell tail". This is an overestimate but that's fine.
        if (positions.IsCreated)
            positions.Dispose();
        positions = new((cells.Count + 1) * (cornerVertices + 2) + 1, Allocator.Persistent);
        Draw();
    }

    private void Draw()
    {
        var centerOffset = grid.CellCenterOffset();

        // Draw from tail to head, easier for me to think about in terms of turning corners.
        positions[0] = grid.GetWorldPos(cells[^1]) + centerOffset;

        var positionCount = 1;
        var prevDir = Vector2Int.zero;

        for (int i = cells.Count - 2; i >= 0; i--)
        {
            var cell = cells[i];
            var cellPos = grid.GetWorldPos(cell) + centerOffset;
            var dir = cell - cells[i + 1];

            if (prevDir == Vector2Int.zero || dir == prevDir)
            {
                // straight line.
                positions[positionCount] = cellPos;
                positionCount++;
            }
            else
            {
                // snake has turned a corner... this logic is unnecessarily complicated and not that
                // great since it only gives a smoothness of 1. though TBH that looks cool in its own way.
                // better: step back position count, then generate a quarter-circle.

                var cellOnTurnPos = grid.GetWorldPos(cells[i + 1]) + centerOffset;
                var cellBeforeTurnPos = grid.GetWorldPos(cells[i + 2]) + centerOffset;

                var midpointCellOnTurnVsBefore = Avg(cellOnTurnPos, cellBeforeTurnPos);
                var midpointCellVsOnTurn = Avg(cellPos, cellOnTurnPos);

                // this is the tricky one - the midpoint that forms a circular curve around the
                // pivot point of the current cell's bottom left corner. maybe it would be better to
                // make this generalised to n "midpoints" using trig but eh.
                var pivotCenter = Avg(cellPos, cellBeforeTurnPos);
                var cellOnTurnVsBeforeDirection = (midpointCellOnTurnVsBefore - pivotCenter).normalized;
                var cellVsOnTurnDirection = (midpointCellVsOnTurn - pivotCenter).normalized;

                // go back to the previous cell to generate the curve around it.
                positionCount--;

                for (int p = 0; p < cornerVertices; p++)
                {
                    var t = p / (float)(cornerVertices - 1);
                    var interpolatedDir = Vector3.Slerp(cellOnTurnVsBeforeDirection, cellVsOnTurnDirection, t);
                    positions[positionCount] = pivotCenter + interpolatedDir * grid.CellSize / 2f;
                    positionCount++;
                }

                positions[positionCount] = cellPos;
                positionCount++;
            }

            prevDir = dir;
        }

        // remove duplicates that were added by all the twisty turny logic.
        // maybe my twisty turny logic should be better? this seems like a crutch.
        positionCount = Uniq(positions, positionCount);

        // Smooth snake movement - remember that the positions are drawn from the tail to the head,
        // so positions[0] is the tail and positions[positionCount-1] is the head.
        var dropHeadLength = Mathf.Lerp(grid.CellSize, 0, renderOffset);

        while (dropHeadLength > 0 && positionCount > 1)
        {
            var prevToHeadDelta = positions[positionCount - 2] - positions[positionCount - 1];
            if (prevToHeadDelta.magnitude < dropHeadLength)
            {
                positionCount--;
                dropHeadLength -= prevToHeadDelta.magnitude;
            }
            else
            {
                positions[positionCount - 1] += prevToHeadDelta.normalized * dropHeadLength;
                dropHeadLength = 0;
            }
        }

        var dropTailLength = Mathf.Lerp(0, grid.CellSize, renderOffset);
        int droppedTailPositions = 0;

        while (hasExtraTail && dropTailLength > 0 && positionCount > 1)
        {
            var tailToPrevDelta = positions[droppedTailPositions + 1] - positions[droppedTailPositions];
            if (tailToPrevDelta.magnitude < dropTailLength)
            {
                droppedTailPositions++;
                positionCount--;
                dropTailLength -= tailToPrevDelta.magnitude;
            }
            else
            {
                positions[droppedTailPositions] += tailToPrevDelta.normalized * dropTailLength;
                dropTailLength = 0;
            }
        }

        var positionsSlice = new NativeSlice<Vector3>(positions, droppedTailPositions, positionCount);
        lineRenderer.positionCount = positionsSlice.Length;
        lineRenderer.SetPositions(positionsSlice);
    }

    private static int Uniq(NativeArray<Vector3> positionArray, int positionCount)
    {
        int positionIndex = 1;

        for (int i = 1; i < positionCount; i++)
        {
            if (positionArray[positionIndex - 1] != positionArray[i])
            {
                positionArray[positionIndex] = positionArray[i];
                positionIndex++;
            }
        }

        return positionIndex;
    }

    private Vector3 Avg(Vector3 a, Vector3 b)
    {
        return (a + b) / 2;
    }

    public Vector3 GetHeadPosition(out Vector3 lookDirection)
    {
        var head = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
        lookDirection = Vector3.up;

        if (lineRenderer.positionCount > 1)
        {
            var next = lineRenderer.GetPosition(lineRenderer.positionCount - 2);
            lookDirection = (head - next).normalized;
        }

        return head;
    }
}
