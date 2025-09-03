using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class SFX : MonoBehaviour
{
    public static SFX Instance;

    [Header("SFX")]
    public EventReference EatApple;
    public EventReference PickupCoin;
    public EventReference Sell;
    public EventReference InsideItem;
    public EventReference SellItem;
    public EventReference Death;
    public EventReference TurnSnake;
    public EventReference SnakeAlive;

    [Header("UI")]
    public EventReference NextDay;
    public EventReference Upgrade;
    public EventReference Button;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
