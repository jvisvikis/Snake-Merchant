using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDataCollection", menuName = "Snake/ItemCollection", order = 0)]
public class ItemDataCollection : ScriptableObject
{
    public List<ItemData> Items = new();
}
