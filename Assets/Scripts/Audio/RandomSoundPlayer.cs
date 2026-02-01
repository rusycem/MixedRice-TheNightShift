using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio; // Required for AudioMixerGroup

public class RandomSoundPlayer : MonoBehaviour
{
    public List<AudioClip> clips;

    [Header("Volume & Mixer")]
    public AudioMixerGroup outputGroup; // Drag your 'SFX' Mixer Group here
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Pitch Settings")]
    public bool randomPitch = true;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    [Header("3D Sound Settings")]
    public float minDistance = 1f;    // Distance where sound starts falling off
    public float maxDistance = 20f;   // Distance where sound becomes silent
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

    private AudioSource source;

    void Awake()
    {
        // Add the component and configure for 3D immediately
        source = gameObject.AddComponent<AudioSource>();

        source.spatialBlend = 1.0f;        // 1.0 = Fully 3D
        source.dopplerLevel = 0f;          // Usually 0 for footsteps/AI to avoid weird warping
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.rolloffMode = rolloffMode;
        source.playOnAwake = false;

        // --- NEW: Assign the Mixer Group ---
        if (outputGroup != null)
        {
            source.outputAudioMixerGroup = outputGroup;
        }
    }

    public void Play()
    {
        if (clips == null || clips.Count == 0) return;

        // Pick a random clip
        int index = Random.Range(0, clips.Count);
        source.clip = clips[index];

        // Apply pitch variation
        if (randomPitch)
            source.pitch = Random.Range(minPitch, maxPitch);

        // --- NEW: Apply Volume ---
        source.volume = volume;

        source.Play();
    }
}