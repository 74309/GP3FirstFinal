using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;
    public GameObject settingsPanel;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Toggle musicToggle;
    public Toggle sfxToggle;
    public Button backButton;
    public TextMeshProUGUI highScoreText;
    public Toggle timedModeToggle;
    public Slider gameDurationSlider;
    public TextMeshProUGUI gameDurationText;

    private GameSettings gameSettings;
    private AudioManager audioManager;

    private void Start()
    {
        // Find game settings
        gameSettings = FindObjectOfType<GameSettings>();
        if (gameSettings == null)
        {
            // Create game settings if not found
            GameObject settingsObj = new GameObject("GameSettings");
            gameSettings = settingsObj.AddComponent<GameSettings>();
        }

        // Find audio manager
        audioManager = FindObjectOfType<AudioManager>();

        // Set up button listeners
        playButton.onClick.AddListener(PlayGame);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(QuitGame);
        backButton.onClick.AddListener(CloseSettings);

        // Set up toggle and slider listeners
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(SetMusicVolume);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        if (musicToggle != null)
            musicToggle.onValueChanged.AddListener(ToggleMusic);

        if (sfxToggle != null)
            sfxToggle.onValueChanged.AddListener(ToggleSFX);

        if (timedModeToggle != null)
            timedModeToggle.onValueChanged.AddListener(ToggleTimedMode);

        if (gameDurationSlider != null)
            gameDurationSlider.onValueChanged.AddListener(SetGameDuration);

        // Hide settings panel initially
        settingsPanel.SetActive(false);

        // Set initial values from game settings
        UpdateSettingsUI();

        // Update high score text
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = "High Score: " + highScore.ToString();
    }

    private void UpdateSettingsUI()
    {
        if (gameSettings != null)
        {
            if (musicSlider != null)
                musicSlider.value = gameSettings.musicVolume;

            if (sfxSlider != null)
                sfxSlider.value = gameSettings.soundEffectsVolume;

            if (musicToggle != null)
                musicToggle.isOn = gameSettings.musicEnabled;

            if (sfxToggle != null)
                sfxToggle.isOn = gameSettings.soundEffectsEnabled;

            if (timedModeToggle != null)
                timedModeToggle.isOn = gameSettings.isTimedMode;

            if (gameDurationSlider != null)
            {
                gameDurationSlider.value = gameSettings.gameTime / 60f; // Convert seconds to minutes
                UpdateGameDurationText(gameSettings.gameTime / 60f);
            }
        }
    }

    public void PlayGame()
    {
        // Play button click sound
        if (audioManager != null)
            audioManager.PlayButtonClickSound();

        // Load game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    public void OpenSettings()
    {
        // Play button click sound
        if (audioManager != null)
            audioManager.PlayButtonClickSound();

        // Show settings panel
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        // Play button click sound
        if (audioManager != null)
            audioManager.PlayButtonClickSound();

        // Save settings
        if (gameSettings != null)
            gameSettings.SaveSettings();

        // Hide settings panel
        settingsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        // Play button click sound
        if (audioManager != null)
            audioManager.PlayButtonClickSound();

        // Quit application
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SetMusicVolume(float volume)
    {
        if (gameSettings != null)
            gameSettings.SetMusicVolume(volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (gameSettings != null)
            gameSettings.SetSoundEffectsVolume(volume);
    }

    public void ToggleMusic(bool enabled)
    {
        if (gameSettings != null)
            gameSettings.ToggleMusic(enabled);
    }

    public void ToggleSFX(bool enabled)
    {
        if (gameSettings != null)
            gameSettings.ToggleSoundEffects(enabled);
    }

    public void ToggleTimedMode(bool enabled)
    {
        if (gameSettings != null)
            gameSettings.SetTimedMode(enabled);

        // Enable/disable game duration slider based on timed mode
        if (gameDurationSlider != null)
            gameDurationSlider.interactable = enabled;
    }

    public void SetGameDuration(float minutes)
    {
        if (gameSettings != null)
            gameSettings.SetGameTime(minutes * 60f); // Convert minutes to seconds

        UpdateGameDurationText(minutes);
    }

    private void UpdateGameDurationText(float minutes)
    {
        if (gameDurationText != null)
        {
            int mins = Mathf.FloorToInt(minutes);
            gameDurationText.text = mins.ToString() + " minutes";
        }
    }

    public void ResetHighScore()
    {
        // Reset high score in PlayerPrefs
        PlayerPrefs.SetInt("HighScore", 0);
        PlayerPrefs.Save();

        // Update high score text
        highScoreText.text = "High Score: 0";

        // Play button click sound
        if (audioManager != null)
            audioManager.PlayButtonClickSound();
    }
}



