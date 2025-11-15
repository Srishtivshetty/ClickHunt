using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

[System.Serializable]
public class GameData
{
    public int coins = 500;
    public int highScore = 0;
    public int remainingAttempts = 3;
}

public class GameManager : MonoBehaviour
{
    [Header("Targets & UI")]
    public List<GameObject> targets;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI gameWinText;
    public Button restartButton;
    public GameObject titleScreen;
    public TextMeshProUGUI highScoreText;
    public Button pauseButton;
    public Button resumeButton;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI levelText;

    [Header("Attempts System")]
    public TextMeshProUGUI attemptsText; //shows remaining attempts
    public int maxAttempts = 3;
    private int currentAttempts;

    [Header("Game Settings")]
    public int gameWinReward = 200; // coins for winning
    private int score;
    private bool isGameActive;
    private bool isGameWon = false;
    private float spawnRate = 3.0f;
    private bool isPaused = false;

    // Level system variables
    private int level = 1;
    private int[] scoreToNextLevel = { 50, 150, 200 };

    // JSON save system
    private GameData gameData; // Store all persistent data
    private string dataPath;  //File path for save life

    // Public properties to access private info externally
    public bool IsGameActive { get { return isGameActive; } }
    public bool IsGameWon { get { return isGameWon; } }
    public int CurrentScore { get { return score; } }
    public int CurrentAttempts { get { return currentAttempts; } }
    public int CurrentLevel { get { return level; } }

    // Unity methods
    void Awake()
    {
        // Buid save file path & load game from JSON
        dataPath = Application.persistentDataPath + "/gamedata.json";
        LoadGameData();
    }

    void Start()
    {
        // Load remaining attempts from save file
        currentAttempts = gameData.remainingAttempts;
        // If out of attempts, prevent game from starting
        if (currentAttempts <= 0)
        {
            Debug.Log("No attempts left! Please return to the lobby and pay entry fee.");
            isGameActive = false;
            if (titleScreen != null) titleScreen.SetActive(true);
        }
        // Hide resume button until paused
        if (resumeButton != null)
            resumeButton.gameObject.SetActive(false);
        // Hook up pause/resume buttons
        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);
        if (resumeButton != null)
            resumeButton.onClick.AddListener(TogglePause);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
        if (gameWinText != null)
            gameWinText.gameObject.SetActive(false);

