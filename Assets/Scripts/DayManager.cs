using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance => instance;
    private static DayManager instance;

    [Header("DayModifiers")]
    public float snakeSpeedModifier;
    public int maxDaysToModifySnakeSpeed;
    public float maxTimeLimit;
    public float timeLimitModifier;
    public int maxDayToModifyTimeLimit;
    public int obstacleDayStart;
    public int obstacleDayEnd;
    public int minScore;
    public int maxScore;
    public int scoreModifier;

    public bool IsPlaying => isPlaying;
    public int CurrentDay => currentDay;

    private bool isPlaying = true;
    private int currentDay;
    private int currentScore;
    private int currentDayScore;
    private float dayTimeLimit;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        dayTimeLimit = maxTimeLimit;
        currentDayScore = minScore;
        StartCoroutine(StartDay());
    }

    public IEnumerator StartDay()
    {

        isPlaying = true;
        float currentTime = 0;
        UIManager.Instance.SetTimeSliderValue(1f);
        while(isPlaying)
        {
            currentTime += Time.deltaTime;
            UIManager.Instance.SetTimeSliderValue(1 - currentTime / dayTimeLimit);
            yield return null;
        }
        isPlaying = false;
        //Temp, change to button event
        EndDay();
        //^^^^
    }

    public void EndDay()
    {
        currentDay++;
        ModifyDayMaxTimeLimit();
        ReloadScene();
        StartCoroutine(StartDay());
    }

    private void ModifyDayMaxTimeLimit()
    {
      dayTimeLimit -= dayTimeLimit * timeLimitModifier;
    }

    private void ModifyDayScore()
    {
        currentDayScore *= scoreModifier;
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
