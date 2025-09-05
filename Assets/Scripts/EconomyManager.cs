using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance => instance;
    private static EconomyManager instance;

    [Header("Debug")]
    [SerializeField]
    private bool manyCoins;

    [Header("Upgrade Settings")]
    public LevelData[] warehouses;
    public List<int> snakeLengthPrices = new();
    public List<int> snakeSpeedPrices = new();
    [SerializeField]
    private bool singleUpgradePrice;
    [SerializeField]
    private int defaultSinglePrice;
    [SerializeField]
    private float upgradePriceModifier;
    [SerializeField]
    private int maxLengthLevel;

    [Header("Life Settings")]
    [SerializeField]
    private int maxLives;
    [SerializeField]
    private int lifePrice;

    public int TotalCoins => totalCoins;
    public int WarehouseLevel => warehouseLevel;
    public int SnakeSpeedLevel => snakeSpeedLevel;
    public int SnakeLengthLevel => snakeLengthLevel;
    public int CurrentUpgradePrice => currentUpgradePrice;
    public int Lives => lives;
    public bool IsAlive => lives <= 0;
    public int NumOfObstacles => numOfObstacles;

    private int totalCoins = 0;
    private int warehouseLevel = 0;
    private int snakeSpeedLevel = 0;
    private int snakeLengthLevel = 1;
    private int lives = 1;
    private int currentUpgradePrice;
    private int numOfObstacles = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            if (manyCoins)
                totalCoins = 999;
            currentUpgradePrice = defaultSinglePrice;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    public void SetupWarehouses(LevelData[] levels)
    {
        warehouses = levels;
        if(numOfObstacles > warehouses[warehouseLevel].MaxObstacles)
            numOfObstacles = warehouses[warehouseLevel].MaxObstacles;

        warehouses[warehouseLevel].NumRandomObstacles = numOfObstacles;

        if (WarehouseUpgradeAvailable())
        {
            if(totalCoins >= GetCurrentWarehouseUpgradePrice())
                UIManager.Instance.SetEnableWarehouseUpgrade(true);
            else
                UIManager.Instance.SetEnableWarehouseUpgrade(false);
            UIManager.Instance.SetWarehouseUpgradePrice($"{GetCurrentWarehouseUpgradePrice()}");
        }
        else
            UIManager.Instance.SetWarehouseUpgradePrice($"<color=red>MAX");

        UIManager.Instance.SetWarehouseInfo(warehouses[warehouseLevel]); 
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
        if (SpendCoins(warehouses[warehouseLevel+1].Cost, true))
        {
            warehouseLevel++;
            UIManager.Instance.SetWarehouseUpgradePrice($"{GetCurrentWarehouseUpgradePrice()}");
            if (!WarehouseUpgradeAvailable())
            {
                UIManager.Instance.SetEnableWarehouseUpgrade(false);               
            }
            
            UIManager.Instance.SetWarehouseInfo(warehouses[warehouseLevel]);
            
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
        if (SpendCoins(snakeSpeedPrices[snakeSpeedLevel], false))
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
        int spending = singleUpgradePrice ? currentUpgradePrice : snakeLengthPrices[snakeLengthLevel];
        if (SpendCoins(spending, false))
        {
            snakeLengthLevel++;
            if (singleUpgradePrice)
            {
                UIManager.Instance.SetLengthLevelText($"{snakeLengthLevel}");
                UIManager.Instance.SetUpgradePriceText($"{currentUpgradePrice}");
            }
            else
            {
                UIManager.Instance.SetLengthLevelText($"Length: {snakeLengthLevel + 1}<color=green> > {snakeLengthLevel + 2}");
                UIManager.Instance.SetLengthUpgradePrice($"{GetCurrentLengthUpgradePrice()}");
            }
            if (!SnakeLengthUpgradeAvailable())
            {
                if(singleUpgradePrice)
                    UIManager.Instance.SetLengthLevelText("<color=red>MAX");
                else
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
        int spending = singleUpgradePrice ? currentUpgradePrice : lifePrice;
        if (SpendCoins(spending, false))
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
        if (singleUpgradePrice)
            return snakeLengthLevel < maxLengthLevel;
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
    public bool ObstacleUpgradeAvailable()
    {
        return numOfObstacles > 0;
    }
    public bool HasCoinsForUpgrades()
    {
        return currentUpgradePrice <= totalCoins;
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
        snakeLengthLevel = 1;
        lives = 1;
    }

    public void BuyObstacleRemoval()
    {
        if (ObstacleUpgradeAvailable())
        {
            if(SpendCoins(currentUpgradePrice, false))
                numOfObstacles--;
            Debug.Log($"Obstacles:{numOfObstacles}");
            if (!ObstacleUpgradeAvailable())
                UIManager.Instance.SetEnableObstacleUpgrade(false);
        }
    }
    public void AddCoins(int coins)
    {
        this.totalCoins += coins;
        UIManager.Instance.SetTotalCoinText($"<color=yellow>{totalCoins}");
    }
    public bool SpendCoins(int value, bool isWarehouseUpgrade)
    {
        if (value > totalCoins)
        {
            return false;
        }
        totalCoins -= value;
        UIManager.Instance.SetTotalCoinText($" <color=yellow>{totalCoins}");
        if(!isWarehouseUpgrade)
            UpdateSingleUpgradePrice();
       
        if (!HasCoinsForUpgrades())
            UIManager.Instance.SetAllUpgrades(false);
        if(WarehouseUpgradeAvailable() && totalCoins < GetCurrentWarehouseUpgradePrice())
            UIManager.Instance.SetEnableWarehouseUpgrade(false);
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
        UIManager.Instance.SetLivesText($"<color=green>{lives}");
        UIManager.Instance.SetUpgradeLivesText(lives.ToString());
        UIManager.Instance.SetUpgradePriceText($"{currentUpgradePrice}");
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

    public void UpdateSingleUpgradePrice()
    {
        currentUpgradePrice = Mathf.RoundToInt(currentUpgradePrice * upgradePriceModifier);
    }

    public void AddObstacles(int obstaclesToAdd)
    {
        numOfObstacles += obstaclesToAdd;
        UIManager.Instance.SetObstaclesText(obstaclesToAdd.ToString());
    }

}
