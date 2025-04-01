using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    // Singleton instance
    public static GameSettings Instance;

    [Header("Game Modes")]
    public bool isTimedMode = false;
    public float gameTime = 120f;

    [Header("Difficulty Settings")]
    [Range(0f, 1f)]
    public float bigFruitChance = 0.2f;
    public bool allowBigFruitStart = false;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;
    [Range(0f, 1f)]
    public float soundEffectsVolume = 0.8f;
    public bool musicEnabled = true;
    public bool soundEffectsEnabled = true;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadSettings()
    {
        // Load settings from PlayerPrefs
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        soundEffectsVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        soundEffectsEnabled = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;

        // Apply audio settings
        ApplyAudioSettings();
    }

    public void SaveSettings()
    {
        // Save settings to PlayerPrefs
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", soundEffectsVolume);
        PlayerPrefs.SetInt("MusicEnabled", musicEnabled ? 1 : 0);
        PlayerPrefs.SetInt("SFXEnabled", soundEffectsEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ApplyAudioSettings()
    {
        // Find audio manager
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            // Apply settings to audio manager
            foreach (AudioManager.SoundEffect sound in audioManager.soundEffects)
            {
                if (sound.loop)
                {
                    // Background music
                    sound.source.volume = musicVolume;
                    sound.source.mute = !musicEnabled;
                }
                else
                {
                    // Sound effects
                    sound.source.volume = soundEffectsVolume;
                    sound.source.mute = !soundEffectsEnabled;
                }
            }
        }
    }

    public void SetTimedMode(bool isTimedMode)
    {
        this.isTimedMode = isTimedMode;
    }

    public void SetGameTime(float seconds)
    {
        gameTime = seconds;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        ApplyAudioSettings();
    }

    public void SetSoundEffectsVolume(float volume)
    {
        soundEffectsVolume = volume;
        ApplyAudioSettings();
    }

    public void ToggleMusic(bool enabled)
    {
        musicEnabled = enabled;
        ApplyAudioSettings();
    }

    public void ToggleSoundEffects(bool enabled)
    {
        soundEffectsEnabled = enabled;
        ApplyAudioSettings();
    }
}
