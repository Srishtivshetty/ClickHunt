using System;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyEconomy : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI dailyRewardText;

    private LobbyData data;
    private string dataPath;

    private const int entryFee = 100;
    private const int refillAmount = 100;
    private const int refillIntervalSeconds = 3600; // 1 hour

    private int[] dailyRewards = { 100, 200, 300, 300, 300, 300, 300 };
    private bool rewardClaimedToday = false;

    void Awake()
    {
        dataPath = Path.Combine(Application.persistentDataPath, "lobbydata.json");
        LoadData();
    }

    void Start()
    {
        UpdateCoinUI();
        UpdateDailyRewardUI();
    }

    void Update()
    {
        HandleAutoRefill();
        UpdateRefillTimer();
    }

    private void HandleAutoRefill()
    {
        if (string.IsNullOrEmpty(data.nextRefillTime))
        {
            data.nextRefillTime = DateTime.Now.AddSeconds(refillIntervalSeconds).ToBinary().ToString();
            SaveData();
        }

        DateTime nextRefill = DateTime.FromBinary(Convert.ToInt64(data.nextRefillTime));

        if (DateTime.Now >= nextRefill)
        {
            data.coins += refillAmount;
            data.nextRefillTime = DateTime.Now.AddSeconds(refillIntervalSeconds).ToBinary().ToString();
            SaveData();
            UpdateCoinUI();
        }
    }

    private void UpdateRefillTimer()
    {
        if (timerText == null) return;

        DateTime nextRefill = DateTime.FromBinary(Convert.ToInt64(data.nextRefillTime));
        TimeSpan remaining = nextRefill - DateTime.Now;

        if (remaining.TotalSeconds <= 0)
            timerText.text = "Refill ready!";
        else
            timerText.text = $"Next refill in: {remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
    }

    public void ClaimDailyReward()
    {
        DateTime lastClaim = string.IsNullOrEmpty(data.lastClaimDate)
            ? DateTime.MinValue
            : DateTime.Parse(data.lastClaimDate);

        if (lastClaim.Date == DateTime.Now.Date)
        {
            rewardClaimedToday = true;
            if (dailyRewardText != null)
                dailyRewardText.text = "You already claimed today's reward!";
            return;
        }

        int reward = dailyRewards[data.loginDayIndex];
        data.coins += reward;
        data.lastClaimDate = DateTime.Now.ToShortDateString();

        data.loginDayIndex++;
        if (data.loginDayIndex >= dailyRewards.Length)
            data.loginDayIndex = dailyRewards.Length - 1;

        rewardClaimedToday = true;
        SaveData();
        UpdateCoinUI();
        UpdateDailyRewardUI();
    }

    private void UpdateDailyRewardUI()
    {
        if (dailyRewardText == null) return;

        if (rewardClaimedToday)
            dailyRewardText.text = "You already claimed today's reward!";
        else
            dailyRewardText.text = $"Today's reward: {dailyRewards[data.loginDayIndex]} coins";
    }

    private void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = $"Coins: {data.coins}";
    }

    public void TryEnterGame()
    {
        if (data.coins >= entryFee)
        {
            data.coins -= entryFee;
            SaveData();
            UpdateCoinUI();
            SceneManager.LoadScene("ClickHunt"); // Replace with your scene name
        }
        else
        {
            Debug.Log("Not enough coins!");
        }
    }

    private void SaveData()
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(dataPath, json);
    }

    private void LoadData()
    {
        if (File.Exists(dataPath))
        {
            string json = File.ReadAllText(dataPath);
            data = JsonUtility.FromJson<LobbyData>(json);
        }
        else
        {
            data = new LobbyData();
            SaveData();
        }
    }
}
