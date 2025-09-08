using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using Unity.VisualScripting;
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

    [Range(0f, 1f)]
    public float gridAutoSizeScale = 1f;

    // public Vector3 orig;

    [Header("Game variation")]
    public bool onlyCollectSpecificItem = false;
    public bool canExitAtAnyCell = false;
    public bool canEnterAtAnyCell = false;
    public bool mustCompleteItemAfterEntering = false;

    [Header("Game feel/settings")]
    public float initTimeToMove = 0.5f;
    public float minTimeToMove = 0.1f;
    public float timeToMoveReductionPerApple = 0.01f;
    public float timeToDieGrace = 0.1f;
    public int collectionWalkDelay = 3;
    public int bonusPointStack = 5;
    public float timeToMoveReductionPerDay = 0.1f;

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

    [Header("Grid")]
    [SerializeField]
    private GridSquare.TypeSprites[] gridTypeSprites;

    [Header("Bonus")]
    [SerializeField, Min(0f)]
    private float itemBonusMult = 1f;

    [SerializeField, Min(0f)]
    private float timeLeftBonusMult = 1f;

    [SerializeField, Min(0f)]
    private float snakeLengthBonusMult = 1f;

    [SerializeField, Min(0f)]
    private float dayReachedBonusMult = 1f;

    public CameraController.FocusOptions focusSpawn = CameraController.DefaultFocusOptions;
    public CameraController.FocusOptions focusRespawn = CameraController.DefaultFocusOptions;
    public CameraController.FocusOptions focusItem = CameraController.DefaultFocusOptions;
    public CameraController.FocusOptions focusSell = CameraController.DefaultFocusOptions;

    public Grid Grid => grid;
    public Snake Snake => snake;
    public bool SnakeIsMoving => snakeIsMoving;
    public ItemsManager ItemsManager => itemsManager;
    public int Coins => coins;
    public int CoinSpawnCountdown => coinSpawnCountdown;
    public LevelData CurrentLevel => levels[EconomyManager.Instance.WarehouseLevel];
    public Vector2Int CurrentLevelSpawn => currentLevelSpawn;
    public Dictionary<Vector2Int, GridSquare> GridSquares => gridSquares;
    public int CurrentNumParts => currentNumParts;
    public List<ItemData> Items => levelItems;
    public ItemData CurrentItem => itemToCollect;
    public int ItemsSold => itemsSold;

    private Snake snake;
    private Grid grid;
    private Dictionary<Vector2Int, GridSquare> gridSquares = new();
    private ItemsManager itemsManager;
    private float timeToMove;
    private bool snakeIsMoving;

    // private ItemController specificItem;
    private int currentItemBonus = 0;
    private int coins = 0;
    private int itemsSold = 0;
    private int coinSpawnCountdown;
    private int currentDayScore;
    private Vector2Int currentLevelSpawn = Vector2Int.zero;
    private int currentNumParts;
    public List<ItemData> levelItems = new();
    public ItemData itemToCollect;
    public string whyLastItemNotCollected = "";
    public FitToScreen fitToScreen;
    private Coroutine moveSnakeCoroutine;

    public void SnakeDidEatApple()
    {
        currentNumParts++;
    }

    void Awake()
    {
        DayManager.Instance.SetGame(this);
    }

    private float MoveTimeScaleForCurrentDay()
    {
        return Mathf.Pow(1f - timeToMoveReductionPerDay, DayManager.Instance.CurrentDay);
    }

    private float MoveTimeScaleForSnakeLength()
    {
        return Mathf.Pow(1 - timeToMoveReductionPerApple, currentNumParts - 1);
    }

    void Start()
    {
        EconomyManager.Instance.SetupWarehouses(levels);
        CurrentLevel.ParseLayout();
        var orig = new Vector2(CurrentLevel.Width, CurrentLevel.Height) / -2f;
        grid = new Grid(CurrentLevel.Width, CurrentLevel.Height, orig);
        currentNumParts = startNumParts + EconomyManager.Instance.SnakeLengthLevel;
        GenerateLevelItemList();
        SpawnPerRoundObjects(true);
        SetFirstItem();
        timeToMove = initTimeToMove * MoveTimeScaleForCurrentDay() * MoveTimeScaleForSnakeLength();
        Debug.Log($"Start time to move: {timeToMove}");
        moveSnakeCoroutine = StartCoroutine(MoveSnake(currentLevelSpawn, true));
        StartCoroutine(DayManager.Instance.StartDay());
    }

    private void SpawnItemsManager()
    {
        itemsManager = Instantiate(itemsManagerPrefab, transform);
    }

    private void SpawnPerRoundObjects(bool isStart)
    {
        if (isStart)
        {
            CameraController.Instance.Init(grid.Height / 2f / gridAutoSizeScale);
        }
        else
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
        var middleTypeSprites = FindTypeSprites(GridSquare.Type.Middle);

        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                var gridSquare = GameObject.Instantiate(gridSquarePrefab, transform);
                gridSquare.transform.position = grid.GetWorldPos(x, y) + grid.CellCenterOffset();
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

                var typeSprites = FindTypeSprites(gridSquareType);

                gridSquare.Init(
                    cell,
                    new GridSquare.TypeSprite
                    {
                        Type = typeSprites.Type,
                        Sprite = ListUtil.Random(typeSprites.Sprites),
                        Offset = typeSprites.Offset,
                    },
                    middleTypeSprites
                );
                gridSquares[cell] = gridSquare;
            }
        }
    }

    private GridSquare.TypeSprites FindTypeSprites(GridSquare.Type type)
    {
        foreach (var typeSprites in gridTypeSprites)
        {
            if (typeSprites.Type == type)
                return typeSprites;
        }
        Debug.Assert(false);
        return gridTypeSprites[0];
    }

    void SpawnSnake()
    {
        snake = Instantiate(snakePrefab, grid.Orig, Quaternion.identity, null);
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
        controls.PlayerInput.NextDay.performed += OnNextDay;
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
        controls.PlayerInput.MoveVertical.performed -= MoveVertical;
        controls.PlayerInput.MoveHorizontal.performed -= MoveHorizontal;
        controls.PlayerInput.Reset.performed -= OnReset;
        controls.PlayerInput.Pause.performed -= OnPause;
        controls.PlayerInput.NextDay.performed -= OnNextDay;
    }

    private IEnumerator MoveSnake(Vector2Int spawn, bool isStart)
    {
        snakeIsMoving = false;

        yield return CameraController.Instance.SetFocus(isStart ? focusSpawn : focusRespawn, grid.GetWorldPos(spawn) + grid.CellCenterOffset());
        // if (isStart)
        //     yield return new WaitForSeconds(spawnPause);
        AudioManager.Instance.StartMusic();
        yield return CameraController.Instance.ClearFocus(isStart ? focusSpawn : focusRespawn);
        fitToScreen.SetFittingOff();
        snake.ApplyQueuedDirection();

        int didSellPreviousIteration = 0;
        snakeIsMoving = true;

        while (DayManager.Instance.IsPlaying)
        {
            for (float t = 0; t < timeToMove; t += Time.deltaTime)
            {
                snake.SetMoveOffset(t / timeToMove);
                yield return null;
            }

            snake.SetMoveOffset(1);

            if (didSellPreviousIteration > 0)
            {
                snakeIsMoving = false;

                yield return CameraController.Instance.SetFocus(focusSell, grid.GetWorldPos(spawn) + grid.CellCenterOffset());
                var moreBonus = 0;
                while (snake.DespawnNextCarryingItem())
                {
                    DayManager.Instance.OnItemsSold(1);
                    if (moreBonus > 0)
                        AddBonus(moreBonus, !snake.IsCarryingItems());
                    moreBonus += bonusPointStack;
                    yield return new WaitForSeconds(sellPause);
                }
                yield return CameraController.Instance.ClearFocus(focusSell);

                snakeIsMoving = true;
            }

            string whyFail;
            ItemController blockedByItemOnLastMove;
            bool didMove = snake.Move(out didSellPreviousIteration, out whyFail, out blockedByItemOnLastMove);

            // allow grace time if snake is about to die for player to avoid death.
            for (float t = 0; t < timeToDieGrace && !didMove; t += Time.deltaTime)
            {
                yield return null;
                didMove = snake.Move(out didSellPreviousIteration, out whyFail, out blockedByItemOnLastMove);
            }

            if (!didMove)
            {
                snakeIsMoving = false;
                yield return CameraController.Instance.SetFocus(focusRespawn, grid.GetWorldPos(snake.Head) + grid.CellCenterOffset());
                if (blockedByItemOnLastMove != null)
                    blockedByItemOnLastMove.Shake();
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

    public void GenerateLevelItemList()
    {
        levelItems = new();

        while (levelItems.Count < 100)
        {
            // generate 100 items, should be enough for this level though code will generate more
            // just in case. add in blocks of shuffled lists for the current level, but shuffle in 2
            // of each item type to give the opportunity to have the same item twice in a row.
            var block = new List<ItemData>();
            block.AddRange(CurrentLevel.Items.Items);
            block.AddRange(CurrentLevel.Items.Items);
            ListUtil.Shuffle(block);
            levelItems.AddRange(block);
        }
    }

    public void SetFirstItem()
    {
        var collectibleItems = itemsManager.CollectibleItems;

        if (collectibleItems.Count == 0)
        {
            Debug.Log("uhoh there are no items to collect");
            return;
        }

        itemToCollect = ListUtil.Random(collectibleItems).RItemData.ItemData;

        string text = "";
        if (itemToCollect.flavourText.Count >= 1)
            text = ListUtil.Random(itemToCollect.flavourText);

        UIManager.Instance.SetFirstItem(itemToCollect.sprite, text);
    }

    public void ConsumeItem(ItemData itemConsumed)
    {
        if (itemConsumed.IsConsumable)
        {
            if (itemConsumed.IsApple)
                IncreaseSpeed();
            return;
        }

        ItemsManager.SpawnCollectibles(false);
        levelItems.Remove(itemConsumed);

        if (levelItems.Count == 0)
            GenerateLevelItemList();

        SetFirstItem();
        Debug.Log(levelItems.Count);
        //indexToCollect++;

        int textIdx = Random.Range(0, itemToCollect.flavourText.Count);
        if (itemToCollect.flavourText.Count == 0)
        {
            Debug.LogError($"{itemToCollect.name} has no flavour text");
            return;
        }
        UIManager.Instance.SetFirstItem(itemToCollect.sprite, itemToCollect.flavourText[textIdx]);
        UIManager.Instance.DialogueBox.ResetAnimation();
    }

    public void Die(string whyDie)
    {
        Debug.Log($"DYING: {whyDie}");
        RuntimeManager.PlayOneShotAttached(SFX.Instance.Death, snake.gameObject);
        //This will need a revist
        // TODO and when it does, i.e. because snake has more lives, move the camera slowly back to the spawn point.
        // Add a new CameraController.FocusOptions like "focusRespawn" and use that.
        StopAllCoroutines();
        SpawnPerRoundObjects(false);
        // don't add bonus to current day score
        DayManager.Instance.EndDay(currentDayScore, CalculateDayEndBonus(false), true);
    }

    private void MoveVertical(InputAction.CallbackContext callbackContext)
    {
        snake.QueueDirection(new Vector2Int(0, RoundIntValue(callbackContext)), !snakeIsMoving);
    }

    private void MoveHorizontal(InputAction.CallbackContext callbackContext)
    {
        snake.QueueDirection(new Vector2Int(RoundIntValue(callbackContext), 0), !snakeIsMoving);
    }

    private void OnReset(InputAction.CallbackContext callbackContext)
    {
        if (Application.isEditor)
            Die("manual reset");
    }

    private void OnNextDay(InputAction.CallbackContext callbackContext)
    {
        if (Application.isEditor)
        {
            currentDayScore = DayManager.Instance.CurrentTargetScore;
            CheckDaySuccess();
        }
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
        itemsSold += items.Count;
        int collectionSold = 0;
        foreach (var item in items)
        {
            currentDayScore += item.Value;
            collectionSold += item.Value;
        }

        RuntimeManager.PlayOneShotAttached(SFX.Instance.Sell, snake.gameObject);

        ShowCollectionUI(collectionSold);

        // If there is only a single item then CheckDaySuccess happens at the end of the AddBonus calls.
        // Otherwise there will be no AddBonus calls so succeed right now.
        if (items.Count == 1 && CheckDaySuccess())
            return;

        ItemsManager.SpawnCollectibles(true);

        snake.SetSpeedFactor(timeToMove / initTimeToMove);
        CameraController.Instance.SetFocusSpeedScale(timeToMove / initTimeToMove);
    }

    public void IncreaseSpeed()
    {
        timeToMove = Mathf.Max(minTimeToMove, timeToMove - timeToMoveReductionPerApple);
    }

    private void ShowCollectionUI(int sold)
    {
        CollectionWorldUI collectionUI = Instantiate(collectionUIPrefab);
        collectionUI.transform.position = grid.GetWorldPos(currentLevelSpawn + collectionUIOffset);
        collectionUI.SetProfitText($"+{sold}");
        Destroy(collectionUI.gameObject, collectionUI.ClipLength);
        UIManager.Instance.SetCurrentScoreText($"Current: {currentDayScore}");
    }

    private void AddBonus(int addBonus, bool checkSuccess)
    {
        currentItemBonus += addBonus;
        currentDayScore += addBonus;

        ShowCollectionUI(addBonus);

        if (checkSuccess)
            CheckDaySuccess();
    }

    private bool CheckDaySuccess()
    {
        if (currentDayScore < DayManager.Instance.CurrentTargetScore)
        {
            Debug.Log($"check success = false ({currentDayScore} < {DayManager.Instance.CurrentTargetScore}");
            return false;
        }

        Debug.Log("Check success: true!");
        var totalBonus = CalculateDayEndBonus(true);
        currentDayScore += totalBonus;
        Debug.Assert(moveSnakeCoroutine != null);
        StopCoroutine(moveSnakeCoroutine);
        DayManager.Instance.EndDay(currentDayScore, totalBonus, false);
        return true;
    }

    private int CalculateDayEndBonus(bool includeTime)
    {
        var totalItemBonus = (int)Mathf.Floor(currentItemBonus * itemBonusMult);
        var timeBonus = (int)Mathf.Floor(timeLeftBonusMult * DayManager.Instance.TimeLeft);
        var snakeLengthBonus = (int)Mathf.Floor(snakeLengthBonusMult * (snake.Length - 1));
        var dayReachedBonus = (int)Mathf.Floor(dayReachedBonusMult * DayManager.Instance.CurrentDay);

        // yes add the item bonus back to the current day score again - make it mean something to
        // total score otherwise all it does is count towards ending day faster.
        var totalBonus = totalItemBonus + snakeLengthBonus + dayReachedBonus;

        if (includeTime)
            totalBonus += timeBonus;

        Debug.Log($"BONUS: sell {totalItemBonus} + time {timeBonus} + snake {snakeLengthBonus} + day {dayReachedBonus} = {totalBonus}");
        return totalBonus;
    }
}
