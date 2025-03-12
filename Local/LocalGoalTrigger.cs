using UnityEngine;

public class LocalGoalTrigger : MonoBehaviour
{
    public int teamID; // 0 = Team 1, 1 = Team 2
    public AudioClip goalSound; // Hinzugef�gt: AudioClip f�r den Torsound
    private AudioSource audioSource; // Hinzugef�gt: AudioSource-Komponente
    private LocalGameManager gameManager;

    void Start()
    {
        gameManager = Object.FindFirstObjectByType<LocalGameManager>();
        if (gameManager == null)
        {
            Debug.LogError("LocalGameManager nicht im Szene gefunden!");
        }

        // AudioSource-Komponente hinzuf�gen und konfigurieren
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = goalSound;
        audioSource.playOnAwake = false; //manuell abspielen
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            if (gameManager != null)
            {
                gameManager.AddScore(teamID);
                PlayGoalSound(); // Torsound abspielen
            }
        }
    }

    private void PlayGoalSound()
    {
        if (audioSource != null && goalSound != null)
        {
            audioSource.Play();
        }
    }
}