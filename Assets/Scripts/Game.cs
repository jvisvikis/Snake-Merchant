using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    [Header("Friends")]
    public InputActions controls;
    public Snake snakePrefab;
    public GameObject specificItemParent;
    public ItemController itemControllerPrefab;

    [Header("Grid")]
    public int width = 10;
    public int height = 10;
    public float cellSize = 1;
    public Vector3 orig;
    public Vector2Int initialSnakePos;

    [Header("Game options")]
    public int numItems = 1;
    public bool spawnNewSnakeOnCollection = false;
    public bool collectionWalksOffScreen = false;
    public int collectionWalkDelay = 3;
    public bool onlyCollectSpecificItem = false;

    [Header("Game feel")]
    public float initTimeToMove = 0.5f;
    public float minTimeToMove = 0.1f;
    public float timeToMoveReduction = 0.01f;
    public float timeToDieGrace = 0.1f;

    public Grid Grid => grid;
    public Snake Snake => snake;

    private Snake snake;
    private Grid grid;
    private ItemsManager itemsManager;
    private float timeToMove;
    private ItemController specificItem;

    // Start is called before the first frame update
    private void Awake()
    {
        itemsManager = GetComponent<ItemsManager>();
        timeToMove = initTimeToMove;
    }

    void Start()
    {
        grid = new Grid(width, height, cellSize, orig);
        SpawnSnake();
        StartCoroutine(MoveSnake(timeToMove));
    }

    void SpawnSnake()
    {
        snake = Instantiate(snakePrefab, orig, Quaternion.identity, null);
        snake.SetSize(cellSize);
        snake.SetInitialPos(initialSnakePos);
    }

    private void OnEnable()
    {
        if (controls == null)
            controls = new InputActions();
        controls.PlayerInput.MoveVertical.performed += MoveVertical;
        controls.PlayerInput.MoveHorizontal.performed += MoveHorizontal;
        controls.PlayerInput.Reset.performed += OnReset;
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
        controls.PlayerInput.MoveVertical.performed -= MoveVertical;
        controls.PlayerInput.MoveHorizontal.performed -= MoveHorizontal;
        controls.PlayerInput.Reset.performed -= OnReset;
    }

    public IEnumerator MoveSnake(float timeToMove)
    {
        // Gross hack to make sure that all of the Start() stuff - e.g. items and so on - has been
        // set up before starting to move the snake.
        yield return null;
        MaybeSpawnSpecificItem();

        while (true)
        {
            yield return new WaitForSeconds(timeToMove);
            bool didMove = snake.Move();

            // allow grace time if snake is about to die for player to avoid death.
            for (float t = 0; t < timeToDieGrace && !didMove; t += Time.deltaTime)
            {
                yield return null;
                didMove = snake.Move();
            }

            if (!didMove)
            {
                Die();
            }
            else if (itemsManager.SnakeMoved(specificItem?.ItemData))
            {
                timeToMove = Mathf.Max(minTimeToMove, timeToMove - timeToMoveReduction);

                if (spawnNewSnakeOnCollection)
                {
                    GameObject.Destroy(snake);
                    SpawnSnake();
                }

                MaybeSpawnSpecificItem();
            }
        }
    }

    private void MaybeSpawnSpecificItem()
    {
        if (!onlyCollectSpecificItem)
            return;

        if (specificItem != null)
            GameObject.Destroy(specificItem.gameObject);

        specificItem = Instantiate(itemControllerPrefab, specificItemParent.transform);
        specificItem.SetData(itemsManager.GetRandomExistingCollectibleItem().ItemData);
    }

    public void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void MoveVertical(InputAction.CallbackContext callbackContext)
    {
        snake.SetDirection(new Vector2Int(0, RoundIntValue(callbackContext)));
    }

    private void MoveHorizontal(InputAction.CallbackContext callbackContext)
    {
        snake.SetDirection(new Vector2Int(RoundIntValue(callbackContext), 0));
    }

    private void OnReset(InputAction.CallbackContext callbackContext)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private int RoundIntValue(InputAction.CallbackContext callbackContext)
    {
        return (int)Mathf.Ceil(callbackContext.ReadValue<float>());
    }
}
