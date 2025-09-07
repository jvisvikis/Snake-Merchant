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
    public int addTimePerItemSold = 5;

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
    private float currentTime;
    private Game game;

    public float TimeLeft => Mathf.Max(0, dayTimeLimit - currentTime);

    public void SetGame(Game game)
    {
        this.game = game;
    }

    public void OnItemsSold(int numItemsSold)
    {
        currentTime = Mathf.Max(0f, currentTime - numItemsSold * addTimePerItemSold * TimeLimitModifierForCurrentDay());
    }

    private void Awake()
    {
        if (instance == null)
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
        currentTime = 0;
        UIManager.Instance.SetTimeSliderValue(1f);
        UIManager.Instance.SetCurrentDayText($"Day: {currentDay + 1}");
        UIManager.Instance.SetTargetText($"Target: {currentTargetScore}");
        UIManager.Instance.SetCurrentScoreText($"Current: {0}");
        if (instantWin)
        {
            EndDay(0, 0, false);
        }
        while (isPlaying && currentTime < dayTimeLimit)
        {
            if (game.SnakeIsMoving)
                currentTime += Time.deltaTime;
            UIManager.Instance.SetTimeSliderValue(1 - currentTime / dayTimeLimit);
            UIManager.Instance.SetTimerText($"{(int)(dayTimeLimit - currentTime)}");
            yield return null;
        }
        if(currentTime > dayTimeLimit)
            EndDay(0, 0, true);
    }

    public void EndDay(int currentDayScore, int totalBonus, bool dead)
    {
        currentTotalScore += currentDayScore;

        string bonusPrefix = "";
        if (dead)
            bonusPrefix = "<s>";
        else
            EconomyManager.Instance.AddObstacles(1);

        string totalScoreTitle = "Current";
        if (dead && EconomyManager.Instance.Lives == 1)
            totalScoreTitle = "Final";

        isPlaying = false;
        string titleColor = "#aaa";
        Debug.Log($"end day with lives: {EconomyManager.Instance.Lives}");
        UIManager.Instance.SetTotalScoreText($"<color={titleColor}>{totalScoreTitle} Score: <color=white>{currentTotalScore}");
        UIManager.Instance.SetTimeLeftText($"<color={titleColor}>Time Left: <color=white>{Mathf.RoundToInt(maxTimeLimit - currentTime)}s");
        UIManager.Instance.SetBonusText($"{bonusPrefix}<color={titleColor}>Day Bonus: <color=white>{totalBonus}");
        UIManager.Instance.SetCoinsCollectedText($"<color={titleColor}>Coins Collected: <color=white>{game.Coins}");
        UIManager.Instance.SetItemsCollectedText($"<color={titleColor}>Items Collected: <color=white>{game.ItemsSold}");
        UIManager.Instance.EndDay(dead);
    }

    public void NextDay()
    {
        currentDay++;
        ModifyDayMaxTimeLimit();
        ModifyDayScore();
        ReloadScene();
    }

    public void RetryDay()
    {
        ReloadScene();
    }

    public void Reset()
    {
        isPlaying = false;
        currentDay = 0;
        currentTargetScore = minScore;
        dayTimeLimit = maxTimeLimit;
        EconomyManager.Instance.Reset();
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

    private float TimeLimitModifierForCurrentDay()
    {
        return Mathf.Pow(1f - timeLimitModifier, CurrentDay);
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
