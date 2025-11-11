using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyCoins : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI coinText;   // UI text for displaying coins
    public TextMeshProUGUI timerText; // show refile countdown
    public TextMeshProUGUI dailyRewardText; // shows daily rewards
    private int coins;
    private int entryFee = 100;        // Entry fee for restarting attempts
    private const int refillAmount = 100; //auto refile amount
    private const int refillIntervalSeconds = 3600; // 1hr=3600 sec
    private GameManager gameManager;   // Only exists in the gameplay scene
    private DateTime nextRefillTime;
    private int[] dailyRewards = { 100, 200, 300, 300, 300, 300, 300 };
    private int currentDayIndex = 0;
    private DateTime lastLoginDate;
    private bool rewardClaimedToday = false;
    void Start()
    {
        // Load saved coins (default = 500)
        coins = PlayerPrefs.GetInt("Coins", 500);
        UpdateCoinText();
        // Load or set the next refill time
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
        // Daily login initialization 
        string savedDate = PlayerPrefs.GetString("LastLoginDate", "");
        if (!string.IsNullOrEmpty(savedDate))
        {
            lastLoginDate = DateTime.Parse(savedDate);
        }
        else
        {
           lastLoginDate = DateTime.MinValue; 
        }
        currentDayIndex = PlayerPrefs.GetInt("LoginDayIndex", 0);
        rewardClaimedToday = (lastLoginDate.Date == DateTime.Now.Date);
        UpdateDailyRewardUI();
        
        // Try to find GameManager (won't exist in lobby)
            gameManager = GameObject.Find("Game Manager")?.GetComponent<GameManager>();
        if (gameManager == null)
        {
            Debug.Log("Game Manager not found in the Lobby scene — that’s normal. It exists only in gameplay.");
        }
    }
    void Update()
    {
        HandleAutoRefill();
        UpdateRefillTimer();
    }
    //Automatically add coins every hour
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

            Debug.Log($"+{refillAmount} coins added automatically! Next refill in 1 hour.");
        }
    }

    // Show time remaining for next refill
    private void UpdateRefillTimer()
    {
        if (timerText == null) return;

        TimeSpan remaining = nextRefillTime - DateTime.Now;
        if (remaining.TotalSeconds <= 0)
        {
            timerText.text = "Refill ready!";
        }
        else
        {
            timerText.text = $"Next refill in: {remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }
    }
    // Daily reward claim
     public void ClaimDailyReward()
    {
        if (rewardClaimedToday)
        {
            Debug.Log("You already claimed today's reward!");
            return;
        }

        int reward = dailyRewards[currentDayIndex];
        coins += reward;
        UpdateCoinText();

        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetString("LastLoginDate", DateTime.Now.Date.ToString());

        currentDayIndex++;
        if (currentDayIndex >= dailyRewards.Length)
            currentDayIndex = dailyRewards.Length - 1;
        PlayerPrefs.SetInt("LoginDayIndex", currentDayIndex);

        PlayerPrefs.Save();

        rewardClaimedToday = true;
        UpdateDailyRewardUI();

        Debug.Log($"Daily reward claimed: {reward} coins!");
    }

    // --- Update daily reward UI ---
    private void UpdateDailyRewardUI()
    {
        if (dailyRewardText == null) return;

        if (rewardClaimedToday)
            dailyRewardText.text = $"Today's Reward Claimed!";
        else
            dailyRewardText.text = $"Today's Reward: {dailyRewards[currentDayIndex]} Coins";
    }

    // Called when the player presses the Play button --Game entry logic
    public void TryEntryGame(int difficulty = 1)
    {
        int remainingAttempts = PlayerPrefs.GetInt("RemainingAttempts", 3); // Default to 3 attempts

        // Case 1: Player still has attempts left
        if (remainingAttempts > 0)
        {
            Debug.Log($"You have {remainingAttempts} attempts remaining. Starting game...");
            LoadGameScene();
            return;
        }

        // No attempts left → check if player can pay entry fee
        if (coins >= entryFee)
        {
            coins -= entryFee; // Deduct entry fee
            PlayerPrefs.SetInt("Coins", coins);

            // Reset 3 new attempts for gameplay
            PlayerPrefs.SetInt("RemainingAttempts", 3);
            PlayerPrefs.Save();

            UpdateCoinText();
            Debug.Log("Entry fee paid. Attempts reset to 3. Starting gameplay...");
            LoadGameScene();
        }
        else
        {
            // Not enough coins
            Debug.Log("Not enough coins to pay entry fee!");
        }
    }

    // Load the main gameplay scene
    private void LoadGameScene()
    {
        SceneManager.LoadScene("ClickHunt"); // Replace "ClickHunt" with your actual gameplay scene name if different
    }

    // Update coin display
    public void UpdateCoinText()
    {
        if (coinText != null)
            coinText.text = "Coins: " + coins;
    }

    // Add coins (for rewards or testing)
    public void AddCoins(int amount)
    {
        coins += amount;
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.Save();
        UpdateCoinText();
        Debug.Log($"{amount} coins added! Total coins: {coins}");
    }

    // Cheat: Add 1000 coins if player has 0
    public void CheatAddCoins()
    {
        if (coins == 0)
        {
            int cheatAmount = 1000;
            coins += cheatAmount;
            PlayerPrefs.SetInt("Coins", coins);
            PlayerPrefs.Save();
            UpdateCoinText();
            Debug.Log($"Cheat used! Added {cheatAmount} coins. Total coins: {coins}");
        }
        else
        {
            Debug.Log("Cheat unavailable! You can only use it when coins = 0.");
        }
    }
}
