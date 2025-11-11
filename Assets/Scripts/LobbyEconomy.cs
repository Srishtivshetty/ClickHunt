using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class LobbyCoins : MonoBehaviour
{
    public TextMeshProUGUI coinText;   //UI text for coin
    private int coins;
    private int entryFee = 100;       //Entry fees
    // Start is called before the first frame update
    void Start()
    {
      // start with default 500
        coins = PlayerPrefs.GetInt("Coins", 500);
        UpdateCoinText();
    }
      // Check if player can enter the game
    public bool CanEnterGame()
    {
        return coins >= entryFee;
    }

    public void TryEntryGame()
    {
        if (coins >= entryFee)
        {
            coins -= entryFee;
            PlayerPrefs.SetInt("Coins", coins);
            PlayerPrefs.Save();
            UpdateCoinText();
            Debug.Log("Entry fee paid. Loading game..");
            // replacing with game scene
            SceneManager.LoadScene("ClickHunt");
        }
        else
        {
            Debug.Log("Not enough coins!");
        }
    }

    // Update is called once per frame
    public void UpdateCoinText()
    {
        if (coinText != null)
            coinText.text = "Coins:" + coins;
    }
    // Adding coins for game rewards
    public void AddCoins(int amount)
    {
        coins += amount;
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.Save();
        UpdateCoinText();
        Debug.Log(amount + " coins added! Total coins: " + coins);
    }
    //Cheat: Add 1000 coins instantly
    public void CheatAddCoins()
    {
        if (coins == 0)
        {
            int cheatAmount = 1000;
            coins += cheatAmount;
            PlayerPrefs.SetInt("Coins", coins);
            PlayerPrefs.Save();
            UpdateCoinText();
            Debug.Log("Cheat used! Added " + cheatAmount + " coins. Total coins: " + coins);
        }
        else
        {
            Debug.Log("Cheat unavailable! You can only use it when coins = 0.");
        }
    }
}