        UpdateAttemptsText();
    }
    // -Target spawning-
    //Continuously spawns random target objects while the game is active
    IEnumerator SpawnTarget()
    {
        while (isGameActive)
        {
            yield return new WaitForSeconds(spawnRate);
            int index = Random.Range(0, targets.Count);
            Instantiate(targets[index]);
        }
    }
    // --Score & level system--
    public void UpdateScore(int scoreToAdd)
    {
        score += scoreToAdd;
        scoreText.text = "Score: " + score;

        // Level up check
        if (level <= 3 && score >= scoreToNextLevel[level - 1])
            LevelUp();

        CheckGameWin();
    }
    // Upgrades the player's level, increases difficulty, and shows a message
    private void LevelUp()
    {
        if (level < 3)
        {
            level++;
            if (levelText != null) levelText.text = "Level: " + level;

            if (spawnRate > 0.5f)
                spawnRate -= 0.2f;

            StartCoroutine(ShowLevelUpMessage());
        }
    }
    
    // Shows level text 
    private IEnumerator ShowLevelUpMessage()
    {
        if (countdownText != null)
        {
            countdownText.text = "LEVEL " + level;
            countdownText.gameObject.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            countdownText.gameObject.SetActive(false);
        }
    }
     // Checks if the player reached the final level and score requirement
    private void CheckGameWin()
    {
        if (level == 3 && score >= scoreToNextLevel[2] && !isGameWon)
        {
            isGameActive = false;
            Win();

            if (gameWinText != null) gameWinText.gameObject.SetActive(true);

            if (score > gameData.highScore)  // save new high score
            {
                gameData.highScore = score;
                SaveGameData();
                highScoreText.text = "High Score: " + gameData.highScore;
            }

            if (pauseButton != null) pauseButton.gameObject.SetActive(false);
            if (resumeButton != null) resumeButton.gameObject.SetActive(false);
            if (restartButton != null) restartButton.gameObject.SetActive(true);
        }
    }
    // Coins reward and sets win state
    void Win()
    {
        if (isGameWon) return;
        isGameWon = true;

        gameData.coins += gameWinReward;
        SaveGameData();

        Debug.Log("Game Won! Rewarded " + gameWinReward + " coins.");
    }
    // --Game over & attempts--
    // Called when player loses. Handles attempts, saving, UI, and lobby return.
    public void GameOver()
    {
        if (!isGameWon)
        {
            if (gameOverText != null)
                gameOverText.gameObject.SetActive(true);

            isGameActive = false;

            if (restartButton != null) restartButton.gameObject.SetActive(true);
            if (pauseButton != null) pauseButton.gameObject.SetActive(false);
            if (resumeButton != null) resumeButton.gameObject.SetActive(false);

            if (score > gameData.highScore)  // Save high score if new
            {
                gameData.highScore = score;
                SaveGameData();
                highScoreText.text = "High Score: " + gameData.highScore;
            }

            // Reduce attempts
            currentAttempts--;
            if (currentAttempts < 0) currentAttempts = 0;
            gameData.remainingAttempts = currentAttempts;
            SaveGameData();
            UpdateAttemptsText();

            Debug.Log("Game Over! Remaining attempts: " + currentAttempts);
            // If no attempt left then return to lobby
            if (currentAttempts <= 0)
            {
                Debug.Log("No attempts left! Returning to Lobby...");
                StartCoroutine(ReturnToLobby());
            }
        }
    }
    // waits 2 sec then loads the lobby scene
    private IEnumerator ReturnToLobby()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("Lobby"); // Replace with your lobby scene
    }
    // --Restarting and Startinging--
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    // Called when "Play" is pressed. Starts countdown and gameplay.
    public void StartGame(int difficulty)
    {
        if (currentAttempts <= 0)
        {
            Debug.Log("No attempts left! Pay entry fee to reset attempts.");
            return;
        }

        titleScreen.SetActive(false);
        score = 0;
        UpdateScore(0);

        highScoreText.text = "High Score: " + gameData.highScore;

        level = 1;
        if (levelText != null)
            levelText.text = "Level: " + level;

        if (pauseButton != null) pauseButton.gameObject.SetActive(true);
        if (resumeButton != null) resumeButton.gameObject.SetActive(false);

        StartCoroutine(GameStartCountdown(difficulty));
    }
    // 3-second countdown then activates game and spawns targets.
    private IEnumerator GameStartCountdown(int difficulty)
    {
        int countdown = 3;
        if (countdownText != null)
            countdownText.gameObject.SetActive(true);

        while (countdown > 0)
        {
            if (countdownText != null)
                countdownText.text = countdown.ToString();

            yield return new WaitForSeconds(1f);
            countdown--;
        }

        if (countdownText != null)
        {
            countdownText.text = "GO!";
            yield return new WaitForSeconds(1f);
            countdownText.gameObject.SetActive(false);
        }

        isGameActive = true;
        spawnRate /= difficulty; // Difficulty modifies spawn rate
        StartCoroutine(SpawnTarget());
    }
    //--UI Upadates--
    void UpdateAttemptsText()
    {
        if (attemptsText != null)
            attemptsText.text = "Attempts: " + currentAttempts;
    }
    //--Pause System--
    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f; // freezes game
        isPaused = true;
        if (pauseButton != null) pauseButton.gameObject.SetActive(false);
        if (resumeButton != null) resumeButton.gameObject.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f; // unfreezes game
        isPaused = false;
        if (pauseButton != null) pauseButton.gameObject.SetActive(true);
        if (resumeButton != null) resumeButton.gameObject.SetActive(false);
    }

    #region JSON Save/Load
    private void LoadGameData()
    {
        if (File.Exists(dataPath))
        {
            string json = File.ReadAllText(dataPath);
            gameData = JsonUtility.FromJson<GameData>(json);
        }
        else
        {
            gameData = new GameData();  // create default save
            SaveGameData();
        }
    }
    // Saves GameData to the JSON file.
    private void SaveGameData()
    {
        File.WriteAllText(dataPath, JsonUtility.ToJson(gameData));
    }
    #endregion
}
