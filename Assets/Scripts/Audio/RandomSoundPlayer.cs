using UnityEngine;
using System.Collections.Generic;

public class RandomSoundPlayer : MonoBehaviour
{
    public List<AudioClip> clips;
    public bool randomPitch = true;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    private AudioSource source;

    void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
    }

    public void Play()
    {
        if (clips.Count == 0) return;

        int index = Random.Range(0, clips.Count);
        source.clip = clips[index];

        if (randomPitch)
            source.pitch = Random.Range(minPitch, maxPitch);

        source.Play();
    }
}
