using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CollectionWorldUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI profitText;
    [SerializeField]
    private Animator animator;
    public float ClipLength => animator.GetCurrentAnimatorClipInfo(0).Length;

    public void SetProfitText(string text)
    {
        profitText.text = text;
    }
}
