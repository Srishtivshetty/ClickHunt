using System;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

[System.Serializable]
public class LobbyData
{
    public int coins = 500;
    public int loginDayIndex = 0;
    public string lastClaimDate = "";
    public string nextRefillTime = "";
    public int remainingAttempts = 3;
}

public class LobbyEconomy : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI dailyRewardText;

    private LobbyData data;
    private string dataPath;               // Path to lobby JSON file

    private const int entryFee = 100;        // Coins required to enter the game
    private const int refillAmount = 100;    // Auto refill amount
    private const int refillIntervalSeconds = 3600; // 1 hour refill interval

    private int[] dailyRewards = { 100, 200, 300, 300, 300, 300, 300 }; // Weekly reward structure
    private bool rewardClaimedToday = false;

    // -Unity Lifecycle -
    void Awake()
    {
        dataPath = Path.Combine(Application.persistentDataPath, "lobbydata.json");
        LoadData();
        SyncCoinsFromGame(); // <-- Sync coins from GameManager JSON
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

    // - Refill System -
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

    // - Daily Reward -
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

        Debug.Log($"Daily reward claimed: {reward} coins!");
    }

    private void UpdateDailyRewardUI()
    {
        if (dailyRewardText == null) return;

        if (rewardClaimedToday)
            dailyRewardText.text = "You already claimed today's reward!";
        else
            dailyRewardText.text = $"Today's reward: {dailyRewards[data.loginDayIndex]} coins";
    }

    // UI Updates 
    private void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = $"Coins: {data.coins}";
    }

    // Enter Game Logic
    public void TryEnterGame()
    {
        if (data.coins >= entryFee)
        {
            // Deduct entry fee
            data.coins -= entryFee;
            SaveData();
            UpdateCoinUI();

            // Sync to GameManager JSON file
            string gameDataPath = Path.Combine(Application.persistentDataPath, "gamedata.json");
            GameData gameData;

            if (File.Exists(gameDataPath))
            {
                string json = File.ReadAllText(gameDataPath);
                gameData = JsonUtility.FromJson<GameData>(json);
            }
            else
            {
                gameData = new GameData();
            }

            //Reset attempts and sync coins
            gameData.remainingAttempts = 3;
            gameData.coins = data.coins;
            File.WriteAllText(gameDataPath, JsonUtility.ToJson(gameData, true));

            Debug.Log("Entry fee paid. Attempts reset to 3 and synced with GameManager.");

            // Load gameplay scene
            SceneManager.LoadScene("ClickHunt");
        }
        else
        {
            Debug.Log("Not enough coins!");
        }
    }

    // Cheat Function 
    public void CheatAddCoins()
    {
        if (data.coins == 0)
        {
            int cheatAmount = 1000;
            data.coins += cheatAmount;
            SaveData();
            UpdateCoinUI();

            // Also sync to GameManager JSON
            string gameDataPath = Path.Combine(Application.persistentDataPath, "gamedata.json");
            GameData gameData;

            if (File.Exists(gameDataPath))
            {
                string json = File.ReadAllText(gameDataPath);
                gameData = JsonUtility.FromJson<GameData>(json);
            }
            else
            {
                gameData = new GameData();
            }

            gameData.coins = data.coins;
            File.WriteAllText(gameDataPath, JsonUtility.ToJson(gameData, true));

            Debug.Log($"Cheat used! Added {cheatAmount} coins. Total coins: {data.coins}");
        }
        else
        {
            Debug.Log("Cheat unavailable! You can only use it when coins = 0.");
        }
    }
    
    // Sync coins from GameManager
    public void SyncCoinsFromGame()
    {
        string gameDataPath = Path.Combine(Application.persistentDataPath, "gamedata.json");
        if (File.Exists(gameDataPath))
        {
            string json = File.ReadAllText(gameDataPath);
            GameData gameData = JsonUtility.FromJson<GameData>(json);
            data.coins = gameData.coins;
            SaveData();
            UpdateCoinUI();
            Debug.Log("Lobby coins synced from game!");
        }
    }

    // JSON Save/Load
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
