using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueBox : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private TextMeshProUGUI textBox;

    private string text;
    public void ResetAnimation()
    {
        animator.SetTrigger("Reset");
    }

    public void SetText(string text)
    {
        this.text = text;
        SetTextBox();
    }

    public void SetTextBox()
    {
        textBox.text = text;
    }
}
