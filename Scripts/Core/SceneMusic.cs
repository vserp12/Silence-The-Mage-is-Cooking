using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    public AudioClip music;

    void Start()
    {
        // Si el AudioManager no existe (por ejemplo, si das Play directo en Game), lo crea
        if (AudioManager.Instance == null)
        {
            GameObject audioObj = new GameObject("AudioManager");
            audioObj.AddComponent<AudioManager>();
        }

        AudioManager.Instance.PlayMusic(music);
    }
}