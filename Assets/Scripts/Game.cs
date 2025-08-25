using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Game : MonoBehaviour
{
    [Header("Friends")]
    public InputActions controls;
    public Snake snakePrefab;
    public GameObject specificItemParent;
    public ItemController itemControllerPrefab;

    [Header("Levels")]
    [SerializeField]
    private LevelData[] levels;

    [Header("Grid")]
    public float cellSize = 1;

    // public Vector3 orig;

    [Header("Game variation")]
    public bool onlyCollectSpecificItem = false;
    public bool mustHaveExactLengthToCollectItem = false;
    public bool canExitAtAnyCell = false;
    public bool canCarryMultipleItems = false;

    [Header("Game feel/settings")]
    public float initTimeToMove = 0.5f;
    public float minTimeToMove = 0.1f;
    public float timeToMoveReduction = 0.01f;
    public float timeToDieGrace = 0.1f;
    public int collectionWalkDelay = 3;

    [Header("Coins")]
    public int coinsSpawnTurns = 15;
    public int coinsFirstSpawnTurns = 5;

    [Header("Snake")]
    public int startNumParts = 3;

    public Grid Grid => grid;
    public Snake Snake => snake;
    public ItemsManager ItemsManager => itemsManager;
    public int Coins => coins;
    public int CoinSpawnCountdown => coinSpawnCountdown;
    public LevelData CurrentLevel => levels[EconomyManager.Instance.WarehouseLevel];

    private Snake snake;
    private Grid grid;
    private ItemsManager itemsManager;
    private float timeToMove;
    private ItemController specificItem;
    private int bonus = 0;
    private int coins = 0;
    private int itemsSold = 0;
    private int coinSpawnCountdown;
    private int currentDayScore;
    //private int currentLevelIndex = 0;

    // Start is called before the first frame update
    private void Awake()
    {
        itemsManager = GetComponent<ItemsManager>();
        //timeToMove = initTimeToMove;
    }

    void Start()
    {
        var chosenLevel = levels[EconomyManager.Instance.WarehouseLevel];
        EconomyManager.Instance.SetupWarehouses(levels);
        var orig = new Vector2(chosenLevel.Width, chosenLevel.Height) * cellSize / -2f;
        grid = new Grid(chosenLevel.Width, chosenLevel.Height, cellSize, orig);
        coinSpawnCountdown = coinsFirstSpawnTurns;
        startNumParts += EconomyManager.Instance.SnakeLengthLevel;
        timeToMove = initTimeToMove - timeToMoveReduction * EconomyManager.Instance.SnakeSpeedLevel;
        SpawnSnake();
        itemsManager.LoadLevel();
        StartCoroutine(MoveSnake());
        StartCoroutine(DayManager.Instance.StartDay());
    }

    private void Update()
    {
        grid.DrawGrid();
    }

    void SpawnSnake()
    {
        snake = Instantiate(snakePrefab, grid.Orig, Quaternion.identity, null);
        snake.SetSize(cellSize);
        snake.Init(new Vector2Int(0, 0));
    }

    private void OnEnable()
    {
        if (controls == null)
            controls = new InputActions();
        controls.PlayerInput.MoveVertical.performed += MoveVertical;
        controls.PlayerInput.MoveHorizontal.performed += MoveHorizontal;
        controls.PlayerInput.Reset.performed += OnReset;
        controls.PlayerInput.Pause.performed += OnPause;
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
        controls.PlayerInput.MoveVertical.performed -= MoveVertical;
        controls.PlayerInput.MoveHorizontal.performed -= MoveHorizontal;
        controls.PlayerInput.Reset.performed -= OnReset;
        controls.PlayerInput.Pause.performed -= OnPause;
    }

    public IEnumerator MoveSnake()
    {
        // Gross hack to make sure that all of the Start() stuff - e.g. items and so on - has been
        // set up before starting to move the snake.
        yield return null;

        RefreshSpecificItem();

        while (DayManager.Instance.IsPlaying)
        {
            for (float t = 0; t < timeToMove; t += Time.deltaTime)
            {
                snake.SetMoveOffset(t / timeToMove);
                yield return null;
            }

            snake.SetMoveOffset(1);
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
            else
            {
                itemsManager.SnakeMoved(specificItem?.RItemData?.ItemData);
            }

            coinSpawnCountdown--;

            if (coinSpawnCountdown == 0)
            {
                itemsManager.RespawnCoins();
                coinSpawnCountdown = coinsSpawnTurns;
            }
        }
    }

    public void RefreshSpecificItem()
    {
        if (specificItem != null)
            GameObject.Destroy(specificItem.gameObject);

        specificItem = Instantiate(itemControllerPrefab, specificItemParent.transform);
        specificItem.SetData(itemsManager.GetRandomExistingCollectibleItem().RItemData);
        specificItem.SetFloating();
        UIManager.Instance.SetFirstItemImage(specificItem.RItemData.Sprite);
    }

    public void Die()
    {
        //This will need a revist
        DayManager.Instance.ResetDay();
    }

    private void MoveVertical(InputAction.CallbackContext callbackContext)
    {
        snake.QueueDirection(new Vector2Int(0, RoundIntValue(callbackContext)));
    }

    private void MoveHorizontal(InputAction.CallbackContext callbackContext)
    {
        snake.QueueDirection(new Vector2Int(RoundIntValue(callbackContext), 0));
    }

    private void OnReset(InputAction.CallbackContext callbackContext)
    {
        EconomyManager.Instance.Reset();
        DayManager.Instance.Reset();
    }

    private void OnPause(InputAction.CallbackContext callbackContext)
    {
        Time.timeScale = 1f - Time.timeScale;
    }

    private int RoundIntValue(InputAction.CallbackContext callbackContext)
    {
        return (int)Mathf.Ceil(callbackContext.ReadValue<float>());
    }

    public void AddCoin()
    {
        coins++;
        EconomyManager.Instance.AddCoins(1);
    }

    public void OnItemsSold(List<ItemData> items)
    {
        Debug.Assert(specificItem != null);
        Debug.Assert(specificItem.RItemData != null);

        itemsSold += items.Count;

        foreach (var item in items)
            currentDayScore += item.Value;

        UIManager.Instance.SetCurrentScoreText($"Current: {currentDayScore}");

        if (DayManager.Instance.CurrentTargetScore <= currentDayScore)
        {
            DayManager.Instance.EndDay(currentDayScore,bonus,coins,itemsCollected);
            return;
        }

        if (!canCarryMultipleItems)
        {
            // If only carrying a single item, respawn that item when it's sold.
            Debug.Assert(items.Count == 1);
            ItemsManager.SpawnRandomNonExistentCollectibleItem();
            RefreshSpecificItem();
        }

        timeToMove = Mathf.Max(minTimeToMove, timeToMove - timeToMoveReduction);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 500, 500), $"Coins: {coins}\nItems: {itemsSold}\nSpeed: {timeToMove:F3}s");
    }
}
