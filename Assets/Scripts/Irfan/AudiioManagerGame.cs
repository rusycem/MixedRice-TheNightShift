using UnityEngine;
using UnityEngine.Audio; // REQUIRED for AudioMixer logic
using System;

// Renamed to 'GameAudioManager' to fix "Ambiguity" errors caused by duplicate scripts
public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager instance;

    [Header("Mixer Settings")]
    public AudioMixer mainMixer;       // Drag your 'MainMixer' asset here
    public AudioMixerGroup bgmGroup;   // Drag 'BGM' group here
    public AudioMixerGroup sfxGroup;   // Drag 'SFX' group here

    [Header("Audio Sources")]
    public AudioSource musicSource;    // Dedicated source for background music
    public AudioSource sfxSource;      // Dedicated source for 2D sound effects

    void Awake()
    {
        // Singleton Pattern
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);

        // Assign the mixer groups to the sources automatically on start
        if (musicSource) musicSource.outputAudioMixerGroup = bgmGroup;
        if (sfxSource) sfxSource.outputAudioMixerGroup = sfxGroup;
    }

    // --- PLAYING SOUNDS ---

    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        // PlayOneShot allows multiple SFX to overlap without cutting each other off
        sfxSource.PlayOneShot(clip);
    }

    // --- VOLUME CONTROL (Connect these to UI Sliders) ---

    public void SetMusicVolume(float sliderValue)
    {
        // Convert slider (0 to 1) to Decibels (-80 to 0)
        // usage: Mathf.Log10(sliderValue) * 20
        float db = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20;

        mainMixer.SetFloat("BGMVolume", db); // "BGMVolume" must match the Exposed Parameter name
    }

    public void SetSFXVolume(float sliderValue)
    {
        float db = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20;
        mainMixer.SetFloat("SFXVolume", db);
    }
}