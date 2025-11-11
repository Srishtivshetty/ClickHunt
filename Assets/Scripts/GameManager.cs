using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public TextMeshProUGUI attemptsText;
    public int maxAttempts = 3;
    public int currentAttempts;

    [Header("Game Settings")]
    public int gameWinReward = 200; // coins for winning
    private int highScore;
    public bool isGameActive;
    public bool isGameWon = false;
    private float spawnRate = 3.0f;
    private int score;
    private bool isPaused = false;

    // Level system variables
    private int level = 1;
    private int[] scoreToNextLevel = { 50, 150, 200 };

    void Start()
    {
        // Load remaining attempts or set to max (only first time)
        currentAttempts = PlayerPrefs.GetInt("RemainingAttempts", maxAttempts);

        // Stop the player if attempts are 0
        if (currentAttempts <= 0)
        {
            Debug.Log("No attempts left! Please return to the lobby and pay entry fee.");
            isGameActive = false;
            if (titleScreen != null) titleScreen.SetActive(true);
        }

        if (resumeButton != null)
            resumeButton.gameObject.SetActive(false);

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

    IEnumerator SpawnTarget()
    {
        while (isGameActive)
        {
            yield return new WaitForSeconds(spawnRate);
            int index = Random.Range(0, targets.Count);
            Instantiate(targets[index]);
        }
    }

    public void UpdateScore(int scoreToAdd)
    {
        score += scoreToAdd;
        scoreText.text = "Score: " + score;

        // Level up check
        if (level <= 3 && score >= scoreToNextLevel[level - 1])
            LevelUp();

        CheckGameWin();
    }

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

    private void CheckGameWin()
    {
        if (level == 3 && score >= scoreToNextLevel[2] && !isGameWon)
        {
            isGameActive = false;
            Win();

            if (gameWinText != null) gameWinText.gameObject.SetActive(true);

            if (score > highScore)
            {
                highScore = score;
                PlayerPrefs.SetInt("HighScore", highScore);
                PlayerPrefs.Save();
                highScoreText.text = "High Score: " + highScore;
            }

            if (pauseButton != null) pauseButton.gameObject.SetActive(false);
            if (resumeButton != null) resumeButton.gameObject.SetActive(false);
            if (restartButton != null) restartButton.gameObject.SetActive(true);
        }
    }

    void Win()
    {
        if (isGameWon) return;
        isGameWon = true;

        int totalCoins = PlayerPrefs.GetInt("Coins", 500);
        totalCoins += gameWinReward;
        PlayerPrefs.SetInt("Coins", totalCoins);
        PlayerPrefs.Save();

        Debug.Log("Game Won! Rewarded " + gameWinReward + " coins.");
    }

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

            if (score > highScore)
            {
                highScore = score;
                PlayerPrefs.SetInt("HighScore", highScore);
                PlayerPrefs.Save();
                highScoreText.text = "High Score: " + highScore;
            }

            //Reduce attempts by 1
            currentAttempts--;
            if (currentAttempts < 0) currentAttempts = 0;
            PlayerPrefs.SetInt("RemainingAttempts", currentAttempts);
            PlayerPrefs.Save();
            UpdateAttemptsText();

            Debug.Log("Game Over! Remaining attempts: " + currentAttempts);

            // If all attempts used, go back to lobby
            if (currentAttempts <= 0)
            {
                Debug.Log("No attempts left! Returning to Lobby...");
                StartCoroutine(ReturnToLobby());
            }
        }
    }

    private IEnumerator ReturnToLobby()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("Lobby"); // Replace with your actual lobby scene name
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

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
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = "High Score: " + highScore;

        level = 1;
        if (levelText != null)
            levelText.text = "Level: " + level;

        if (pauseButton != null) pauseButton.gameObject.SetActive(true);
        if (resumeButton != null) resumeButton.gameObject.SetActive(false);

        StartCoroutine(GameStartCountdown(difficulty));
    }

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
        spawnRate /= difficulty;
        StartCoroutine(SpawnTarget());
    }

    void UpdateAttemptsText()
    {
        if (attemptsText != null)
            attemptsText.text = "Attempts: " + currentAttempts;
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
        if (pauseButton != null)
            pauseButton.gameObject.SetActive(false);
        if (resumeButton != null)
            resumeButton.gameObject.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        if (pauseButton != null)
            pauseButton.gameObject.SetActive(true);
        if (resumeButton != null)
            resumeButton.gameObject.SetActive(false);
    }
}
