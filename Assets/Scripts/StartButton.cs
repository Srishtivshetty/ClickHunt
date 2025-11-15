using UnityEngine;
using UnityEngine.UI;

public class StartButtonScript : MonoBehaviour
{
    private Button button;            // Reference to the UI Button component
    public LobbyEconomy lobbyEconomy; // Reference to the LobbyEconomy script (assigned in Inspector)

    void Start()
    {
        // Get the Button component on this GameObject
        button = GetComponent<Button>();

        // Register click event -> runs OnPlayButtonClicked() when pressed
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
