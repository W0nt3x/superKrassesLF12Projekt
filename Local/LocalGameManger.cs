using UnityEngine;
using TMPro;

public class LocalGameManager : MonoBehaviour
{
    [Header("Spieler Einstellungen")]
    public GameObject player1Prefab;
    public GameObject player2Prefab;
    public Transform player1Spawn;
    public Transform player2Spawn;

    [Header("UI Einstellungen")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI player1NameText;
    public TextMeshProUGUI player2NameText;

    [Header("Spiel Einstellungen")]
    private int scoreTeam1 = 0;
    private int scoreTeam2 = 0;
    private float roundTime = 300f; // 5 Minuten (300 Sekunden)

    private GameObject player1Instance;
    private GameObject player2Instance;

    private string player1Name = "Spieler 1"; // Standardname
    private string player2Name = "Spieler 2"; // Standardname

    private bool goalScored = false; // Sperre für doppelte Torwertung

    void Start()
    {
        LoadPlayerNames();
        SpawnPlayers();
        UpdateScoreText();
        UpdateTimerText(roundTime);
        UpdatePlayerNamesUI();
    }

    void Update()
    {
        if (roundTime > 0)
        {
            roundTime -= Time.deltaTime;
            UpdateTimerText(roundTime);
        }
        else
        {
            ResetGame();
        }
    }

    void LoadPlayerNames()
    {
        player1Name = PlayerPrefs.GetString("Player1Name", "Spieler 1");
        player2Name = PlayerPrefs.GetString("Player2Name", "Spieler 2");
    }

    void SpawnPlayers()
    {
        player1Instance = Instantiate(player1Prefab, player1Spawn.position, Quaternion.identity);
        player2Instance = Instantiate(player2Prefab, player2Spawn.position, Quaternion.identity);
    }

    public void AddScore(int teamID)
    {
        if (goalScored) return;
        goalScored = true;

        if (teamID == 0) scoreTeam2++;
        else scoreTeam1++;

        UpdateScoreText();
        ResetBall();
        ResetPlayers();

        Invoke(nameof(ResetGoalScored), 0.5f);
    }

    void ResetGoalScored()
    {
        goalScored = false;
    }

    void UpdateScoreText()
    {
        scoreText.text = $"{scoreTeam1} - {scoreTeam2}";
    }

    void UpdateTimerText(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    void ResetBall()
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

    void ResetPlayers()
    {
        if (player1Instance != null)
            player1Instance.transform.position = player1Spawn.position;

        if (player2Instance != null)
            player2Instance.transform.position = player2Spawn.position;

        if (player1Instance != null)
            player1Instance.transform.rotation = Quaternion.identity;

        if (player2Instance != null)
            player2Instance.transform.rotation = Quaternion.identity;

        Rigidbody rbPlayer1 = player1Instance?.GetComponent<Rigidbody>();
        Rigidbody rbPlayer2 = player2Instance?.GetComponent<Rigidbody>();

        if (rbPlayer1 != null)
        {
            rbPlayer1.linearVelocity = Vector3.zero;
            rbPlayer1.angularVelocity = Vector3.zero;
        }

        if (rbPlayer2 != null)
        {
            rbPlayer2.linearVelocity = Vector3.zero;
            rbPlayer2.angularVelocity = Vector3.zero;
        }
    }

    void ResetGame()
    {
        scoreTeam1 = 0;
        scoreTeam2 = 0;

        UpdateScoreText();
        ResetPlayers();
        ResetBall();

        roundTime = 300f;
        UpdateTimerText(roundTime);

        Debug.Log("Das Spiel wurde zurückgesetzt!");
    }

    void UpdatePlayerNamesUI()
    {
        if (player1NameText != null)
        {
            player1NameText.text = player1Name;
        }
        if (player2NameText != null)
        {
            player2NameText.text = player2Name;
        }
    }
}