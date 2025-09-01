using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shaker : MonoBehaviour
{
    [SerializeField, Min(0f)]
    private float shakeJitter = 1f;

    [SerializeField, Min(0f)]
    private float shakeDuration = 0.1f;

    [SerializeField, Min(0f)]
    private float shakeSize = 0.1f;

    private bool busy;

    public Coroutine Shake()
    {
        if (busy)
            return StartCoroutine(NullCoroutine());

        busy = true;
        return StartCoroutine(ShakeCoroutine(1, 1f));
    }

    public Coroutine LittleShake()
    {
        if (busy)
            return StartCoroutine(NullCoroutine());

        busy = true;
        return StartCoroutine(ShakeCoroutine(0.5f, 0.25f));
    }

    private IEnumerator ShakeCoroutine(float scaleDuration, float scaleSize)
    {
        var startPosition = transform.localPosition;

        for (float t = 0; t < shakeDuration * scaleDuration; t += Time.deltaTime)
        {
            var progress = t / (shakeDuration * scaleDuration);
            if (progress > 0.5f)
                progress = 0.5f - progress;
            progress *= 2;

            var r = Mathf.PerlinNoise1D(t * shakeJitter * scaleSize);
            float angleRadians = Mathf.Lerp(0, 2 * Mathf.PI, r) * progress;
            transform.localPosition =
                startPosition + new Vector3(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians), 0f).normalized * shakeSize * scaleSize;

            yield return null;
        }

        transform.localPosition = startPosition;
        busy = false;
    }

    private IEnumerator NullCoroutine()
    {
        yield return null;
    }
}
