using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeEye : MonoBehaviour
{
    [Header("Sizes")]
    [SerializeField, Range(0, 0.5f)]
    private float borderSize = 0.1f;

    [SerializeField, Range(0.01f, 1f)]
    private float pupilSize = 0.1f;

    [Header("Movement")]
    [SerializeField, Range(0f, 1f)]
    private float lookDirectionRange = 0.3f;

    [SerializeField]
    private float lookDirectionTime = 0.1f;

    [Header("Objects")]
    [SerializeField]
    private GameObject border;

    [SerializeField]
    private GameObject inner;

    [SerializeField]
    private GameObject pupil;

    [SerializeField]
    private bool updateSizeOnUpdate = false;

    private Vector3 targetLookDirection = Vector3.up;
    private Vector3 lookDirection = Vector3.up;

    private void Start()
    {
        SetSizes();
        Draw();
    }

    private void OnEnable()
    {
        StartCoroutine(UpdateLookDirectionLoop());
    }

    private IEnumerator UpdateLookDirectionLoop()
    {
        while (true)
        {
            while (targetLookDirection == lookDirection)
                yield return null;

            var startLookDirection = lookDirection;
            var savedTargetLookDirection = targetLookDirection;

            for (float t = 0; t < lookDirectionTime; t += Time.deltaTime)
            {
                lookDirection = Vector3.Slerp(startLookDirection, savedTargetLookDirection, t / lookDirectionTime).normalized;
                yield return null;
            }

            lookDirection = savedTargetLookDirection.normalized;
        }
    }

    public void SetLookDirection(Vector3 direction)
    {
        targetLookDirection = direction.normalized;
    }

    private void Update()
    {
        if (updateSizeOnUpdate)
            SetSizes();

        Draw();
    }

    private void SetSizes()
    {
        border.transform.localScale = Vector3.one;
        inner.transform.localScale = Vector3.one * (1f - borderSize);
        pupil.transform.localScale = Vector3.one * pupilSize;
    }

    private void Draw()
    {
        pupil.transform.position = inner.transform.position + lookDirection * lookDirectionRange;
    }
}
