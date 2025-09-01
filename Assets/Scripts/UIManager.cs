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
    [SerializeField]
    private TextMeshProUGUI timerText;

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
    private GameObject overviewHolder;
    [SerializeField]
    private GameObject upgradesHolder;
    [SerializeField]
    private Button nextPanelButton;
    [SerializeField]
    private Button nextDayButton;

    [Header("OverviewTextUI")]
    [SerializeField]
    private TextMeshProUGUI timeLeftText;
    [SerializeField]
    private TextMeshProUGUI totalBonusText;
    [SerializeField]
    private TextMeshProUGUI itemsCollectedText;
    [SerializeField]
    private TextMeshProUGUI coinsCollectedText;

    [Header("UpgradesUI")]
    [SerializeField]
    private TextMeshProUGUI warehouseLevelText;
    [SerializeField]
    private TextMeshProUGUI warehouseSizeText;
    [SerializeField]
    private TextMeshProUGUI warehouseMaxCoinsText;
    [SerializeField]
    private TextMeshProUGUI lengthLevelText;
    [SerializeField]
    private TextMeshProUGUI speedLevelText;

    [Header("UpgradeShopUI")]
    [SerializeField]
    private Button warehouseUpgradeButton;
    [SerializeField]
    private Button lengthUpgradeButton;
    [SerializeField]
    private Button speedUpgradeButton;
    [SerializeField]
    private Button livesUpgradeButton;
    [SerializeField]
    private TextMeshProUGUI warehouseUpgradePriceText;
    [SerializeField]
    private TextMeshProUGUI lengthUpgradePriceText;
    [SerializeField]
    private TextMeshProUGUI speedUpgradePriceText;
    [SerializeField]
    private TextMeshProUGUI livesUpgradePriceText;

    [Header("CoinUI")]
    [SerializeField]
    private TextMeshProUGUI totalCoinsText;

    [Header("LivesUI")]
    [SerializeField]
    private TextMeshProUGUI livesText;

    [Header("ItemQueueUI")]
    [SerializeField]
    private Image [] itemImages;

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
        SetTotalCoinText($"<sprite=0> <color=yellow>{EconomyManager.Instance.TotalCoins}");
        SetLivesText($"<sprite=1> <color=green>{EconomyManager.Instance.Lives}");
        //string warehouseLevelText = EconomyManager.Instance.WarehouseUpgradeAvailable() ? $"{EconomyManager.Instance.WarehouseLevel} <color=green> > {EconomyManager.Instance.WarehouseLevel + 1}" : "MAX";
        //SetWarehouseLevelText($"Level: {warehouseLevelText}");
        string lengthLevelText = EconomyManager.Instance.SnakeLengthUpgradeAvailable() ? $"{EconomyManager.Instance.SnakeLengthLevel+1} <color=green> > {EconomyManager.Instance.SnakeLengthLevel + 2}" : "<color=red>MAX";
        SetLengthLevelText($"Length: {lengthLevelText}");
        SetSpeedLevelText(EconomyManager.Instance.SnakeSpeedLevel.ToString());
        SetWarehouseUpgradePrice($"Upgrade: <color=yellow>{EconomyManager.Instance.GetCurrentWarehouseUpgradePrice()}<sprite=0>");
        SetLengthUpgradePrice($"{EconomyManager.Instance.GetCurrentLengthUpgradePrice()}<sprite=0>");
        SetSpeedUpgradePrice(EconomyManager.Instance.GetCurrentSpeedUpgradePrice().ToString());
        string lifePrice = EconomyManager.Instance.LivesUpgradeAvailable() ? EconomyManager.Instance.GetLifeUpgradePrice().ToString() : "";
        SetLifePurchasePrice(lifePrice);
        SetEnableWarehouseUpgrade(EconomyManager.Instance.WarehouseUpgradeAvailable());
        SetEnableLengthUpgrade(EconomyManager.Instance.SnakeLengthUpgradeAvailable());
        SetEnableSpeedUpgrade(EconomyManager.Instance.SnakeSpeedUpgradeAvailable());
        SetEnableLifeUpgrade(EconomyManager.Instance.LivesUpgradeAvailable());
    }
    #region Set Text
    public void SetTimeSliderValue(float value)
    {
        timeSlider.value = value;
    }
    public void SetTimerText(string text)
    {
        timerText.text = text;
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
    public void SetTotalCoinText(string text)
    {
        totalCoinsText.text = text;
    }
    public void SetLivesText(string text)
    {
        livesText.text = text;
    }
    public void SetWarehouseUpgradePrice(string text)
    {
        if (text.Contains("-1"))
            text = "Upgrade: <color=red>MAX";
        warehouseUpgradePriceText.text = text;
    }
    public void SetLengthUpgradePrice(string text)
    {
        if (text.Contains("-1"))
            text = "";
        lengthUpgradePriceText.text = text;
    }
    public void SetSpeedUpgradePrice(string text)
    {
        if (text.Contains("-1"))
            text = "";
        speedUpgradePriceText.text = text;
    }
    public void SetLifePurchasePrice(string text)
    {
        if (text.Contains("-1"))
            text = "";
        livesUpgradePriceText.text = text;
    }
    public void SetTimeLeftText(string text)
    {
        timeLeftText.text = text;
    }
    public void SetBonusText(string text)
    {
        totalBonusText.text = text;
    }
    public void SetItemsCollectedText(string text)
    {
        itemsCollectedText.text = text;
    }
    public void SetCoinsCollectedText(string text)
    {
        coinsCollectedText.text = text;
    }

    public void SetWarehouseInfo(LevelData current, LevelData next)
    {
        if(next == null)
        {
            warehouseSizeText.text = $"Size: <color=red>{current.Width}x{current.Height}";
            warehouseMaxCoinsText.text = $"Max Coins: <color=red>{current.NumCoins}";
        }
        else
        {
            warehouseSizeText.text = $"Size: {current.Width}x{current.Height}<color=green> > {next.Width}x{next.Height}";
            warehouseMaxCoinsText.text = $"Max Coins: {current.NumCoins}<color=green> > {next.NumCoins}";
        }
    }
    #endregion
    #region Set Images
    public void SetFirstItemImage(Sprite sprite)
    {
        itemImages[0].sprite = sprite;
    }

    public void SetAllItemImages(Sprite[] sprites)
    {
        for(int i = 0; i < sprites.Length; i++)
        {
            itemImages[i].sprite = sprites[i];
        }
    }
    #endregion

    #region Buy Upgrades
    public void BuyWarehouseUpgrade()
    {
        EconomyManager.Instance.UpgradeWarehouse();
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
    public void BuyLife()
    {
        EconomyManager.Instance.BuyLife();
    }
    #endregion
    #region Enable/Disable Buttons
    public void SetEnableWarehouseUpgrade(bool active)
    {
        warehouseUpgradeButton.interactable = active;
    }
    public void SetEnableLengthUpgrade(bool active)
    {
        lengthUpgradeButton.interactable = active;
    }
    public void SetEnableSpeedUpgrade(bool active)
    {
        speedUpgradeButton.interactable = active;
    }
    public void SetEnableLifeUpgrade(bool active)
    {
        livesUpgradeButton.interactable = active;
    }
    #endregion
    public void EndDay()
    {
        SetEndDayPanelActive(true);
        overviewHolder.gameObject.SetActive(true);
        upgradesHolder.gameObject.SetActive(false);
        nextPanelButton.gameObject.SetActive(true);
        nextDayButton.gameObject.SetActive(false);
    }

    public void OpenUpgradesPanel()
    {
        overviewHolder.gameObject.SetActive(false);
        upgradesHolder.gameObject.SetActive(true);
        nextPanelButton.gameObject.SetActive(false);
        nextDayButton.gameObject.SetActive(true);

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
    


}
