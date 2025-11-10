using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartButtonScript : MonoBehaviour
{
    private Button button;
    public LobbyCoins lobbyCoins;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnPlayButtonClicked);
    }

    void OnPlayButtonClicked()
    {
        if (lobbyCoins != null)
        {
            lobbyCoins.TryEntryGame(); 
        }
        else
        {
           Debug.LogWarning("LobbyCoins reference not set on StartButtonScript!");
        }
    }
}
