using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Medusa : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private List<string> sayingsPass;
    [SerializeField]
    private List<string> sayingsFail;
    [SerializeField]
    private TextMeshProUGUI dialogueText;


    public void ChooseSayingsPass()
    {
        dialogueText.text = "";
        if (sayingsPass != null)
        {
            int idx = Random.Range(0, sayingsPass.Count);
            dialogueText.text = sayingsPass[idx];
        }
    }

    public void ChooseSayingsFail()
    {
        dialogueText.text = "";
        if (sayingsFail != null)
        {
            int idx = Random.Range(0, sayingsFail.Count);
            dialogueText.text = sayingsFail[idx];
        }
    }
}
