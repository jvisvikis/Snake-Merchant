using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance => instance;
    private static EconomyManager instance;

    public int WareHouseLevel => warehouseLevel;

    private int coins = 0;
    private int warehouseLevel = 0;

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

}
