using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Serializable]
    public struct FocusOptions
    {
        [Range(0f, 1f)]
        public float FocusZoom;

        [Range(0f, 1f)]
        public float FocusPosition;

        public float SetFocusDuration;

        public float ClearFocusDuration;
    }

    public static FocusOptions DefaultFocusOptions =
        new()
        {
            FocusZoom = 0.9f,
            FocusPosition = 1f,
            SetFocusDuration = 1f,
            ClearFocusDuration = 0.5f,
        };

    public static CameraController Instance => instance;

    [Header("Shake")]
    [SerializeField, Min(0f)]
    private float shakeJitter = 1f;

    [SerializeField, Min(0f)]
    private float shakeDuration = 0.1f;

    [SerializeField, Min(0f)]
    private float shakeSize = 0.1f;

    private static CameraController instance = null;
    private Camera cam;
    private bool busy;
    private float focusSpeedScale = 1f;

    // Focus
    private Vector3 defaultPosition;
    private float defaultOrthoSize;

    void Awake()
    {
        if (instance == null)
            instance = this;
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        defaultPosition = transform.position;
        defaultOrthoSize = cam.orthographicSize;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public void Init(float orthoSize)
    {
        defaultOrthoSize = orthoSize;
        Reset();
    }

    public void SetFocusSpeedScale(float scale)
    {
        focusSpeedScale = scale;
    }

    public void Reset()
    {
        Debug.Log("Resetting camera");
        StopAllCoroutines();

        transform.position = defaultPosition;
        cam.orthographicSize = defaultOrthoSize;

        busy = false;
    }

    public Coroutine Shake()
    {
        if (busy)
        {
            Reset();
            return StartCoroutine(NullCoroutine());
        }

        busy = true;
        return StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        var startPosition = transform.position;

        for (float t = 0; t < shakeDuration; t += Time.deltaTime)
        {
            var progress = t / shakeDuration;
            if (progress > 0.5f)
                progress = 0.5f - progress;
            progress *= 2;

            var r = Mathf.PerlinNoise1D(t * shakeJitter);
            float angleRadians = Mathf.Lerp(0, 2 * Mathf.PI, r) * progress;
            transform.position = startPosition + new Vector3(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians), 0f).normalized * shakeSize;

            yield return null;
        }

        transform.position = defaultPosition;
        busy = false;
    }

    public Coroutine SetFocus(FocusOptions opts, Vector3 focusPosition)
    {
        if (busy)
        {
            Reset();
            return StartCoroutine(NullCoroutine());
        }

        var targetPosition = new Vector3(focusPosition.x, focusPosition.y, transform.position.z);
        var targetPositionDelta = targetPosition - transform.position;
        targetPosition = transform.position + targetPositionDelta * opts.FocusPosition;
        var targetOrthoSize = cam.orthographicSize * opts.FocusZoom;

        if (opts.SetFocusDuration == 0)
        {
            transform.position = targetPosition;
            cam.orthographicSize = targetOrthoSize;
            return StartCoroutine(NullCoroutine());
        }

        busy = true;

        StopAllCoroutines();
        return StartCoroutine(FocusCoroutine(opts.SetFocusDuration, targetPosition, targetOrthoSize));
    }

    private IEnumerator NullCoroutine()
    {
        yield return null;
    }

    public Coroutine ClearFocus(FocusOptions opts)
    {
        if (busy)
        {
            Reset();
            return StartCoroutine(NullCoroutine());
        }

        if (opts.ClearFocusDuration == 0)
        {
            transform.position = defaultPosition;
            cam.orthographicSize = defaultOrthoSize;
            return StartCoroutine(NullCoroutine());
        }

        busy = true;

        StopAllCoroutines();
        return StartCoroutine(FocusCoroutine(opts.ClearFocusDuration, defaultPosition, defaultOrthoSize));
    }

    private IEnumerator FocusCoroutine(float focusDuration, Vector3 targetPosition, float targetOrthoSize)
    {
        var startPosition = transform.position;
        var startOrthoSize = cam.orthographicSize;

        for (float t = 0; t < focusDuration * focusSpeedScale; t += Time.deltaTime)
        {
            var progress = t / (focusDuration * focusSpeedScale);
            transform.position = VectorUtil.SmoothStep(startPosition, targetPosition, progress);
            cam.orthographicSize = Mathf.SmoothStep(startOrthoSize, targetOrthoSize, progress);
            yield return null;
        }

        transform.position = targetPosition;
        cam.orthographicSize = targetOrthoSize;

        busy = false;
    }
}
