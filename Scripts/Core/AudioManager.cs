using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public float musicVolume = 0.5f;
    public float sfxVolume = 0.8f;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    void Awake()
    {
        // Singleton: solo puede existir uno en todo el juego
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Sobrevive al cambiar de escena
        DontDestroyOnLoad(gameObject);

        // Creamos las fuentes de audio por código
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.volume = sfxVolume;
    }

    // Cambia la música (solo si es distinta a la que ya suena)
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    // Para el futuro: sonidos cortos (hechizos, golpes, clicks)
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null) sfxSource.PlayOneShot(clip);
    }
}