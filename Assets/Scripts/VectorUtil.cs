using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class VectorUtil
{
    public static Vector3 SmoothStep(Vector3 a, Vector3 b, float t)
    {
        return new Vector3(Mathf.SmoothStep(a.x, b.x, t), Mathf.SmoothStep(a.y, b.y, t), Mathf.SmoothStep(a.z, b.z, t));
    }

    public static Color SmoothStep(Color a, Color b, float t)
    {
        return new Color(Mathf.SmoothStep(a.r, b.r, t), Mathf.SmoothStep(a.g, b.g, t), Mathf.SmoothStep(a.b, b.b, t), Mathf.SmoothStep(a.a, b.a, t));
    }

    public static Color SetAlpha(Color c, float alpha)
    {
        c.a = alpha;
        return c;
    }
}
