using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMusic : MonoBehaviour
{
    public AudioClip music;

    void Start()
    {
        if (AudioManager.Instance == null)
        {
            GameObject audioObj = new GameObject("AudioManager");
            audioObj.AddComponent<AudioManager>();
        }

        // Auto-resolve music if missing
        if (music == null)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "Game")
            {
                #if UNITY_EDITOR
                music = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/GameMusic.mp3");
                #endif
                if (music == null) music = Resources.Load<AudioClip>("Audio/GameMusic") ?? Resources.Load<AudioClip>("GameMusic");
            }
            else if (sceneName == "MainMenu")
            {
                #if UNITY_EDITOR
                music = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/MenuMusic.mp3");
                #endif
                if (music == null) music = Resources.Load<AudioClip>("Audio/MenuMusic") ?? Resources.Load<AudioClip>("MenuMusic");
            }
        }

        if (music != null)
        {
            AudioManager.Instance.PlayMusic(music);
        }
    }
}