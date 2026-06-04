using UnityEngine;
using UnityEngine.SceneManagement;

public class Audio : MonoBehaviour
{
    [Header("--------------------------Audio-------------------------------")]
    [SerializeField] AudioSource source;   // Background music source
    [SerializeField] AudioSource SFXSource; // For sound effects

    [Header("--------------------------AudioCLIP-------------------------------")]
    public AudioClip background;
    public AudioClip bomb;
    public AudioClip power;
    public AudioClip pop;

    public static Audio Instance; // Singleton reference

    private void Awake()
    {
        // Singleton pattern so only one Audio instance exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // ❌ Removed DontDestroyOnLoad so music doesn't carry into other scenes
        // DontDestroyOnLoad(gameObject);

        // Optional: stop music when scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (source != null && background != null)
        {
            source.clip = background;
            source.loop = true; // ✅ Make background music loop forever
            source.Play();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Stop background music when leaving the menu scene
        if (scene.name != "Menu") // 🔹 replace "Menu" with your menu scene name
        {
            if (source.isPlaying)
                source.Stop();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && SFXSource != null)
            SFXSource.PlayOneShot(clip);
    }
}
