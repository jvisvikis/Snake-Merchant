using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance => instance;
    private static UIManager instance;
    [Header("TimerUI")]
    public Slider timeSlider;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void SetTimeSliderValue(float value)
    {
        timeSlider.value = value;
    }
}
