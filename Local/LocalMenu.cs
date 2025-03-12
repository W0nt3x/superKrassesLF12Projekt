using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LocalMainMenu : MonoBehaviour
{
    public Button startGameButton;
    public TMP_InputField nameInputField1;
    public TMP_InputField nameInputField2;
    public TMP_Text localFeedbackText;

    private void Start()
    {
        startGameButton.onClick.AddListener(StartLocalGame);
    }

    private void StartLocalGame()
    {
        string playerName1 = nameInputField1.text;
        string playerName2 = nameInputField2.text;

        // �berpr�fe, ob beide Namen ausgef�llt sind
        if (string.IsNullOrEmpty(playerName1) || string.IsNullOrEmpty(playerName2))
        {
            localFeedbackText.text = "Bitte gib beide Spielernamen ein!"; // Fehlermeldung anzeigen
            return; // Starte das Spiel nicht
        }

        // Spielernamen in PlayerPrefs speichern
        PlayerPrefs.SetString("Player1Name", playerName1);
        PlayerPrefs.SetString("Player2Name", playerName2);
        PlayerPrefs.Save(); // Speichern der �nderungen

        SceneManager.LoadScene("LocalGame");
    }
}