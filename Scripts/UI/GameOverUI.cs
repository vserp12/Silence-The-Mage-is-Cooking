using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public GameObject deathPanel; // El panel que creamos
    public Button restartButton;  // Botón Reintentar
    public Button menuButton;     // Botón Menú

    void Start()
    {
        // Asignamos las funciones a los botones
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
            
        if (menuButton != null)
            menuButton.onClick.AddListener(GoToMenu);
    }

    // Esta función la llamamos cuando el jugador muere
    public void ShowGameOver()
    {
        deathPanel.SetActive(true); // Mostramos el panel
        Time.timeScale = 0f;        // Pausamos el juego (el tiempo se detiene)
    }

    void RestartGame()
    {
        Time.timeScale = 1f; // ¡Muy importante! Reactivamos el tiempo antes de cargar
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Recarga la escena actual
    }

    void GoToMenu()
    {
        Time.timeScale = 1f; // Reactivamos el tiempo
        SceneManager.LoadScene("MainMenu"); // Vuelve al menú
    }
}