using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelData", menuName = "Snake/Level", order = 0)]
public class LevelData : ScriptableObject
{
    public int Width = 10;
    public int Height = 10;
    public int Cost = 1;
    public int NumCoins = 5;
    public int NumItems = 3;
    public bool Mushrooms = false;
    public ItemDataCollection Items;
}
