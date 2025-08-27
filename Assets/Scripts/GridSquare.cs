using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }

    [SerializeField]
    private TypeSprite[] typeSprites;

    private SpriteRenderer spriteRenderer;

    private Type type;
    private Grid grid;

    public void Init(Type type, Grid grid)
    {
        this.type = type;
        this.grid = grid;

        spriteRenderer.sprite = null;

        foreach (var typeSprite in typeSprites)
        {
            if (typeSprite.Type == type)
            {
                spriteRenderer.sprite = typeSprite.Sprite;
                break;
            }
        }

        if (spriteRenderer.sprite == null)
            Debug.LogError($"No sprite found for type {type}");
    }

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
}
