using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    private Grid grid;
    public int width = 10;
    public int height = 10;
    public float cellSize = 1;
    public Vector3 orig;
    public Snake snakePrefab;
    private Snake snake;
    // Start is called before the first frame update
    void Start()
    {
        grid = new Grid(width,height,cellSize,orig);
        snake = Instantiate(snakePrefab, orig, Quaternion.identity, null);
        snake.SetSize(cellSize);
        snake.SetInitialPos(new Vector2(width/2,height/2));
        StartCoroutine(MoveSnake(0.5f));
    }

    public IEnumerator MoveSnake(float timeToMove)
    {
        while (true)
        {
            yield return new WaitForSeconds(timeToMove);
            snake.Move();
        }
    }
}
