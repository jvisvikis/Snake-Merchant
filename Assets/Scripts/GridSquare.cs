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
        BottomRight
    }

    [System.Serializable]
    public struct TypeSprite
    {
        public Type Type;
        public Sprite Sprite;
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

    [SerializeField]
    private Color spawnColor = Color.green;

    [SerializeField, Min(0f)]
    private float setColorTime = 0.1f;

    public Vector2Int Cell => cell;

    private SpriteRenderer spriteRenderer;
    private Vector2Int cell;
    private bool hasSnake;
    private bool isSpawn;
    private ItemData itemData;
    private bool invertItemColor;

    public void Init(Vector2Int cell, Type type)
    {
        this.cell = cell;
        spriteRenderer.sprite = null;

        foreach (var typeSprite in typeSprites)
        {
            if (typeSprite.Type == type)
            {
                spriteRenderer.sprite = typeSprite.Sprite;
                spriteRenderer.transform.localPosition += (Vector3)typeSprite.Offset;
                break;
            }
        }

        if (spriteRenderer.sprite == null)
            Debug.LogError($"No sprite found for type {type}");

        Render(true);
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

    public void SetItemData(ItemData itemData)
    {
        this.itemData = itemData;
        Render();
    }

    private void Render(bool immediate = false)
    {
        if (spriteRenderer == null)
            return;

        Color setColor;

        if (invertItemColor && itemData == null)
            setColor = hasSnake ? snakeColor : obstacleColor;
        else if (itemData == null)
            setColor = isSpawn
                ? spawnColor
                : hasSnake
                    ? snakeColor
                    : Color.white;
        else if (itemData.IsObstacle)
            setColor = obstacleColor;
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
        isSpawn = false;
        itemData = null;
        invertItemColor = false;
        Render();
    }

    public void SetIsSpawn(bool isSpawn)
    {
        this.isSpawn = isSpawn;
        Render();
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
