using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance => instance;
    private static EconomyManager instance;

    public List<Grid> warehouses = new();
    public List<int> snakeLengthPrices = new();
    public List<int> snakeSpeedPrices = new();
    [Header("Life Settings")]
    [SerializeField]
    private int maxLives;
    [SerializeField]
    private int lifePrice;

    public int TotalCoins => totalCoins;
    public int WarehouseLevel => warehouseLevel;
    public int SnakeSpeedLevel => snakeSpeedLevel;
    public int SnakeLengthLevel => snakeLengthLevel;
    public int Lives => lives;
    public bool IsAlive => lives <= 0;

    private int totalCoins = 999;
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
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void AddCoins(int coins)
    {
        this.totalCoins += coins;
        UIManager.Instance.SetTotalCoinText($"Coins: <color=yellow>{totalCoins}");
    }

    public bool SpendCoins(int value)
    {
        if(value>totalCoins)
        {
            return false;
        }
        totalCoins -= value;
        UIManager.Instance.SetTotalCoinText($"Coins: <color=yellow>{totalCoins}");
        return true;
    }
    #region Upgrades
    //TODO Change method to use warehouse upgrade price
    public bool UpgradeWarehouse(int price)
    {
        if (!WarehouseUpgradeAvailable())
        {
            Debug.LogError("No warehouse upgrade available");
            return false;
        }
        return false;
        //Uncomment when warehouses have prices
        //if (SpendCoins(warehouses[warehouseLevel].price))
        //{
        //    warehouseLevel++;
        //    return true;
        //}
        //else
        //{
        //    Debug.Log("Not enough Coins");
        //    return false;
        //}
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
            UIManager.Instance.SetLengthLevelText(snakeLengthLevel.ToString());
            UIManager.Instance.SetLengthUpgradePrice(GetCurrentSpeedUpgradePrice().ToString());
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
        return warehouseLevel < warehouses.Count - 1;
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
        //Change once warehouse items are in and have prices
        //warehouses[warehouseLevel+1].price
        return -1;
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
        totalCoins = 0;
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

    public bool AddLife()
    {
        if (!LivesUpgradeAvailable())
            return false;
        lives++;
        if (lives >= maxLives)
        {
            UIManager.Instance.SetLifePurchasePrice("-1");
        }
        UIManager.Instance.SetLivesText($"Lives: <color=green>{lives}");
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
