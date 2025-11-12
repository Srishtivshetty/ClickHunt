using UnityEngine;
using UnityEngine.UI;

public class StartButtonScript : MonoBehaviour
{
    private Button button;
    public LobbyEconomy lobbyEconomy; // Correct reference

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnPlayButtonClicked);
    }

    void OnPlayButtonClicked()
    {
        if (lobbyEconomy != null) // Use the correct variable name
        {
            lobbyEconomy.TryEnterGame(); // Call the method from LobbyEconomy
        }
        else
        {
            Debug.LogWarning("LobbyEconomy reference not set on StartButtonScript!");
        }
    }
}
