using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance => instance;
    private static UIManager instance;

    [Header("TimerUI")]
    public Slider timeSlider;
    [Header("ScoreUI")]
    public TextMeshProUGUI targetScoreText;
    public TextMeshProUGUI currentScoreText;
    [Header("DayUI")]
    public TextMeshProUGUI currentDayText;
    [Header("EndDayUI")]
    public Button nextDayButton;

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
    private void Start()
    {
        SetNextDayButtonVisible(false);
    }
    public void SetTimeSliderValue(float value)
    {
        timeSlider.value = value;
    }
    public void SetTargetText(string text)
    {
        targetScoreText.text = text;
    }
    public void SetCurrentScoreText(string text)
    {
        currentScoreText.text = text;
    }
    public void SetCurrentDayText(string text)
    {
        currentDayText.text = text;
    }
    public void EndDay()
    {
        //TODO add all end of day elements
        SetNextDayButtonVisible(true);
    }
    public void StartNextDay()
    {
        DayManager.Instance.NextDay();
    }
    public void SetNextDayButtonVisible(bool visible)
    {
        nextDayButton.gameObject.SetActive(visible);
    }
}
