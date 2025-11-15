using System;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

// Stores all persistent data for the lobby:
// coins, login streak, last reward claim, refill timer, attempts, etc.
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

    //--Data System--
    private LobbyData data;   //Runtime copy of Lobby data
    private string dataPath;  //File path for Lobbydata.json
    
    //--Economy Setting--
    private const int entryFee = 100;  //Entry fees
    private const int refillAmount = 100;
    private const int refillIntervalSeconds = 3600; // 1 hour

    //Weekly daily reward structure 
    private int[] dailyRewards = { 100, 200, 300, 300, 300, 300, 300 }; 

    //-- Unity Methods--
    void Awake()
    {
        // Build file path and load saved data
        dataPath = Path.Combine(Application.persistentDataPath, "lobbydata.json");
        LoadData();
        SyncCoinsFromGame(); // Ensure coins match GameData (if game was played)
        CheckForNewDay();    // Check if we moved to a new day for daily reward progression
    }

    void Start()
    {
        UpdateCoinUI();
        UpdateDailyRewardUI();
    }

    void Update()
    {
        HandleAutoRefill();   // Auto-add coins every hour
        UpdateRefillTimer();  // Update countdown text
    }

    // -- AUTO REFILL SYSTEM --
    // Gives the player coins automatically every hour.
    private void HandleAutoRefill()
    {
        // If no next refill time stored -> create one
        if (string.IsNullOrEmpty(data.nextRefillTime))
        {
            data.nextRefillTime = DateTime.Now.AddSeconds(refillIntervalSeconds).ToBinary().ToString();
            SaveData();
        }
        DateTime nextRefill = DateTime.FromBinary(Convert.ToInt64(data.nextRefillTime));
        
        // If refill time reached -> add coins and schedule next refill
        if (DateTime.Now >= nextRefill)
        {
            data.coins += refillAmount;
            data.nextRefillTime = DateTime.Now.AddSeconds(refillIntervalSeconds).ToBinary().ToString();
            SaveData();
            UpdateCoinUI();
        }
    }
    
    // Shows a countdown to the next refill.
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

    //--Daily Reward Syatem--
    private void CheckForNewDay()
    {
        if (string.IsNullOrEmpty(data.lastClaimDate)) return;

        DateTime lastClaim = DateTime.Parse(data.lastClaimDate);
        // If today is a new calendar day → advance reward cycle
        if ((DateTime.Now.Date - lastClaim.Date).Days >= 1)
        {
            // Move to next reward if a new day has come
            data.loginDayIndex++;
            // Loop back to day 0 after day 6
            if (data.loginDayIndex >= dailyRewards.Length)
                data.loginDayIndex = 0;     // Restart weekly cycle
            SaveData();
        }
    }
     // Called when user presses "Claim Daily Reward".
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

        // Grant today's reward
        int reward = dailyRewards[data.loginDayIndex];
        data.coins += reward;

        // Update last claim date
        data.lastClaimDate = DateTime.Now.ToString("yyyy-MM-dd");

        SaveData();
        UpdateCoinUI();
        UpdateDailyRewardUI();

        Debug.Log($"Daily reward claimed: {reward} coins!");
    }
    // Updates the text showing today's reward or claim status.
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

    // --UI UPDATES-- 
    // Refresh the coin total shown to the player.
    private void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = $"Coins: {data.coins}";
    }

    //--Game Entry Logic--
    // Called when "Play" button is pressed. Charges entry fee and resets attempts.
    public void TryEnterGame()
    {
        if (data.coins >= entryFee)
        {
            data.coins -= entryFee;  // Pay entry fee
            SaveData();
            UpdateCoinUI();
            // Sync to GameData (used by GameManager)
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
            // Reset attempts and coin sync
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

    // --CHEAT FUNCTION COINS--
    public void CheatAddCoins()
    {
        if (data.coins == 0)
        {
            int cheatAmount = 1000;
            data.coins += cheatAmount;
            SaveData();
            UpdateCoinUI();
            // Also sync to game data
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

    // --SYNC WITH GAME MANAGER--
    // Loads GameData and syncs its coin value back to the lobby.
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

    //--Save & Load--
    // Writes the current LobbyData to JSON file.
    private void SaveData()
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(dataPath, json);
    }
    // Loads lobby data, or creates new file if none exists
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
