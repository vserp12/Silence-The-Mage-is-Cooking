using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class NameInputManager : MonoBehaviour
{
    public GameObject nameInputPanel;
    public TMP_InputField nameInputField;
    public Button confirmButton;

    private string playerName;

    void Start()
    {
        confirmButton.onClick.AddListener(OnConfirmName);
        
        // Permitir Enter para confirmar
        nameInputField.onSubmit.AddListener(OnSubmitName);
    }

    public void ShowNameInput()
    {
        nameInputPanel.SetActive(true);
        nameInputField.Select();
        nameInputField.ActivateInputField();
    }

    private void OnSubmitName(string name)
    {
        OnConfirmName();
    }

    private void OnConfirmName()
    {
        playerName = nameInputField.text.Trim();
        
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "Mago Anónimo";
        }

        // Guardar el nombre para usarlo después
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();

        Debug.Log("Nombre guardado: " + playerName);

        // Cargar el juego
        SceneManager.LoadScene("Game");
    }
}