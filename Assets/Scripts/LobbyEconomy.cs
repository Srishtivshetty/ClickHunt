using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class LobbyCoins : MonoBehaviour
{
    public TextMeshProUGUI coinText;
    private int coins;
    // Start is called before the first frame update
    void Start()
    {
        coins = PlayerPrefs.GetInt("Coins", 500);
        UpdateCoinText();
    }

    // Update is called once per frame
    void UpdateCoinText()
    {
        coinText.text = "Coins:" + coins;
    }
}
