using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Game : MonoBehaviour
{
    [Header("Friends")]
    public InputActions controls;
    public Snake snakePrefab;

    // public GameObject specificItemParent;
    public ItemController itemControllerPrefab;
    public GridSquare gridSquarePrefab;
    public CollectionWorldUI collectionUIPrefab;
    public Vector2Int collectionUIOffset;
    public ItemsManager itemsManagerPrefab;

    [Header("Levels")]
    [SerializeField]
    private LevelData[] levels;

    [Header("Grid")]
    public float cellSize = 1;

    [Range(0f, 1f)]
    public float gridAutoSizeScale = 1f;

    // public Vector3 orig;

    [Header("Game variation")]
    public bool onlyCollectSpecificItem = false;
    public bool canExitAtAnyCell = false;
    public bool canEnterAtAnyCell = false;
    // public bool canCarryMultipleItems = false;
    public bool mustCompleteItemAfterEntering = false;

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
    [SerializeField]
    private int startNumParts = 3;

    [Header("Camera")]
    [Min(0f)]
    public float spawnPause;

    [Min(0f)]
    public float sellPause;

    [Header("Animations")]
    [Min(0f)]
    public float itemBloopTime = 0.2f;

    public CameraController.FocusOptions focusSpawn = CameraController.DefaultFocusOptions;
    public CameraController.FocusOptions focusItem = CameraController.DefaultFocusOptions;
    public CameraController.FocusOptions focusSell = CameraController.DefaultFocusOptions;

    public Grid Grid => grid;
    public Snake Snake => snake;
    public ItemsManager ItemsManager => itemsManager;
    public int Coins => coins;
    public int CoinSpawnCountdown => coinSpawnCountdown;
    public LevelData CurrentLevel => levels[EconomyManager.Instance.WarehouseLevel];
    public Vector2Int CurrentLevelSpawn => currentLevelSpawn;
    public Dictionary<Vector2Int, GridSquare> GridSquares => gridSquares;
    public int CurrentNumParts => currentNumParts;
    public ItemData CurrentItem => items[indexToCollect] ? items[indexToCollect] : null;

    private Snake snake;
    private Grid grid;
    private Dictionary<Vector2Int, GridSquare> gridSquares = new();
    private ItemsManager itemsManager;
    private float timeToMove;

    // private ItemController specificItem;
    private int bonus = 0;
    private int coins = 0;
    private int itemsSold = 0;
    private int coinSpawnCountdown;
    private int currentDayScore;
    private Vector2Int currentLevelSpawn = Vector2Int.zero;
    private int currentNumParts;
    public List<ItemData> items;
    public int indexToCollect;

    public void SnakeDidEatApple()
    {
        currentNumParts++;
    }

    void Start()
    {
        CurrentLevel.ParseLayout();
        EconomyManager.Instance.SetupWarehouses(levels);
        var orig = new Vector2(CurrentLevel.Width, CurrentLevel.Height) * cellSize / -2f;
        grid = new Grid(CurrentLevel.Width, CurrentLevel.Height, cellSize, orig);
        currentNumParts = startNumParts + EconomyManager.Instance.SnakeLengthLevel;
        timeToMove = initTimeToMove - timeToMoveReduction * EconomyManager.Instance.SnakeSpeedLevel;
        SetItemList();
        SpawnPerRoundObjects(true);
        StartCoroutine(MoveSnake(currentLevelSpawn));
        StartCoroutine(DayManager.Instance.StartDay());
    }

    private void SpawnItemsManager()
    {
        itemsManager = Instantiate(itemsManagerPrefab, transform);
    }

    private void SpawnPerRoundObjects(bool isStart)
    {
        if (!isStart)
        {
            DestroyImmediate(snake.gameObject);
            DestroyImmediate(itemsManager.gameObject);
            foreach (var gridSquare in gridSquares.Values)
                DestroyImmediate(gridSquare.gameObject);
            gridSquares.Clear();
        }
        currentLevelSpawn = GetSpawnPoint(CurrentLevel);
        SpawnGrid();
        coinSpawnCountdown = coinsFirstSpawnTurns;
        CameraController.Instance.Init(grid.Height * grid.CellSize / 2f / gridAutoSizeScale);
        SpawnSnake();
        SpawnItemsManager();
        itemsManager.LoadLevel(this);
    }

    private Vector2Int GetSpawnPoint(LevelData levelData)
    {
        if (levelData.HasSpawnPoint)
            return levelData.SpawnPoint;
        // only randomly spawn in the left half of the board so that the snake doesn't immediately
        // spawn next to the right wall and die.
        return new Vector2Int(Random.Range(1, grid.Width / 2 - 1), Random.Range(1, grid.Height - 1));
    }

    private void Update()
    {
        grid.DrawGrid();
    }

    private void SpawnGrid()
    {
        Debug.Assert(gridSquares.Count == 0);

        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                var gridSquare = GameObject.Instantiate(gridSquarePrefab, transform);
                gridSquare.transform.position = grid.GetWorldPos(x, y) + grid.CellCenterOffset();
                gridSquare.transform.localScale = cellSize * Vector3.one;
                var cell = new Vector2Int(x, y);
                var gridSquareType = GridSquare.Type.Middle;
                if (x == 0 && y == 0)
                    gridSquareType = GridSquare.Type.BottomLeft;
                else if (x == 0 && y == grid.Height - 1)
                    gridSquareType = GridSquare.Type.TopLeft;
                else if (x == 0)
                    gridSquareType = GridSquare.Type.Left;
                else if (y == 0 && x == grid.Width - 1)
                    gridSquareType = GridSquare.Type.BottomRight;
                else if (y == 0)
                    gridSquareType = GridSquare.Type.Bottom;
                else if (x == grid.Width - 1 && y == grid.Height - 1)
                    gridSquareType = GridSquare.Type.TopRight;
                else if (x == grid.Width - 1)
                    gridSquareType = GridSquare.Type.Right;
                else if (y == grid.Height - 1)
                    gridSquareType = GridSquare.Type.Top;
                else if (cell == currentLevelSpawn)
                    gridSquareType = GridSquare.Type.Spawn;
                gridSquare.Init(cell, gridSquareType);
                gridSquares[cell] = gridSquare;
            }
        }
    }

    void SpawnSnake()
    {
        snake = Instantiate(snakePrefab, grid.Orig, Quaternion.identity, null);
        snake.SetSize(cellSize);
        snake.Init(currentLevelSpawn);
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

    public IEnumerator MoveSnake(Vector2Int spawn)
    {
        yield return CameraController.Instance.SetFocus(focusSpawn, grid.GetWorldPos(spawn) + grid.CellCenterOffset());
        yield return new WaitForSeconds(spawnPause);
        yield return CameraController.Instance.ClearFocus(focusSpawn);

        snake.ApplyQueuedDirection();

        bool didSellPreviousIteration = false;

        while (DayManager.Instance.IsPlaying)
        {
            for (float t = 0; t < timeToMove; t += Time.deltaTime)
            {
                snake.SetMoveOffset(t / timeToMove);
                yield return null;
            }

            snake.SetMoveOffset(1);

            if (didSellPreviousIteration)
            {
                yield return CameraController.Instance.SetFocus(focusSell, grid.GetWorldPos(spawn) + grid.CellCenterOffset());
                yield return new WaitForSeconds(sellPause);
                while (snake.DespawnNextCarryingItem())
                    yield return new WaitForSeconds(sellPause);
                yield return CameraController.Instance.ClearFocus(focusSell);
            }

            string whyFail = "";
            bool didMove = snake.Move(out didSellPreviousIteration, out whyFail);

            // allow grace time if snake is about to die for player to avoid death.
            for (float t = 0; t < timeToDieGrace && !didMove; t += Time.deltaTime)
            {
                yield return null;
                didMove = snake.Move(out didSellPreviousIteration, out whyFail);
            }

            if (!didMove)
            {
                // TODO: (re)spawn animation here
                yield return CameraController.Instance.Shake();
                Die(whyFail);
            }
            else
            {
                itemsManager.SnakeMoved(); // specificItem?.RItemData?.ItemData);
            }

            coinSpawnCountdown--;

            if (coinSpawnCountdown == 0)
            {
                itemsManager.RespawnCoins();
                coinSpawnCountdown = coinsSpawnTurns;
            }
        }
    }

    public void SetItemList()
    {
        items = new();
        int currentValue = 0;
        while (currentValue < DayManager.Instance.CurrentTargetScore)
        {
            int index = 2 + Random.Range(0, CurrentLevel.Items.Items.Count - 2);
            items.Add(CurrentLevel.Items.Items[index]);
            currentValue += CurrentLevel.Items.Items[index].Value;
        }
        // UIManager.Instance.SetFirstItem(items[indexToCollect].sprite, items[indexToCollect].flavourText[Random.Range(0, items[indexToCollect].flavourText.Count)]);
    }

    public void ConsumeItem(ItemData itemConsumed)
    {
        if (itemConsumed.IsConsumable)
            return;

        ItemsManager.SpawnCollectibles();
        items.Remove(itemConsumed);
        if (items.Count == 0)
            return;
        indexToCollect = Random.Range(0, items.Count);
        UIManager.Instance.SetFirstItem(items[indexToCollect].sprite, items[indexToCollect].flavourText[Random.Range(0, items[indexToCollect].flavourText.Count)]);
        UIManager.Instance.DialogueBox.ResetAnimation();
    }

    public void Die(string whyDie)
    {
        Debug.Log($"DYING: {whyDie}");
        //This will need a revist
        // TODO and when it does, i.e. because snake has more lives, move the camera slowly back to the spawn point.
        // Add a new CameraController.FocusOptions like "focusRespawn" and use that.
        StopAllCoroutines();
        SpawnPerRoundObjects(false);
        DayManager.Instance.ResetDay();
        StartCoroutine(MoveSnake(currentLevelSpawn));
        StartCoroutine(DayManager.Instance.StartDay());
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
        Die("manual reset");
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
        // Debug.Assert(specificItem != null);
        // Debug.Assert(specificItem.RItemData != null);

        itemsSold += items.Count;
        float collectionSold = 0;
        float collectionCount = 0;
        foreach (var item in items)
        {
            currentDayScore += item.Value;
            collectionSold += item.Value;
            collectionCount++;
            if (collectionCount > 1)
            {
                bonus += 10;
            }
        }
        currentDayScore += bonus;
        collectionSold += bonus;
        CollectionWorldUI collectionUI = Instantiate(collectionUIPrefab);
        collectionUI.transform.position = grid.GetWorldPos(currentLevelSpawn + collectionUIOffset);
        collectionUI.SetProfitText($"${collectionSold}");
        Destroy(collectionUI.gameObject, collectionUI.ClipLength);

        UIManager.Instance.SetCurrentScoreText($"Current: {currentDayScore}");

        if (DayManager.Instance.CurrentTargetScore <= currentDayScore)
        {
            DayManager.Instance.EndDay(currentDayScore, bonus, coins, itemsSold);
            return;
        }

        ItemsManager.SpawnCollectibles();

        // if (!canCarryMultipleItems)
        // {
        //     // If only carrying a single item, respawn that item when it's sold.
        //     Debug.Assert(items.Count == 1);
        //     ItemsManager.SpawnRandomNonExistentCollectibleItem();
        //     // RefreshSpecificItem();
        // }

        timeToMove = Mathf.Max(minTimeToMove, timeToMove - timeToMoveReduction);
        CameraController.Instance.SetFocusSpeedScale(timeToMove / initTimeToMove);
    }
}
