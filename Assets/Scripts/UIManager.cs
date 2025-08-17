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
    [SerializeField]
    private Slider timeSlider;

    [Header("ScoreUI")]
    [SerializeField]
    private TextMeshProUGUI targetScoreText;
    [SerializeField]
    private TextMeshProUGUI currentScoreText;

    [Header("DayUI")]
    [SerializeField]
    private TextMeshProUGUI currentDayText;

    [Header("EndDayUI")]
    [SerializeField]
    private GameObject endDayPanel;
    [SerializeField]
    private Button nextDayButton;

    [Header("UpgradesUI")]
    [SerializeField]
    private TextMeshProUGUI warehouseLevelText;
    [SerializeField]
    private TextMeshProUGUI lengthLevelText;
    [SerializeField]
    private TextMeshProUGUI speedLevelText;

    [Header("UpgradePricesUI")]
    [SerializeField]
    private TextMeshProUGUI warehouseUpgradePriceText;
    [SerializeField]
    private TextMeshProUGUI lengthUpgradePriceText;
    [SerializeField]
    private TextMeshProUGUI speedUpgradePriceText;

    [Header("CoinUI")]
    [SerializeField]
    private TextMeshProUGUI totalCoinsText;
    [SerializeField]
    private TextMeshProUGUI currentCoinsText;

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
        SetupDay();
    }
    private void SetupDay()
    {
        SetEndDayPanelActive(false);
        SetTotalCoinText($"Coins: {EconomyManager.Instance.TotalCoins}");
        SetWarehouseLevelText(EconomyManager.Instance.WarehouseLevel.ToString());
        SetLengthLevelText(EconomyManager.Instance.SnakeLengthLevel.ToString());
        SetSpeedLevelText(EconomyManager.Instance.SnakeSpeedLevel.ToString());
        SetWarehouseUpgradePrice(EconomyManager.Instance.GetCurrentWarehouseUpgradePrice().ToString());
        SetLengthUpgradePrice(EconomyManager.Instance.GetCurrentLengthUpgradePrice().ToString());
        SetSpeedUpgradePrice(EconomyManager.Instance.GetCurrentSpeedUpgradePrice().ToString());
    }
    #region Set Text
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
    public void SetWarehouseLevelText(string text)
    {
        warehouseLevelText.text = text;
    }
    public void SetLengthLevelText(string text)
    {
        lengthLevelText.text = text;
    }
    public void SetSpeedLevelText(string text)
    {
        speedLevelText.text = text;
    }
    public void SetCurrentCoinText(string text)
    {
        currentCoinsText.text = text;
    }
    public void SetTotalCoinText(string text)
    {
        totalCoinsText.text = text;
    }
    public void SetWarehouseUpgradePrice(string text)
    {
        if (text.Equals("-1"))
            text = "N/A";
        warehouseUpgradePriceText.text = text;
    }
    public void SetLengthUpgradePrice(string text)
    {
        if (text.Equals("-1"))
            text = "N/A";
        lengthUpgradePriceText.text = text;
    }
    public void SetSpeedUpgradePrice(string text)
    {
        if (text.Equals("-1"))
            text = "N/A";
        speedUpgradePriceText.text = text;
    }
    #endregion
    public void EndDay()
    {
        SetEndDayPanelActive(true);
    }
    public void StartNextDay()
    {
        DayManager.Instance.NextDay();
    }
    public void SetEndDayPanelActive(bool active)
    {
        endDayPanel.gameObject.SetActive(active);
    }
    public void SetNextDayButtonVisible(bool visible)
    {
        nextDayButton.gameObject.SetActive(visible);
    }

    public void BuyWarehouseUpgrade()
    {
        EconomyManager.Instance.UpgradeWarehouse(3);
    }

    public void BuyLengthUpgrade()
    {
        EconomyManager.Instance.UpgradeSnakeLength();
    }

    public void BuySpeedUpgrade()
    {
        EconomyManager.Instance.UpgradeSpeedLevel();
    }

    public void BuyRemoveObstacle()
    {
        EconomyManager.Instance.BuyObstacleRemoval();
    }

}
