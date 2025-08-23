using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance => instance;
    private static EconomyManager instance;

    public LevelData[] warehouses;
    public List<int> snakeLengthPrices = new();
    public List<int> snakeSpeedPrices = new();
    [Header("Life Settings")]
    [SerializeField]
    private int maxLives;
    [SerializeField]
    private int lifePrice;
    [Header("Debug")]
    [SerializeField]
    private bool manyCoins;


    public int TotalCoins => totalCoins;
    public int WarehouseLevel => warehouseLevel;
    public int SnakeSpeedLevel => snakeSpeedLevel;
    public int SnakeLengthLevel => snakeLengthLevel;
    public int Lives => lives;
    public bool IsAlive => lives <= 0;

    private int totalCoins = 0;
    private int warehouseLevel = 0;
    private int snakeSpeedLevel = 0;
    private int snakeLengthLevel = 0;
    private int lives = 1;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            if (manyCoins)
                totalCoins = 999;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    public void SetupWarehouses(LevelData[] levels)
    {
        warehouses = levels;
        if (WarehouseUpgradeAvailable())
        {
            UIManager.Instance.SetEnableWarehouseUpgrade(true);
            UIManager.Instance.SetWarehouseUpgradePrice($"Upgrade: <color=yellow>{GetCurrentWarehouseUpgradePrice()}<sprite=1>");
            UIManager.Instance.SetWarehouseLevelText($"Level: {warehouseLevel}<color=green> > {warehouseLevel + 1}");
            UIManager.Instance.SetWarehouseInfo(warehouses[warehouseLevel], warehouses[warehouseLevel + 1]);
        }
        else
        {
            UIManager.Instance.SetWarehouseLevelText($"Level: <color=red>MAX");
            UIManager.Instance.SetWarehouseInfo(warehouses[warehouseLevel], null);
        }
    }
    #region Upgrades
    //TODO Change method to use warehouse upgrade price
    public bool UpgradeWarehouse()
    {
        if (!WarehouseUpgradeAvailable())
        {
            Debug.LogError("No warehouse upgrade available");
            return false;
        }
        //Uncomment when warehouses have prices
        if (SpendCoins(warehouses[warehouseLevel].Cost))
        {
            warehouseLevel++;
            UIManager.Instance.SetWarehouseUpgradePrice($"Upgrade: <color=yellow>{GetCurrentWarehouseUpgradePrice()}<sprite=1>");
            if (!WarehouseUpgradeAvailable())
            {
                UIManager.Instance.SetWarehouseLevelText($"Level: <color=red>MAX");
                UIManager.Instance.SetEnableWarehouseUpgrade(false);
                UIManager.Instance.SetWarehouseInfo(warehouses[warehouseLevel], null);
            }
            else
            {
                UIManager.Instance.SetWarehouseLevelText($"Level: {warehouseLevel}<color=green> > {warehouseLevel + 1}");
                UIManager.Instance.SetWarehouseInfo(warehouses[warehouseLevel], warehouses[warehouseLevel + 1]);
            }
                return true;
        }
        else
        {
            Debug.Log("Not enough Coins");
            return false;
        }
    }

    public bool UpgradeSpeedLevel()
    {
        if(!SnakeSpeedUpgradeAvailable())
        {
            Debug.LogError("Snake speed maxxed out");
            return false;
        }
        if (SpendCoins(snakeSpeedPrices[snakeSpeedLevel]))
        {
            snakeSpeedLevel++;
            UIManager.Instance.SetSpeedLevelText(snakeSpeedLevel.ToString());
            UIManager.Instance.SetSpeedUpgradePrice(GetCurrentSpeedUpgradePrice().ToString());
            if (!SnakeSpeedUpgradeAvailable())
            {
                UIManager.Instance.SetEnableSpeedUpgrade(false);
            }
            return true;
        }
        else 
        {
            Debug.Log("Not enough Coins");
            return false;
        }            
    }

    public bool UpgradeSnakeLength()
    {
        if(!SnakeLengthUpgradeAvailable())
        {
            Debug.LogError("Snake length maxxed out");
            return false;
        }
        if (SpendCoins(snakeLengthPrices[snakeLengthLevel]))
        {
            snakeLengthLevel++;
            UIManager.Instance.SetLengthLevelText($"Length: {snakeLengthLevel + 1}<color=green> > {snakeLengthLevel + 2}");
            UIManager.Instance.SetLengthUpgradePrice($"{GetCurrentLengthUpgradePrice()}<sprite=1>");
            if (!SnakeLengthUpgradeAvailable())
            {
                UIManager.Instance.SetLengthLevelText($"Length: <color=red>MAX");
                UIManager.Instance.SetEnableLengthUpgrade(false);
            }
            return true;
        }
        else
        {
            Debug.Log("Not enough Coins");
            return false;
        }
    }

    public bool BuyLife()
    {
        if(!LivesUpgradeAvailable())
        {
            Debug.LogError("Lives maxxed out");
            return false;
        }
        if (SpendCoins(lifePrice))
        {
            AddLife();
            if(!LivesUpgradeAvailable())
            {
                UIManager.Instance.SetEnableLifeUpgrade(false);
            }
            return true;
        }
        else
        {
            Debug.Log("Not enough coins");
            return false;
        }
    }
    #endregion

    #region Check Availability

    public bool WarehouseUpgradeAvailable()
    {
        return warehouseLevel < warehouses.Length - 1;
    }
    public bool SnakeLengthUpgradeAvailable()
    {
        return snakeLengthLevel < snakeLengthPrices.Count - 1;
    }
    public bool SnakeSpeedUpgradeAvailable()
    {
        return snakeSpeedLevel < snakeSpeedPrices.Count - 1;
    }
    public bool LivesUpgradeAvailable()
    {
        return lives < maxLives;
    }
    #endregion

    #region Get Upgrade Prices
    public int GetCurrentWarehouseUpgradePrice()
    {
        if(!WarehouseUpgradeAvailable())
            return -1;
        return warehouses[warehouseLevel+1].Cost;
    }
    public int GetCurrentLengthUpgradePrice()
    {
        if (!SnakeLengthUpgradeAvailable())
            return -1;
        return snakeLengthPrices[snakeLengthLevel+1];
    }
    public int GetCurrentSpeedUpgradePrice()
    {
        if (!SnakeSpeedUpgradeAvailable())
            return -1;
        return snakeSpeedPrices[snakeSpeedLevel+1];
    }
    public int GetLifeUpgradePrice()
    {
        return lifePrice;
    }
    #endregion

    public void Reset()
    {
        totalCoins = manyCoins ? 999 : 0;
        warehouseLevel = 0;
        snakeSpeedLevel = 0;
        snakeLengthLevel = 0;
        lives = 1;
    }

    public void BuyObstacleRemoval()
    {
        //TODO remove obstacle
        Debug.Log("Not implemented yet");
    }
    public void AddCoins(int coins)
    {
        this.totalCoins += coins;
        UIManager.Instance.SetTotalCoinText($"<sprite=1> <color=yellow>{totalCoins}");
    }
    public bool SpendCoins(int value)
    {
        if (value > totalCoins)
        {
            return false;
        }
        totalCoins -= value;
        UIManager.Instance.SetTotalCoinText($"<sprite=1> <color=yellow>{totalCoins}");
        return true;
    }
    public bool AddLife()
    {
        if (!LivesUpgradeAvailable())
            return false;
        lives++;
        if (lives >= maxLives)
        {
            UIManager.Instance.SetLifePurchasePrice("-1");
        }
        UIManager.Instance.SetLivesText($"<sprite=0> <color=green>{lives}");
        return true;
    }
    public void RemoveLife()
    {
        lives--;
        if (lives <= 0)
        {
            DayManager.Instance.Reset();
            Reset();
        }
    }

}
