using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private string animatorBool;
    [SerializeField]
    private TextMeshProUGUI text;
    [SerializeField]
    private Color onHoverColour;
    private Color orig;

    private void Start()
    {
        orig = text.color;
    }
    public void OnMouseOver()
    {
        Debug.Log("MouseEntered");
        animator.SetBool(animatorBool, true);
        text.color = onHoverColour;
    }

    public void OnMouseExit()
    {
        Debug.Log("Mouse Exited");
        animator.SetBool(animatorBool, false);
        text.color = orig;
    }
}
