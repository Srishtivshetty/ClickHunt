using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyButton : MonoBehaviour
{
    private Button button;
    private GameManager gameManager;  // Reference to the GameManager in the scene
    public int difficulty;
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        // Find the GameManager in the scene by name and get its script component
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        // Assign the button click event to trigger SetDifficulty()
        button.onClick.AddListener(SetDifficulty);
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Called when the player clicks the button.
    // Sends the selected difficulty to the GameManager's StartGame() function.
     void SetDifficulty()
    {
        Debug.Log(gameObject.name + " was clicked");
        gameManager.StartGame(difficulty);
    }
}
