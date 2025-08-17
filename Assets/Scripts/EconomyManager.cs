using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance => instance;
    private static EconomyManager instance;

    public List<Grid> warehouses = new();
    [Header("Max Upgrade Levels")]
    public int maxSnakeSpeedLevel;
    public int maxSnakeLengthLevel;
    public int WareHouseLevel => warehouseLevel;
    public int SnakeSpeedLevel => snakeSpeedLevel;
    public int SnakeLengthLevel => snakeLengthLevel;

    private int coins = 0;
    private int warehouseLevel = 0;
    private int snakeSpeedLevel = 0;
    private int snakeLengthLevel = 0;
    private float snakeSpeed = 0;

    private void Awake()
    {
        if(instance == null)
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
        this.coins += coins;
    }

    public bool SpendCoins(int value)
    {
        if(value>coins)
        {
            return false;
        }
        coins -= value;
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
        warehouseLevel++;
    
        return SpendCoins(price);
    }

    public bool UpgradeSpeedLevel()
    {
        if(SnakeSpeedUpgradeAvailable())
        {
            Debug.LogError("Snake speed maxxed out");
            return false;
        }
        snakeSpeedLevel++;

        return true;
    }

    public bool UpgradeSnakeLength()
    {
        if(SnakeLengthUpgradeAvailable())
        {
            Debug.LogError("Snake length maxxed out");
            return false;
        }
        snakeLengthLevel++;

        return true;
    }
    #endregion

    #region Check Availability

    public bool WarehouseUpgradeAvailable()
    {
        return warehouseLevel >= warehouses.Count - 1;
    }

    public bool SnakeLengthUpgradeAvailable()
    {
        return snakeLengthLevel >= maxSnakeLengthLevel;
    }

    public bool SnakeSpeedUpgradeAvailable()
    {
        return snakeSpeedLevel >= maxSnakeSpeedLevel;
    }

    #endregion


    public void BuyObstacleRemoval()
    {
        //TODO remove obstacle
    }

}
