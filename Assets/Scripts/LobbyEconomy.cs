using System;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyCoins : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI coinText;          
    public TextMeshProUGUI timerText;         
    public TextMeshProUGUI dailyRewardText;   

    private int coins;
    private int entryFee = 100;               
    private const int refillAmount = 100;     
    private const int refillIntervalSeconds = 3600; // 1 hour

    private GameManager gameManager;          
    private DateTime nextRefillTime;

    // --- Daily reward data ---
    private int[] dailyRewards = { 100, 200, 300, 300, 300, 300, 300 };
    private int currentDayIndex = 0;
    private DateTime lastClaimDate;
    private bool rewardClaimedToday = false;

    void Start()
    {
        // Load coins (default 500)
        coins = PlayerPrefs.GetInt("Coins", 500);
        UpdateCoinText();

        // --- Refill timer setup ---
        string savedTime = PlayerPrefs.GetString("NextRefillTime", "");
        if (string.IsNullOrEmpty(savedTime))
        {
            nextRefillTime = DateTime.Now.AddSeconds(refillIntervalSeconds);
            PlayerPrefs.SetString("NextRefillTime", nextRefillTime.ToBinary().ToString());
        }
        else
        {
            long binaryTime = Convert.ToInt64(savedTime);
            nextRefillTime = DateTime.FromBinary(binaryTime);
        }

        // --- Daily reward setup ---
        string savedDate = PlayerPrefs.GetString("LastClaimDate", "");
        if (!string.IsNullOrEmpty(savedDate))
            lastClaimDate = DateTime.Parse(savedDate);
        else
            lastClaimDate = DateTime.MinValue;

        currentDayIndex = PlayerPrefs.GetInt("LoginDayIndex", 0);

        // Check if today is a new day
        if (lastClaimDate.Date == DateTime.Now.Date)
        {
            // Already claimed today
            rewardClaimedToday = true;
        }
        else
        {
            // New day → advance reward only if not first login ever
            if (lastClaimDate != DateTime.MinValue)
            {
                currentDayIndex++;
                if (currentDayIndex >= dailyRewards.Length)
                    currentDayIndex = dailyRewards.Length - 1;
            }
            rewardClaimedToday = false;
        }

        UpdateDailyRewardUI();

        // Optional: find GameManager (for gameplay)
        gameManager = GameObject.Find("Game Manager")?.GetComponent<GameManager>();
        if (gameManager == null)
            Debug.Log("Game Manager not found in Lobby — normal behavior.");
    }

    void Update()
    {
        HandleAutoRefill();
        UpdateRefillTimer();
    }

    // --- Automatic hourly refill ---
    private void HandleAutoRefill()
    {
        if (DateTime.Now >= nextRefillTime)
        {
            coins += refillAmount;
            PlayerPrefs.SetInt("Coins", coins);
            UpdateCoinText();

            nextRefillTime = DateTime.Now.AddSeconds(refillIntervalSeconds);
            PlayerPrefs.SetString("NextRefillTime", nextRefillTime.ToBinary().ToString());
            PlayerPrefs.Save();

            Debug.Log($"+{refillAmount} coins added automatically!");
        }
    }

    // --- Show refill timer ---
    private void UpdateRefillTimer()
    {
        if (timerText == null) return;

        TimeSpan remaining = nextRefillTime - DateTime.Now;
        if (remaining.TotalSeconds <= 0)
            timerText.text = "Refill ready!";
        else
            timerText.text = $"Next refill in: {remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
    }

    // --- Claim daily reward ---
    public void ClaimDailyReward()
    {
        // Check if today's reward was already claimed
        if (rewardClaimedToday)
        {
            Debug.Log("You already claimed today's reward!");
            if (dailyRewardText != null)
                dailyRewardText.text = "You already claimed today's reward!";
            return;
        }

        // Give today's reward
        int reward = dailyRewards[currentDayIndex];
        coins += reward;
        PlayerPrefs.SetInt("Coins", coins);

        // Save claim date and current day index
        PlayerPrefs.SetString("LastClaimDate", DateTime.Now.Date.ToShortDateString());
        PlayerPrefs.SetInt("LoginDayIndex", currentDayIndex);
        PlayerPrefs.Save();

        rewardClaimedToday = true;
        UpdateCoinText();

        // Update UI message immediately
        if (dailyRewardText != null)
            dailyRewardText.text = "You already claimed today's reward!";

        Debug.Log($"Daily reward claimed: {reward} coins!");
    }

    // --- Update daily reward UI ---
    private void UpdateDailyRewardUI()
    {
        if (dailyRewardText == null) return;

        if (rewardClaimedToday)
            dailyRewardText.text = "You already claimed today's reward!";
        else
            dailyRewardText.text = $"Today's reward: {dailyRewards[currentDayIndex]} coins";
    }

    // --- Game entry ---
    public void TryEntryGame(int difficulty = 1)
    {
        int remainingAttempts = PlayerPrefs.GetInt("RemainingAttempts", 3);

        if (remainingAttempts > 0)
        {
            Debug.Log($"You have {remainingAttempts} attempts remaining. Starting game...");
            LoadGameScene();
            return;
        }

        if (coins >= entryFee)
        {
            coins -= entryFee;
            PlayerPrefs.SetInt("Coins", coins);
            PlayerPrefs.SetInt("RemainingAttempts", 3);
            PlayerPrefs.Save();

            UpdateCoinText();
            Debug.Log("Entry fee paid. Attempts reset to 3. Starting gameplay...");
            LoadGameScene();
        }
        else
        {
            Debug.Log("Not enough coins to pay entry fee!");
        }
    }

    // --- Load game scene ---
    private void LoadGameScene()
    {
        SceneManager.LoadScene("ClickHunt");
    }

    // --- Update coin text ---
    public void UpdateCoinText()
    {
        if (coinText != null)
            coinText.text = "Coins: " + coins;
    }

    // --- Manual add coins (debug/testing) ---
    public void AddCoins(int amount)
    {
        coins += amount;
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.Save();
        UpdateCoinText();
        Debug.Log($"{amount} coins added! Total: {coins}");
    }

    // --- Cheat add ---
    public void CheatAddCoins()
    {
        if (coins == 0)
        {
            int cheatAmount = 1000;
            coins += cheatAmount;
            PlayerPrefs.SetInt("Coins", coins);
            PlayerPrefs.Save();
            UpdateCoinText();
            Debug.Log($"Cheat used! Added {cheatAmount} coins.");
        }
        else
        {
            Debug.Log("Cheat unavailable — only when coins = 0.");
        }
    }
}
