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
    public float scoreModifier;

    public bool IsPlaying => isPlaying;
    public int CurrentDay => currentDay;
    public int CurrentTargetScore => currentTargetScore;

    private bool isPlaying = true;
    private int currentDay;
    private int currentTargetScore;
    private int currentTotalScore;
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
        currentTargetScore = minScore;
    }

    public IEnumerator StartDay()
    {
        isPlaying = true;
        float currentTime = 0;
        UIManager.Instance.SetTimeSliderValue(1f);
        UIManager.Instance.SetCurrentDayText($"Day: {currentDay}");
        UIManager.Instance.SetTargetText($"Target: {currentTargetScore}");
        UIManager.Instance.SetCurrentScoreText($"Current: {0}");
        while(isPlaying && currentTime < dayTimeLimit)
        {
            currentTime += Time.deltaTime;
            UIManager.Instance.SetTimeSliderValue(1 - currentTime / dayTimeLimit);
            yield return null;
        }
        isPlaying = false;
        EndDay();
    }

    public void EndDay()
    {
        isPlaying = false;
        UIManager.Instance.EndDay();
    }

    public void NextDay()
    {
        currentDay++;
        ModifyDayMaxTimeLimit();
        ModifyDayScore();
        ReloadScene();
    }

    public void Reset()
    {
        isPlaying = false;
        currentDay = 0;
        currentTargetScore = minScore;
        dayTimeLimit = maxTimeLimit;
        ReloadScene();
    }

    private void ModifyDayMaxTimeLimit()
    {
      dayTimeLimit -= dayTimeLimit * timeLimitModifier;
    }

    private void ModifyDayScore()
    {
        currentTargetScore = Mathf.FloorToInt(currentTargetScore * scoreModifier);
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
