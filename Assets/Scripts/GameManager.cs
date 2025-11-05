using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public List<GameObject> targets;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameOverText;
    public Button restartButton;
    public GameObject titleScreen;
    public TextMeshProUGUI highScoreText;
    public Button pauseButton;
    public Button resumeButton;
    public TextMeshProUGUI countdownText;

    private int highScore;
    public bool isGameActive;
    private float spawnRate = 3.0f;
    private int score;
    private bool isPaused = false;


    // Start is called before the first frame update
    void Start()
    { 
        
        if (resumeButton != null)
            resumeButton.gameObject.SetActive(false);
        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);
        if (resumeButton != null)
            resumeButton.onClick.AddListener(TogglePause);
        
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

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
   // Update the score
    public void UpdateScore(int scoreToAdd)
    {
        score += scoreToAdd;
        scoreText.text = "Score:" + score;

    }
    public void GameOver()
    {
        gameOverText.gameObject.SetActive(true);
        isGameActive = false;
        Debug.Log("Game Over!");
        restartButton.gameObject.SetActive(true);
        pauseButton.gameObject.SetActive(false);
        resumeButton.gameObject.SetActive(false);

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore); 
            PlayerPrefs.Save();
            highScoreText.text = "High Score: " + highScore;
        }
    }
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void StartGame(int difficulty)
    {
        titleScreen.SetActive(false);
        score = 0;
        UpdateScore(0);
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = "High Score: " + highScore;

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
    // Pause/Resume functions
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
    
