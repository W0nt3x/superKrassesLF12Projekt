using Unity.Netcode;
using UnityEngine;

public class NetworkGoalTrigger : NetworkBehaviour
{
    public int teamID; // 0 = Team 1, 1 = Team 2
    public AudioClip goalSound;
    private AudioSource audioSource;

    private NetworkVariable<bool> hasScored = new NetworkVariable<bool>(false); // NetworkVariable für das Tor

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = goalSound;
        audioSource.playOnAwake = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball") && !hasScored.Value)
        {
            Debug.Log("Tor erkannt!");

            if (IsServer)
            {
                hasScored.Value = true; // Setzen als NetworkVariable auf dem Server
                NetworkGameManager gameManager = Object.FindFirstObjectByType<NetworkGameManager>();
                if (gameManager != null)
                {
                    gameManager.AddScoreServerRpc(teamID);
                }
                Debug.Log("Server setzt Punkte.");
                Invoke(nameof(ResetScoreFlag), 1.0f); // Verzögertes Zurücksetzen
            }

            PlayGoalSoundClientRpc();
        }
    }

    private void ResetScoreFlag()
    {
        if (IsServer)
        {
            hasScored.Value = false; // NetworkVariable zurücksetzen
        }
    }

    [ClientRpc]
    private void PlayGoalSoundClientRpc()
    {
        if (audioSource != null && goalSound != null)
        {
            audioSource.Play();
        }
    }
}
