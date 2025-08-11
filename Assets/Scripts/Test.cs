using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private Grid grid;
    public int width = 10;
    public int height = 10;
    public float cellSize = 1;
    public Vector3 orig;
    // Start is called before the first frame update
    void Start()
    {
        grid = new Grid(width,height,cellSize,orig);
    }
}
