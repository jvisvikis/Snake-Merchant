using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance => instance;
    private static DayManager instance;

    [Header("Day Modifiers")]
    public float minTimeLimit;
    public float maxTimeLimit;
    public float timeLimitModifier;
    
    [Header("Score Modifiers")]
    public int minScore;
    public int maxScore;
    public float scoreModifier;

    [Header("Debug")]
    [SerializeField]
    private bool instantWin;

    public bool IsPlaying => isPlaying;
    public int CurrentDay => currentDay;
    public int CurrentTargetScore => currentTargetScore;
    public float DayTimeLimit => dayTimeLimit;

    private bool isPlaying = true;
    private int currentDay;
    private int currentTargetScore;
    private int currentTotalScore;
    private float dayTimeLimit;
    private float timeLeft;

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
        if (instantWin)
        {
            EndDay(Int32.MaxValue);

        }
        while (isPlaying && currentTime < dayTimeLimit)
        {
            currentTime += Time.deltaTime;
            UIManager.Instance.SetTimeSliderValue(1 - currentTime / dayTimeLimit);
            timeLeft = currentTime;
            yield return null;
        }
        if(currentTime > dayTimeLimit)
            EndDay(0);
    }

    public void EndDay(int currentDayScore)
    {
        currentTotalScore += currentDayScore;
        isPlaying = false;
        UIManager.Instance.SetTimeLeftText($"Time Left: {Mathf.RoundToInt(timeLeft)}s");
        UIManager.Instance.EndDay();
    }

    public void EndDay(int currentDayScore, int bonusScore, int coins, int itemsCollected)
    {
        currentTotalScore += currentDayScore;
        isPlaying = false;
        UIManager.Instance.SetTimeLeftText($"Time Left: {Mathf.RoundToInt(timeLeft)}s");
        UIManager.Instance.SetBonusText($"Total Bonus: {bonusScore}");
        UIManager.Instance.SetCoinsCollectedText($"Coins Collected: {coins}");
        UIManager.Instance.SetItemsCollectedText($"Items Collected: {itemsCollected}");
        UIManager.Instance.EndDay();
    }

    public void NextDay()
    {
        currentDay++;
        ModifyDayMaxTimeLimit();
        ModifyDayScore();
        ReloadScene();
    }
    public void ResetDay()
    {
        EconomyManager.Instance.RemoveLife();
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
        if (dayTimeLimit < minTimeLimit)
        {
            dayTimeLimit = minTimeLimit;
        }
    }

    private void ModifyDayScore()
    {
        currentTargetScore = Mathf.FloorToInt(currentTargetScore * scoreModifier);
        currentTargetScore = RoundToTen(currentTargetScore);
    }

    private int RoundToTen(int number)
    {
        if(number%10 >=5)
            number += 10 - number % 10;
        else
            number -= number % 10;
        return number;
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
