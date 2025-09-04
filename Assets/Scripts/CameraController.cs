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

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private Shaker camShaker;

    private static CameraController instance = null;
    private float focusSpeedScale = 1f;

    // Focus
    private Vector3 defaultPosition;
    private float defaultOrthoSize;

    void Awake()
    {
        if (instance == null)
            instance = this;
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
        transform.position = defaultPosition;
        cam.orthographicSize = defaultOrthoSize;
    }

    public void SetFocusSpeedScale(float scale)
    {
        focusSpeedScale = scale;
    }

    public Coroutine Shake()
    {
        return camShaker.Shake();
    }

    public Coroutine LittleShake()
    {
        return camShaker.LittleShake();
    }

    public Coroutine SetFocus(FocusOptions opts, Vector3 focusPosition)
    {
        StopAllCoroutines();

        var targetPosition = new Vector3(focusPosition.x, focusPosition.y, transform.position.z);
        var targetPositionDelta = targetPosition - transform.position;
        targetPosition = transform.position + targetPositionDelta * opts.FocusPosition;
        var targetOrthoSize = defaultOrthoSize * opts.FocusZoom;

        if (opts.SetFocusDuration == 0)
        {
            transform.position = targetPosition;
            cam.orthographicSize = targetOrthoSize;
            return StartCoroutine(NullCoroutine());
        }

        return StartCoroutine(FocusCoroutine(opts.SetFocusDuration, targetPosition, targetOrthoSize));
    }

    private IEnumerator NullCoroutine()
    {
        yield return null;
    }

    public Coroutine ClearFocus(FocusOptions opts)
    {
        StopAllCoroutines();

        if (opts.ClearFocusDuration == 0)
        {
            transform.position = defaultPosition;
            cam.orthographicSize = defaultOrthoSize;
            return StartCoroutine(NullCoroutine());
        }

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
    }
}
