using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    private int width;
    private int height;
    private float cellSize;
    private int[,] gridArray;
    private Vector3 orig;

    public Grid (int width, int height, float cellSize, Vector3 orig)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        gridArray = new int[width,height];
        this.orig = orig;

        for (int row = 0; row < width; row++)
        {
            for(int col = 0; col < height; col++)
            {
                Debug.DrawLine(GetWorldPos(row,col), GetWorldPos(row+1,col),Color.white, Mathf.Infinity);
                Debug.DrawLine(GetWorldPos(row,col), GetWorldPos(row, col+1), Color.white,Mathf.Infinity);
            }
        }
        Debug.DrawLine(GetWorldPos(0,height),GetWorldPos(width,height),Color.white,Mathf.Infinity);
        Debug.DrawLine(GetWorldPos(width,height), GetWorldPos(width, 0),Color.white,Mathf.Infinity);
    }

    public Vector3 GetWorldPos(int x,int y)
    {
        return new Vector3(x * cellSize, y * cellSize) + orig;
    }
}
