using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ListUtil
{
    public static void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count - 1; i++)
        {
            var r = UnityEngine.Random.Range(i, list.Count);
            if (i != r)
                (list[i], list[r]) = (list[r], list[i]);
        }
    }

    public static T Random<T>(List<T> list)
    {
        Debug.Assert(list.Count > 0);
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    public static string ArrayToString<T>(T[] list, string delim = ",")
    {
        StringBuilder s = new StringBuilder();
        s.Append($"[");
        foreach (var elem in list)
        {
            if (s.Length == 1)
                s.Append($"{list.Length}:");
            else
                s.Append(delim);
            s.Append(elem.ToString());
        }
        s.Append("]");
        return s.ToString();
    }

    public static string ListToString<T>(List<T> list, string delim = ",")
    {
        StringBuilder s = new StringBuilder();
        s.Append($"[");
        foreach (var elem in list)
        {
            if (s.Length == 1)
                s.Append($"{list.Count}:");
            else
                s.Append(delim);
            s.Append(elem.ToString());
        }
        s.Append("]");
        return s.ToString();
    }
}
