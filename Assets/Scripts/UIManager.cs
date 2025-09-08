using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance => instance;
    private static UIManager instance;

    [SerializeField]
    private TextMeshProUGUI endPanelText;
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
    private TextMeshProUGUI dayCompleteText;
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
    [SerializeField]
    private Button resetGameButton;
    [SerializeField]
    private Button retryDayButton;
    [SerializeField]
    private Medusa medusa;

    [Header("OverviewTextUI")]
    [SerializeField]
    private TextMeshProUGUI totalScoreText;
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
    [SerializeField]
    private TextMeshProUGUI upgradeLivesText;

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
    private Button obstaclesUpgradeButton;
    [SerializeField]
    private TextMeshProUGUI warehouseUpgradePriceText;
    [SerializeField]
    private TextMeshProUGUI lengthUpgradePriceText;
    [SerializeField]
    private TextMeshProUGUI speedUpgradePriceText;
    [SerializeField]
    private TextMeshProUGUI livesUpgradePriceText;
    [SerializeField]
    private TextMeshProUGUI upgradePriceText;

    [Header("CoinUI")]
    [SerializeField]
    private TextMeshProUGUI totalCoinsText;

    [Header("LivesUI")]
    [SerializeField]
    private TextMeshProUGUI livesText;
    [Header("ObstaclesUI")]
    [SerializeField]
    private TextMeshProUGUI obstaclesText;

    [Header("ItemQueueUI")]
    [SerializeField]
    private Image[] itemImages;
    [SerializeField]
    private DialogueBox dialogueBox;
    [SerializeField]
    private CustomerAvatar customerAvatar;

    public DialogueBox DialogueBox => dialogueBox;
    public CustomerAvatar CustomerAvatar => customerAvatar;

    private int upgradeCountThisDay = 0;

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
        medusa.gameObject.SetActive(false);
        SetTotalCoinText($" <color=yellow>{EconomyManager.Instance.TotalCoins}");
        SetLivesText($" <color=green>{EconomyManager.Instance.Lives}");
        //string warehouseLevelText = EconomyManager.Instance.WarehouseUpgradeAvailable() ? $"{EconomyManager.Instance.WarehouseLevel} <color=green> > {EconomyManager.Instance.WarehouseLevel + 1}" : "MAX";
        //SetWarehouseLevelText($"Level: {warehouseLevelText}");
        string lengthLevelText = EconomyManager.Instance.SnakeLengthLevel.ToString();
        SetLengthLevelText(lengthLevelText);
        SetObstaclesText(EconomyManager.Instance.NumOfObstacles.ToString());
        SetUpgradeLivesText(EconomyManager.Instance.Lives.ToString());
        //SetSpeedLevelText(EconomyManager.Instance.SnakeSpeedLevel.ToString());
        SetUpgradePriceText($"{EconomyManager.Instance.CurrentUpgradePrice}");
        //SetSpeedUpgradePrice(EconomyManager.Instance.GetCurrentSpeedUpgradePrice().ToString());
        //SetEnableWarehouseUpgrade(EconomyManager.Instance.WarehouseUpgradeAvailable());
        if (EconomyManager.Instance.HasCoinsForUpgrades())
        {
            SetEnableLengthUpgrade(EconomyManager.Instance.SnakeLengthUpgradeAvailable());
            //SetEnableSpeedUpgrade(EconomyManager.Instance.SnakeSpeedUpgradeAvailable());
            SetEnableLifeUpgrade(EconomyManager.Instance.LivesUpgradeAvailable());
            SetEnableObstacleUpgrade(EconomyManager.Instance.ObstacleUpgradeAvailable());
        }
        else
            SetAllUpgrades(false);

    }
    #region Set Text
    public void SetEndPanelText(string text)
    {
        endPanelText.text = text;
    }
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
        if(EconomyManager.Instance.WarehouseUpgradeAvailable())
            warehouseLevelText.text = text;
        else
            warehouseLevelText.text = $"<color=red>MAX";
    }
    public void SetLengthLevelText(string text)
    {
        if(EconomyManager.Instance.SnakeLengthUpgradeAvailable())
            lengthLevelText.text = text;
        else
            lengthLevelText.text = $"<color=red>MAX";
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
    public void SetUpgradeLivesText(string text)
    {
        if(EconomyManager.Instance.LivesUpgradeAvailable())
            upgradeLivesText.text = text;
        else
            upgradeLivesText.text = $"<color=red>MAX";
    }

    public void SetObstaclesText(string text)
    {
        obstaclesText.text = text;
    }
    public void SetWarehouseUpgradePrice(string text)
    {
        if (text.Contains("-1"))
            text = "<color=red>MAX";
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
    public void SetTotalScoreText(string text)
    {
        totalScoreText.text = text;
    }
    public void SetUpgradePriceText(string text)
    {
        upgradePriceText.text = text;
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

    public void SetWarehouseInfo(LevelData current)
    {
        warehouseSizeText.text = $"Size: {current.Width}x{current.Height}";
        warehouseMaxCoinsText.text = $"Max Coins: {current.NumCoins}";
    }
    #endregion
    #region Set Images
    public void SetFirstItem(Sprite sprite, string text)
    {
        itemImages[0].sprite = sprite;
        dialogueBox.SetText(text);
        customerAvatar.RandomiseCharacter();
    }

    public void SetAllItemImages(Sprite[] sprites)
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            itemImages[i].sprite = sprites[i];
        }
    }
    #endregion

    #region Buy Upgrades
    public void BuyObstacleRemoval()
    {
        PlayUpgradeSFX();
        EconomyManager.Instance.BuyObstacleRemoval();
        Debug.Log("Change Damn it!!!");
        SetObstaclesText(EconomyManager.Instance.NumOfObstacles.ToString());
    }
    public void BuyWarehouseUpgrade()
    {
        PlayUpgradeSFX();
        EconomyManager.Instance.UpgradeWarehouse();
    }

    public void BuyLengthUpgrade()
    {
        PlayUpgradeSFX();
        EconomyManager.Instance.UpgradeSnakeLength();
    }

    public void BuyRemoveObstacle()
    {
        PlayUpgradeSFX();
        EconomyManager.Instance.BuyObstacleRemoval();
    }
    public void BuyLife()
    {
        PlayUpgradeSFX();
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
    public void SetEnableObstacleUpgrade(bool active)
    {
        obstaclesUpgradeButton.interactable=active;
    }
    public void SetAllUpgrades(bool active)
    {
        SetEnableLengthUpgrade(active);
        SetEnableLifeUpgrade(active);
        SetEnableObstacleUpgrade(active);
    }
    #endregion
    public void EndDay(bool dead)
    {
        SetEndDayPanelActive(true);
        medusa.gameObject.SetActive(true);
        overviewHolder.gameObject.SetActive(true);
        upgradesHolder.gameObject.SetActive(false);
        SetObstaclesText(EconomyManager.Instance.NumOfObstacles.ToString());
        DisableButtons();
        if (dead)
        {
            nextPanelButton.gameObject.SetActive(false);
            EconomyManager.Instance.RemoveLife();
            bool timeUp = DayManager.Instance.TimeLeft == 0;
            if (EconomyManager.Instance.Lives == 0)
            {
                dayCompleteText.text = "<color=red>Game ";
                dayCompleteText.text += timeUp ? "Time Up" : "Over";
                resetGameButton.gameObject.SetActive(true);
            }
            else
            {
                dayCompleteText.text = "Day <color=red>";
                dayCompleteText.text += timeUp ? "Time Up" : "Failed";
                retryDayButton.gameObject.SetActive(true);
            }
            medusa.ChooseSayingsFail();
        }
        else
        {
            dayCompleteText.text = "Day <color=green>Finished";
            nextPanelButton.gameObject.SetActive(true);
            medusa.ChooseSayingsPass();
        }

        if (EconomyManager.Instance.HasCoinsForUpgrades())
        {
            SetAllUpgrades(true);
        }
        if(EconomyManager.Instance.GetCurrentWarehouseUpgradePrice() <= EconomyManager.Instance.TotalCoins)
        {
            SetEnableWarehouseUpgrade(true);
        }
    }

    private void DisableButtons()
    {
        nextPanelButton.gameObject.SetActive(false);
        nextDayButton.gameObject.SetActive(false);
        resetGameButton.gameObject.SetActive(false);
    }

    public void RestartGame()
    {
        DayManager.Instance.Reset();
    }

    public void RetryDay()
    {
        if (EconomyManager.Instance.Lives == 0)
        {
            Debug.Assert(false);
            RestartGame();
        }
        upgradeCountThisDay = 0;
        RuntimeManager.PlayOneShot(SFX.Instance.NextDay);
        DayManager.Instance.RetryDay();
    }

    public void OpenUpgradesPanel()
    {
        RuntimeManager.PlayOneShot(SFX.Instance.Button);
        overviewHolder.gameObject.SetActive(false);
        upgradesHolder.gameObject.SetActive(true);
        nextPanelButton.gameObject.SetActive(false);
        nextDayButton.gameObject.SetActive(true);
        SetEndPanelText("Upgrades");
    }
    public void StartNextDay()
    {
        upgradeCountThisDay = 0;
        RuntimeManager.PlayOneShot(SFX.Instance.NextDay);
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
    public void SetHoverText(string type)
    {
        if(type.Contains("length") && lengthUpgradeButton.interactable)
        {
            SetLengthLevelText($"<color=green>{EconomyManager.Instance.SnakeLengthLevel + 1}");
        }
        if(type.Contains("obstacles") && obstaclesUpgradeButton.interactable)
        {
            SetObstaclesText($"<color=green>{EconomyManager.Instance.NumOfObstacles - 1}");
        }
        if(type.Contains("life") && livesUpgradeButton.interactable)
        {
            SetUpgradeLivesText($"<color=green>{EconomyManager.Instance.Lives + 1}");
        }
        if(type.Contains("warehouse") && warehouseUpgradeButton.interactable)
        {
            if (EconomyManager.Instance.WarehouseUpgradeAvailable())
            {
                int idx = EconomyManager.Instance.WarehouseLevel + 1;
                warehouseSizeText.text = $"Size: <color=green>{EconomyManager.Instance.warehouses[idx].Width}x{EconomyManager.Instance.warehouses[idx].Height}";
                warehouseMaxCoinsText.text = $"Max Coins: <color=green>{EconomyManager.Instance.warehouses[idx].NumCoins}";
            }
        }
        if(type.Contains("off"))
        {
            SetLengthLevelText($"{EconomyManager.Instance.SnakeLengthLevel}");
            SetUpgradeLivesText($"{EconomyManager.Instance.Lives}");
            SetObstaclesText($"{EconomyManager.Instance.NumOfObstacles}");
            int idx = EconomyManager.Instance.WarehouseLevel;
            SetWarehouseInfo(EconomyManager.Instance.warehouses[idx]);
        }

    }
    private void PlayUpgradeSFX()
    {
        AudioManager.StartEvent(SFX.Instance.Upgrade, out var _, ("UpgradeCount", upgradeCountThisDay));
        upgradeCountThisDay++;
    }

}
