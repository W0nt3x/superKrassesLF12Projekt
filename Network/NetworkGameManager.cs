using Unity.Netcode;
using UnityEngine;
using TMPro;

public class NetworkGameManager : NetworkBehaviour
{
    [Header("Spieler Einstellungen")]
    public GameObject playerPrefab;
    public Transform[] team1SpawnPoints;
    public Transform[] team2SpawnPoints;

    [Header("UI Einstellungen")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;

    private NetworkVariable<int> scoreTeam1 = new NetworkVariable<int>(0);
    private NetworkVariable<int> scoreTeam2 = new NetworkVariable<int>(0);
    private NetworkVariable<float> roundTime = new NetworkVariable<float>(300f);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Für bereits verbundene Clients:
            SpawnPlayers();
            // Callback registrieren für nachträglich verbundene Clients:
            NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayerForClient;
        }
    }

    private void SpawnPlayerForClient(ulong clientId)
    {
        // Prüfe, ob bereits ein Spielerobjekt für diesen Client existiert
        if (NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId) != null)
        {
            Debug.Log($"Spieler für Client {clientId} existiert bereits.");
            return;
        }

        // Teamzuweisung, Spawnpoint etc.
        int teamID = (int)clientId % 2;
        Transform spawnPoint = teamID == 0
            ? team1SpawnPoints[Random.Range(0, team1SpawnPoints.Length)]
            : team2SpawnPoints[Random.Range(0, team2SpawnPoints.Length)];

        GameObject playerInstance = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        playerInstance.GetComponent<NetworkPlayerMovement>().teamID.Value = teamID;

        Debug.Log($"Neuer Spieler {clientId} wurde in Team {teamID} gespawnt.");
    }




    void Update()
    {
        if (IsServer && roundTime.Value > 0)
        {
            roundTime.Value -= Time.deltaTime;
        }
        UpdateTimerTextClientRpc(roundTime.Value);
    }

    void SpawnPlayers()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            int teamID = (int)client.ClientId % 2; // Oder eine andere Logik für die Teamzuweisung
            Transform spawnPoint = teamID == 0
                ? team1SpawnPoints[(int)(client.ClientId % (ulong)team1SpawnPoints.Length)]
                : team2SpawnPoints[(int)(client.ClientId % (ulong)team2SpawnPoints.Length)];

            GameObject playerInstance = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
            playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.ClientId);
            playerInstance.GetComponent<NetworkPlayerMovement>().teamID.Value = teamID;

            Debug.Log($"Spieler {client.ClientId} wurde in Team {teamID} gespawnt.");
        }
    }



    [ServerRpc]
    public void AddScoreServerRpc(int teamID)
    {
        if (teamID == 0)
            scoreTeam2.Value++;
        else
            scoreTeam1.Value++;

        // Stelle sicher, dass die UI auf allen Clients aktualisiert wird
        UpdateScoreTextClientRpc(scoreTeam1.Value, scoreTeam2.Value);
        ResetBallServerRpc();
        ResetPlayersServerRpc();
    }

    [ClientRpc]
    void UpdateScoreTextClientRpc(int team1Score, int team2Score)
    {
        scoreText.text = $"{team1Score} - {team2Score}";
    }

    [ServerRpc]
    void ResetGameServerRpc()
    {
        scoreTeam1.Value = 0;
        scoreTeam2.Value = 0;
        roundTime.Value = 300f;
        UpdateScoreTextClientRpc(scoreTeam1.Value, scoreTeam2.Value);
        UpdateTimerTextClientRpc(roundTime.Value);
        ResetBallServerRpc();
        ResetPlayersServerRpc();
    }

    [ClientRpc]
    void UpdateTimerTextClientRpc(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    // Ball zurücksetzen
    [ServerRpc]
    public void ResetBallServerRpc()
    {
        // Auf allen Clients Ball zurücksetzen
        ResetBallClientRpc();
    }

    [ClientRpc]
    void ResetBallClientRpc()
    {
        GameObject ball = GameObject.FindGameObjectWithTag("Ball");
        if (ball)
        {
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
                ball.transform.position = Vector3.zero;
                ball.transform.rotation = Quaternion.identity;
                rb.isKinematic = false;
            }
        }
    }
    // Spieler zurücksetzen
    [ServerRpc]
    public void ResetPlayersServerRpc()
    {
        // Auf allen Clients Spieler zurücksetzen
        ResetPlayersClientRpc();
    }

    [ClientRpc]
    void ResetPlayersClientRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            int team = player.GetComponent<NetworkPlayerMovement>().teamID.Value;
            if (team == -1)
            {
                team = 0; // Standardmäßig zu Team 1 zuweisen
            }
            Transform spawnPoint = team == 0
                ? team1SpawnPoints[Random.Range(0, team1SpawnPoints.Length)]
                : team2SpawnPoints[Random.Range(0, team2SpawnPoints.Length)];

            player.transform.position = spawnPoint.position;
            player.transform.rotation = Quaternion.identity;

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}