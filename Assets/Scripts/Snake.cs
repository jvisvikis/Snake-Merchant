using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    public SnakePart head;
    public GameObject bodyPrefab;
    public GameObject tailPrefab;
    private Vector2 pos;
    public Vector2 Pos { get { return pos; } }
    private Vector2 dir = new Vector2(1,0);
    public Vector2 Dir { get { return dir; } }

    public void SetSize(float size)
    {
        this.transform.localScale = new Vector3(size,size);
    }

    public void SetInitialPos(Vector2 pos)
    {
        this.pos = pos;
        head.transform.localPosition = pos;
        SnakePart current = head;
        while (current.next != null)
        {
            current.next.transform.localPosition = current.transform.localPosition + Vector3.left;
            current = current.next;
        }
    }

    public void Move()
    {
        pos += dir;
        Vector2 oldPos = head.transform.localPosition;
        head.transform.localPosition = pos;
        SnakePart current = head;
        while (current.next != null)
        {
            Vector2 temp = current.next.transform.localPosition;
            current.next.transform.localPosition = oldPos;
            oldPos = temp;
            current = current.next;
        }
    }

}
