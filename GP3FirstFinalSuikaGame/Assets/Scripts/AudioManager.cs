using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class SoundEffect
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0.1f, 3f)]
        public float pitch = 1f;
        public bool loop = false;

        [HideInInspector]
        public AudioSource source;
    }

    public SoundEffect[] soundEffects;

    [Header("Game Sounds")]
    public string dropSound = "Drop";
    public string mergeSound = "Merge";
    public string gameOverSound = "GameOver";
    public string buttonClickSound = "ButtonClick";
    public string backgroundMusic = "BackgroundMusic";

    private void Awake()
    {
        // Create audio sources for each sound effect
        foreach (SoundEffect sound in soundEffects)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
        }

        // Start background music
        Play(backgroundMusic);
    }

    public void Play(string name)
    {
        // Find the sound effect by name
        SoundEffect sound = System.Array.Find(soundEffects, s => s.name == name);

        if (sound == null)
        {
            Debug.LogWarning("Sound effect " + name + " not found!");
            return;
        }

        // Play the sound
        sound.source.Play();
    }

    public void Stop(string name)
    {
        // Find the sound effect by name
        SoundEffect sound = System.Array.Find(soundEffects, s => s.name == name);

        if (sound == null)
        {
            Debug.LogWarning("Sound effect " + name + " not found!");
            return;
        }

        // Stop the sound
        sound.source.Stop();
    }

    public void PlayDropSound()
    {
        Play(dropSound);
    }

    public void PlayMergeSound()
    {
        Play(mergeSound);
    }

    public void PlayGameOverSound()
    {
        Play(gameOverSound);
    }

    public void PlayButtonClickSound()
    {
        Play(buttonClickSound);
    }
}
