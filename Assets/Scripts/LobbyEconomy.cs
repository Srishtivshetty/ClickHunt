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
    private string dataPath;

    private const int entryFee = 100;
    private const int refillAmount = 100;
    private const int refillIntervalSeconds = 3600; // 1 hour
    private int[] dailyRewards = { 100, 200, 300, 300, 300, 300, 300 }; // Weekly reward cycle

    void Awake()
    {
        dataPath = Path.Combine(Application.persistentDataPath, "lobbydata.json");
        LoadData();
        SyncCoinsFromGame();
        CheckForNewDay();
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

    // -- AUTO REFILL SYSTEM --
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

    // DAILY REWARD
    private void CheckForNewDay()
    {
        if (string.IsNullOrEmpty(data.lastClaimDate)) return;

        DateTime lastClaim = DateTime.Parse(data.lastClaimDate);
        if ((DateTime.Now.Date - lastClaim.Date).Days >= 1)
        {
            // Move to next reward if a new day has come
            data.loginDayIndex++;
            if (data.loginDayIndex >= dailyRewards.Length)
                data.loginDayIndex = 0; // Restart weekly cycle
            SaveData();
        }
    }

    public void ClaimDailyReward()
    {
        DateTime lastClaim = string.IsNullOrEmpty(data.lastClaimDate)
            ? DateTime.MinValue
            : DateTime.Parse(data.lastClaimDate);

        // Already claimed today
        if (lastClaim.Date == DateTime.Now.Date)
        {
            if (dailyRewardText != null)
                dailyRewardText.text = "You already claimed today's reward!";
            return;
        }

        // Grant reward
        int reward = dailyRewards[data.loginDayIndex];
        data.coins += reward;

        // Update last claim date
        data.lastClaimDate = DateTime.Now.ToString("yyyy-MM-dd");

        SaveData();
        UpdateCoinUI();
        UpdateDailyRewardUI();

        Debug.Log($"Daily reward claimed: {reward} coins!");
    }

    private void UpdateDailyRewardUI()
    {
        if (dailyRewardText == null) return;

        DateTime lastClaim = string.IsNullOrEmpty(data.lastClaimDate)
            ? DateTime.MinValue
            : DateTime.Parse(data.lastClaimDate);

        if (lastClaim.Date == DateTime.Now.Date)
        {
            dailyRewardText.text = "You already claimed today's reward!";
        }
        else
        {
            dailyRewardText.text = $"Today's reward: {dailyRewards[data.loginDayIndex]} coins";
        }
    }

    //  UI UPDATES 
    private void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = $"Coins: {data.coins}";
    }

    //  ENTER GAME 
    public void TryEnterGame()
    {
        if (data.coins >= entryFee)
        {
            data.coins -= entryFee;
            SaveData();
            UpdateCoinUI();

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

            gameData.remainingAttempts = 3;
            gameData.coins = data.coins;
            File.WriteAllText(gameDataPath, JsonUtility.ToJson(gameData, true));

            Debug.Log("Entry fee paid. Attempts reset to 3 and synced with GameManager.");
            SceneManager.LoadScene("ClickHunt");
        }
        else
        {
            Debug.Log("Not enough coins!");
        }
    }

    // CHEAT COINS
    public void CheatAddCoins()
    {
        if (data.coins == 0)
        {
            int cheatAmount = 1000;
            data.coins += cheatAmount;
            SaveData();
            UpdateCoinUI();

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

    // SYNC FROM GAME 
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

    // SAVE / LOAD 
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
