using System.Collections.Generic;
using UnityEngine;

public class ListUtil
{
    public static void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count - 1; i++)
        {
            var r = Random.Range(i, list.Count);
            if (i != r)
                (list[i], list[r]) = (list[r], list[i]);
        }
    }
}
