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
        bodyLine.SetWidth(snakeWidth - borderWidth * 2f);
    }

    public void SetLumpProgress(float t)
    {
        if (t == 0 || t == 1)
        {
            borderLine.SetWidthCurve(AnimationCurve.Constant(0, 1, snakeWidth));
            bodyLine.SetWidthCurve(AnimationCurve.Constant(0, 1, snakeWidth - borderWidth * 2f));
            return;
        }

        t = Mathf.SmoothStep(1, 0, t);
        var segmentWidth = 0.5f;

        var lineLength = borderLine.LineLength();
        var progressCenter = lineLength * t;
        var progressTail = progressCenter - segmentWidth;
        var progressHead = progressCenter + segmentWidth;
        var bodyWidth = snakeWidth - borderWidth * 2f;

        if (progressTail < 0)
        {
            progressTail = 0f;
            progressCenter = segmentWidth;
            progressHead = segmentWidth * 2f;
        }
        if (progressHead > lineLength)
        {
            progressHead = lineLength;
            progressCenter = lineLength - segmentWidth;
            progressTail = lineLength - 2f * segmentWidth;
        }

        borderLine.SetWidthCurve(
            new AnimationCurve(
                new Keyframe[]
                {
                    new Keyframe(0, snakeWidth),
                    new Keyframe(progressTail / lineLength, snakeWidth),
                    new Keyframe(progressCenter / lineLength, 1f),
                    new Keyframe(progressHead / lineLength, snakeWidth),
                    new Keyframe(1, snakeWidth),
                }
            )
        );

        bodyLine.SetWidthCurve(
            new AnimationCurve(
                new Keyframe[]
                {
                    new Keyframe(0, bodyWidth),
                    new Keyframe(progressTail / lineLength, bodyWidth),
                    new Keyframe(progressCenter / lineLength, 1f - borderWidth * 2f),
                    new Keyframe(progressHead / lineLength, bodyWidth),
                    new Keyframe(1, bodyWidth),
                }
            )
        );
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

    // public void SetRenderProgress(float rp)
    // {
    //     borderLine.SetRenderProgress(rp);
    //     bodyLine.SetRenderProgress(rp);
    // }

    public Vector3 GetHeadPosition(out Vector3 headDirection)
    {
        return borderLine.GetHeadPosition(out headDirection);
    }

    public Coroutine FadeIn(float duration)
    {
        borderLine.SetOpacity(0);
        bodyLine.SetOpacity(0);
        return StartCoroutine(FadeFromTo(0, 1, duration));
    }

    public Coroutine FadeOut(float duration)
    {
        return StartCoroutine(FadeFromTo(1, 0, duration));
    }

    private IEnumerator FadeFromTo(float from, float to, float duration)
    {
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            var a = Mathf.SmoothStep(from, to, t);
            borderLine.SetOpacity(a);
            bodyLine.SetOpacity(a);
            yield return null;
        }
        borderLine.SetOpacity(to);
        bodyLine.SetOpacity(to);
    }
}
