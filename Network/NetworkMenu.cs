using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class NetworkMenu : MonoBehaviour
{
    public Button hostButton;
    public Button joinButton;
    public TMP_InputField ipInputField;
    public TMP_Text feedbackText;

    private void Start()
    {
        hostButton.onClick.AddListener(StartHost);
        joinButton.onClick.AddListener(JoinGame);

        // Event registrieren, falls Spieler beitreten
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void StartHost()
    {
        Debug.Log("Starte Host...");
        NetworkManager.Singleton.StartHost();
        feedbackText.text = "Warte auf Spieler...";
    }

    private void JoinGame()
    {
        string ip = ipInputField.text.Trim();

        if (string.IsNullOrEmpty(ip))
        {
            feedbackText.text = "Bitte eine gültige IP-Adresse eingeben!";
            return;
        }

        Debug.Log($"Verbinde zu {ip}...");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ip, 7777);
        NetworkManager.Singleton.StartClient();
        feedbackText.text = "Verbinde zum Host...";
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} ist beigetreten!");

        // Prüfe, ob mindestens 2 Spieler online sind (Host + Client)
        if (NetworkManager.Singleton.ConnectedClientsList.Count >= 2)
        {
            Debug.Log("Spieler gefunden - Starte GameScene!");
            NetworkManager.Singleton.SceneManager.LoadScene("LocalGame", LoadSceneMode.Single);
        }
    }
}
