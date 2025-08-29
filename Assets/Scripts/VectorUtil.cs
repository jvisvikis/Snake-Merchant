using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class VectorUtil
{
    public static Vector3 SmoothStep(Vector3 a, Vector3 b, float t)
    {
        return new Vector3(Mathf.SmoothStep(a.x, b.x, t), Mathf.SmoothStep(a.y, b.y, t), Mathf.SmoothStep(a.z, b.z, t));
    }
}
