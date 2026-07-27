using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    private HashSet<AudioRequestElement>  _frameAudioRequestElements = new HashSet<AudioRequestElement>();
    [SerializeField] private AudioSource audioSource;
    
    
    public static AudioManager Instance{get; private set;}

    private void LateUpdate()
    {
        
        foreach (AudioRequestElement frameAudioRequestElement in _frameAudioRequestElements)
        {
            audioSource.pitch = Random.Range(0.9f, 1f); 
            PlaySound(frameAudioRequestElement.audioClip, frameAudioRequestElement.position);
        }
        _frameAudioRequestElements.Clear();
    }

    private void Awake()
    {
        Instance = this;
    }
    public void PlayAudioOneShot(AudioClip clip, Vector3 position)
    {
        _frameAudioRequestElements.Add(new AudioRequestElement { audioClip = clip, position = position });
    }

    public void PlaySound(AudioClip clip, Vector3 position)
    {
        transform.position = position;
        audioSource.PlayOneShot(clip);
    }
}
