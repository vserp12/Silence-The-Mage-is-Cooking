using UnityEngine;
using TMPro;

public class LeaderboardUI : MonoBehaviour
{
    // Acá vamos a meter los 5 textos desde el Inspector
    public TextMeshProUGUI[] scoreSlots;

    void Start()
    {
        // 1. Limpiamos todos los textos para que digan "---" al arrancar
        foreach (TextMeshProUGUI text in scoreSlots)
        {
            text.text = "---";
        }

        // 2. Buscamos el nombre que guardamos antes
        string savedName = PlayerPrefs.GetString("PlayerName", "Mago Anónimo");

        // 3. Si hay nombre y hay espacio, lo ponemos en el primer puesto (con puntaje falso de prueba)
        if (scoreSlots.Length > 0 && !string.IsNullOrEmpty(savedName))
        {
            scoreSlots[0].text = $"1. {savedName} - 1000 pts";
        }
    }
}