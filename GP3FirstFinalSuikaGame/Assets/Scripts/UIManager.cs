
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;
    public Button restartButton;
    public Button menuButton;

    [Header("Game Settings")]
    public float gameTime = 120f; // 2 minutes per game
    public bool isTimedMode = false;

    private GameManager gameManager;
    private float timeRemaining;
    private bool isGameActive = false;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in the scene!");
            return;
        }

        // Set up button listeners
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (menuButton != null)
            menuButton.onClick.AddListener(ReturnToMenu);

        // Initialize UI
        gameOverPanel.SetActive(false);

        // Start game timer if timed mode is active
        if (isTimedMode)
        {
            timeRemaining = gameTime;
            UpdateTimerText();
            isGameActive = true;
        }
        else
        {
            if (timerText != null)
                timerText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isTimedMode && isGameActive)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerText();

            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                EndGame();
            }
        }
    }

    private void UpdateTimerText()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // Change color when time is running out
            if (timeRemaining <= 30)
                timerText.color = Color.red;
            else
                timerText.color = Color.white;
        }
    }

    private void EndGame()
    {
        isGameActive = false;
        gameOverPanel.SetActive(true);

        // Notify game manager
        if (gameManager != null)
        {
            // Call game over method in game manager
            SendMessage("GameOver", SendMessageOptions.DontRequireReceiver);
        }
    }

    public void RestartGame()
    {
        // Reset timer if in timed mode
        if (isTimedMode)
        {
            timeRemaining = gameTime;
            UpdateTimerText();
            isGameActive = true;
        }

        gameOverPanel.SetActive(false);

        // Restart the game
        if (gameManager != null)
        {
            SendMessage("ResetGame", SendMessageOptions.DontRequireReceiver);
        }
    }

    public void ReturnToMenu()
    {
        // Load menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score.ToString();
    }

    public void UpdateHighScore(int highScore)
    {
        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore.ToString();
    }

    public void ShowGameOver(int finalScore)
    {
        gameOverPanel.SetActive(true);

        // Find and update game over score text
        TextMeshProUGUI gameOverScoreText = gameOverPanel.GetComponentInChildren<TextMeshProUGUI>();
        if (gameOverScoreText != null)
            gameOverScoreText.text = "Game Over!\nFinal Score: " + finalScore;
    }
}
