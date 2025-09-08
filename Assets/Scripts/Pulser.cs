using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pulser : MonoBehaviour
{
    [SerializeField, Min(0.1f)]
    private float pulseMagnitude = 1f;

    [SerializeField, Min(0.1f)]
    private float pulsePeriod = 0.1f;

    private Vector3 initScale;

    private void Start()
    {
        initScale = transform.localScale;
    }

    public void StartPulse()
    {
        StopAllCoroutines();
        transform.localScale = initScale;
        StartCoroutine(PulseCoroutine());
    }

    private IEnumerator PulseCoroutine()
    {
        var halfPulsePeriod = pulsePeriod / 2f;
        Debug.Assert(halfPulsePeriod > 0);

        var startScale = transform.localScale;
        var endScale = startScale * pulseMagnitude;

        while (true)
        {
            for (float t = 0; t < halfPulsePeriod; t += Time.deltaTime)
            {
                transform.localScale = VectorUtil.SmoothStep(startScale, endScale, t / halfPulsePeriod);
                yield return null;
            }

            for (float t = 0; t < halfPulsePeriod; t += Time.deltaTime)
            {
                transform.localScale = VectorUtil.SmoothStep(endScale, startScale, t / halfPulsePeriod);
                yield return null;
            }
        }
    }
}
