using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FitToScreen : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer background;
    [SerializeField]
    private bool resizeOnStart;

    private bool fitting = true;

    private void Start()
    {
        if(resizeOnStart)
        {
            ResizeBackground();
        }
    }
    void Update()
    {
        if (!resizeOnStart && fitting)
        {
            ResizeBackground();   
        }
    }

    private void ResizeBackground()
    {
        Vector2 spriteSize = background.sprite.bounds.size;
        Vector2 cameraSize = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        Vector2 worldSpace = cameraSize * 2;;
        Vector2 scale = worldSpace / spriteSize;
        background.transform.localScale = scale;
    }

    public void SetFittingOff()
    {
        fitting = false;
    }
}
