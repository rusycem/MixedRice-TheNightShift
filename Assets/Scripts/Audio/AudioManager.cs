using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Global Settings")]
    public float globalSFXVolume = 1f;
    public float globalMusicVolume = 1f;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();
    }

    // music
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = globalMusicVolume;
        musicSource.Play();
    }

    // vfx
    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip, globalSFXVolume);
    }

    // random list
    public void PlayRandomSFX(List<AudioClip> clips)
    {
        if (clips.Count == 0) return;

        int index = Random.Range(0, clips.Count);
        sfxSource.PlayOneShot(clips[index], globalSFXVolume);
    }

    // sfx at position
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position)
    {
        AudioSource.PlayClipAtPoint(clip, position, globalSFXVolume);
    }
}
