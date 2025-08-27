using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CollectionWorldUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI profitText;

    public void SetProfitText(string text)
    {
        profitText.text = text;
    }
}
