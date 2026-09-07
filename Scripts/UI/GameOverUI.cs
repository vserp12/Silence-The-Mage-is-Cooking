using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public GameObject deathPanel;
    public Button restartButton;
    public Button menuButton;
    public TextMeshProUGUI waveReachedText; // optional, set by scene setup

    void Start()
    {
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        if (menuButton != null)    menuButton.onClick.AddListener(GoToMenu);
    }

    public void ShowGameOver()
    {
        deathPanel.SetActive(true);
        Time.timeScale = 0f;

        if (waveReachedText != null && WaveManager.Instance != null)
            waveReachedText.text = $"You reached wave {WaveManager.Instance.CurrentWave}!";
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}