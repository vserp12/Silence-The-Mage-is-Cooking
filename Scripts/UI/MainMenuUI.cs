using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    // Arrastrá el botón desde el Inspector
    public Button playButton;

    void Start()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(PlayGame);
        }
    }

    public void PlayGame()
    {
        // Show the name-input panel which then loads the Game scene
        NameInputManager nim = FindObjectOfType<NameInputManager>();
        if (nim != null)
            nim.ShowNameInput();
        else
            SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}