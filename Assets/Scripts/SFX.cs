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

    [Header("Snapshots")]
    public EventReference InsideItemSnapshot;

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
