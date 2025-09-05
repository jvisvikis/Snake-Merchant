using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GridSquare : MonoBehaviour
{
    public enum Type
    {
        Middle,
        Left,
        Right,
        Top,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Spawn,
    }

    [System.Serializable]
    public struct TypeSprite
    {
        public Type Type;
        public Sprite Sprite;
        public Vector2 Offset;
    }

    [System.Serializable]
    public struct TypeSprites
    {
        public Type Type;
        public Sprite[] Sprites;
        public Vector2 Offset;
    }

    [SerializeField]
    private TypeSprite[] typeSprites;

    [SerializeField]
    private Color obstacleColor = Color.black;

    [SerializeField]
    private Color itemColor = Color.grey;

    [SerializeField]
    private Color snakeColor = Color.yellow;

    [SerializeField, Min(0f)]
    private float setColorTime = 0.1f;

    public Vector2Int Cell => cell;

    private SpriteRenderer spriteRenderer;
    private Vector2Int cell;
    public bool hasSnake;
    private RotatedItemData itemData;
    public bool invertItemColor;

    public void Init(Vector2Int cell, TypeSprite typeSprite)
    {
        this.cell = cell;
        spriteRenderer.sprite = typeSprite.Sprite;
        spriteRenderer.transform.localPosition += (Vector3)typeSprite.Offset;
        if (typeSprite.Type == Type.Middle)
            spriteRenderer.transform.localRotation *= RandomSquareRotation();
        Render(true);
    }

    private Quaternion RandomSquareRotation()
    {
        return Quaternion.Euler(0, 0, 90 * Random.Range(0, 3));
    }

    public void SetInvertItemColor(bool invert)
    {
        invertItemColor = invert;
        Render();
    }

    public void SetHasSnake(bool hasSnake)
    {
        this.hasSnake = hasSnake;
        Render();
    }

    public void SetItemData(RotatedItemData itemData)
    {
        this.itemData = itemData;
        Render();
    }

    private bool HasCollectibleItem()
    {
        return itemData != null && itemData.IsCollectible;
    }

    private void Render(bool immediate = false)
    {
        if (spriteRenderer == null)
            return;

        Color setColor;

        if (invertItemColor && !HasCollectibleItem())
            setColor = hasSnake ? snakeColor : obstacleColor;
        else if (!HasCollectibleItem())
            setColor = hasSnake ? snakeColor : Color.white;
        else if (itemData.IsCollectible && !invertItemColor)
            setColor = itemColor;
        else
            setColor = Color.white;

        if (immediate)
        {
            spriteRenderer.color = setColor;
        }
        else if (setColor != spriteRenderer.color)
        {
            StopAllCoroutines();
            StartCoroutine(SetColorCoroutine(setColor));
        }
    }

    public void Reset()
    {
        hasSnake = false;
        itemData = null;
        invertItemColor = false;
        Render(true);
    }

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnDestroy()
    {
        spriteRenderer = null;
    }

    private IEnumerator SetColorCoroutine(Color setColor)
    {
        var fromColor = spriteRenderer.color;
        for (float t = 0; t < setColorTime; t += Time.deltaTime)
        {
            spriteRenderer.color = VectorUtil.SmoothStep(fromColor, setColor, t);
            yield return null;
        }
        spriteRenderer.color = setColor;
    }
}
