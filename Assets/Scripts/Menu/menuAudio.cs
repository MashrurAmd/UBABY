using UnityEngine;

public class MenuMusic : MonoBehaviour
{
    public AudioClip backgroundMusic; // assign your music file in inspector
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.playOnAwake = true;
        audioSource.volume = 0.5f; // adjust volume as needed
        audioSource.Play();
    }
}