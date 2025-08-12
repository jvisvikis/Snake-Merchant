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
    public float timeToMove = 0.5f;
    public Vector3 orig;
    public Snake snakePrefab;
    private Snake snake;
    private Grid grid;
    private ItemsManager itemsManager;

    public Grid Grid => grid;
    public Snake Snake => snake;

    // Start is called before the first frame update
    private void Awake()
    {
        controls = new InputActions();
        controls.PlayerInput.MoveVertical.performed += ctx => MoveVertical(ctx);
        controls.PlayerInput.MoveHorizontal.performed += ctx => MoveHorizontal(ctx);
        itemsManager = GetComponent<ItemsManager>();
    }

    void Start()
    {
        grid = new Grid(width, height, cellSize, orig);
        snake = Instantiate(snakePrefab, orig, Quaternion.identity, null);
        snake.SetSize(cellSize);
        snake.SetInitialPos(new Vector2Int(width / 2, height / 2));
        StartCoroutine(MoveSnake(timeToMove));
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
            if (snake.Move())
                itemsManager.SnakeMoved();
        }
    }

    public void MoveVertical(InputAction.CallbackContext callbackContext)
    {
        snake.SetDirection(new Vector2Int(0, RoundIntValue(callbackContext)));
    }

    public void MoveHorizontal(InputAction.CallbackContext callbackContext)
    {
        snake.SetDirection(new Vector2Int(RoundIntValue(callbackContext), 0));
    }

    private int RoundIntValue(InputAction.CallbackContext callbackContext)
    {
        return (int)Mathf.Ceil(callbackContext.ReadValue<float>());
    }
}
