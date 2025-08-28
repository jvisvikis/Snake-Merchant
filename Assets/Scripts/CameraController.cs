using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance => instance;

    [Header("Shake")]
    [SerializeField]
    private float shakeDuration = 1f;

    [SerializeField]
    private float shakeSize = 1f;

    [Header("Focus")]
    [SerializeField, Min(0f)]
    private float focusPulseDuration = 1f;

    [SerializeField, Min(0f)]
    private float focusZoom = 0.5f;

    [SerializeField, Range(0f, 1f)]
    private float focusPosition = 1f;

    [SerializeField, Min(0f)]
    private float setFocusDuration = 0.3f;

    [SerializeField, Min(0f)]
    private float clearFocusDuration = 0.1f;

    private static CameraController instance = null;
    private Camera cam;

    // Defaults
    private Vector3 defaultPosition;
    private float defaultOrthoSize;

    // Focus
    private Vector3 targetFocusDelta;
    private Vector3 currentFocusDelta;

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

    public void Reset()
    {
        StopAllCoroutines();

        if (currentFocusDelta != Vector3.zero)
        {
            transform.position -= currentFocusDelta;
            currentFocusDelta = Vector3.zero;
            targetFocusDelta = Vector3.zero;
        }

        cam.orthographicSize = defaultOrthoSize;
    }

    public void Shake()
    {
    }

    public void PulseFocus(Vector3 focusPosition)
    {
        StopAllCoroutines();
        StartCoroutine(PulseFocusCoroutine(focusPosition));
    }

    private IEnumerator PulseFocusCoroutine(Vector3 focusPosition)
    {
        var targetPosition = new Vector3(focusPosition.x, focusPosition.y, defaultPosition.z);
        targetFocusDelta = targetPosition - transform.position;

        Coroutine positionCoroutine = null;
        Coroutine orthoCoroutine = null;
        var targetOrthoSize = cam.orthographicSize * focusZoom;

        if (currentFocusDelta != targetFocusDelta)
            positionCoroutine = StartCoroutine(FocusPositionCoroutine(setFocusDuration, targetFocusDelta));

        if (cam.orthographicSize != targetOrthoSize)
            orthoCoroutine = StartCoroutine(FocusOrthoSizeCoroutine(setFocusDuration, targetOrthoSize));

        yield return positionCoroutine;
        yield return orthoCoroutine;

        yield return new WaitForSeconds(focusPulseDuration - setFocusDuration - clearFocusDuration);

        StartCoroutine(FocusPositionCoroutine(clearFocusDuration, ));

        if (cam.orthographicSize != targetOrthoSize)
            StartCoroutine(FocusOrthoSizeCoroutine(clearFocusDuration, targetOrthoSize));
    }

    public void Focus(Vector3 focusPosition)
    {
        StopAllCoroutines();

        var targetPosition = new Vector3(focusPosition.x, focusPosition.y, defaultPosition.z);
        targetFocusDelta = targetPosition - transform.position;
        var targetOrthoSize = cam.orthographicSize * focusZoom;

        if (currentFocusDelta != targetFocusDelta)
            StartCoroutine(FocusPositionCoroutine(setFocusDuration, targetFocusDelta));

        if (cam.orthographicSize != targetOrthoSize)
            StartCoroutine(FocusOrthoSizeCoroutine(setFocusDuration, targetOrthoSize));
    }

    public void ClearFocus()
    {
        StopAllCoroutines();

        var targetPosition = defaultPosition;
        targetFocusDelta = targetPosition - transform.position;
        var targetOrthoSize = defaultOrthoSize;

        if (currentFocusDelta != targetFocusDelta)
            StartCoroutine(FocusPositionCoroutine(clearFocusDuration, targetFocusDelta));

        if (cam.orthographicSize != targetOrthoSize)
            StartCoroutine(FocusOrthoSizeCoroutine(clearFocusDuration, targetOrthoSize));
    }

    private IEnumerator FocusPositionCoroutine(float focusDuration, Vector3 focusDelta)
    {
        yield return null;
        // var prevDelta = Vector3.zero;

        // for (float elapsedTime = 0; elapsedTime < focusDuration; elapsedTime += Time.deltaTime)
        // {
        //     transform.position += Vector3.Slerp(Vector3.zero, focusDelta, elapsedTime / focusDuration) - prevDelta;
        //     yield return null;
        // }

        // transform.position += focusDelta - prevDelta;
    }

    private IEnumerator FocusOrthoSizeCoroutine(float focusDuration, float targetOrthoSize)
    {
        var startOrthoSize = cam.orthographicSize;

        for (float elapsedTime = 0; elapsedTime < focusDuration; elapsedTime += Time.deltaTime)
        {
            cam.orthographicSize = Mathf.SmoothStep(startOrthoSize, targetOrthoSize, elapsedTime / focusDuration);
            yield return null;
        }

        cam.orthographicSize = targetOrthoSize;
    }
}
