using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FitToScreen : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer background;
    [SerializeField]
    private bool resizeOnStart;
    private void Start()
    {
        if(resizeOnStart)
        {
            ResizeBackground();
        }
    }
    void Update()
    {
        if (!resizeOnStart)
        {
            ResizeBackground();   
        }
    }

    private void ResizeBackground()
    {
        Vector2 spriteSize = background.sprite.bounds.size;
        Vector2 cameraSize = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        Vector2 worldSpace = cameraSize * 2;
        Debug.Log($"Sprite: {spriteSize.x},{spriteSize.y} Camera: {worldSpace.x},{worldSpace.y}");
        Vector2 scale = worldSpace / spriteSize;
        Debug.Log($"Scale: {scale.x},{scale.y}");
        background.transform.localScale = scale;
    }
}
