using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenClosePanel : MonoBehaviour
{
    
    public void SetPanelOpen(GameObject panel)
    {
        panel.SetActive(true);
    }

    public void SetPanelClose(GameObject panel)
    {
        panel.SetActive(false);
    }
}
