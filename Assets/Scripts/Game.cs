using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Game : MonoBehaviour
{
    public InputActions controls;
    public int width = 10;
    public int height = 10;
    public float cellSize = 1;
    public Vector3 orig;
    public Snake snakePrefab;
    private Snake snake;
    private Grid grid;
    // Start is called before the first frame update
    private void Awake()
    {
        controls = new InputActions();
        controls.PlayerInput.MoveVertical.performed += ctx => MoveVertical(ctx);
        controls.PlayerInput.MoveHorizontal.performed += ctx => MoveHorizontal(ctx);
    }
    void Start()
    {
        grid = new Grid(width,height,cellSize,orig);
        snake = Instantiate(snakePrefab, orig, Quaternion.identity, null);
        snake.SetSize(cellSize);
        snake.SetInitialPos(new Vector2(width/2,height/2));
        StartCoroutine(MoveSnake(0.5f));
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    public IEnumerator MoveSnake(float timeToMove)
    {
        while (true)
        {
            yield return new WaitForSeconds(timeToMove);
            snake.Move();
        }
    }

    public void MoveVertical(InputAction.CallbackContext callbackContext)
    {
        
        snake.SetDirection(new Vector2(0, callbackContext.ReadValue<float>()));
    }

    public void MoveHorizontal(InputAction.CallbackContext callbackContext)
    {
        snake.SetDirection(new Vector2(callbackContext.ReadValue<float>(),0));
    }
}
