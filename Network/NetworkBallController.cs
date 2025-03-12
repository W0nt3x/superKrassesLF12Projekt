using Unity.Netcode;
using UnityEngine;

public class NetworkBallController : NetworkBehaviour
{
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return; // Nur der Server berechnet Physik
        if (collision.gameObject.CompareTag("Player"))
        {
            Vector3 force = collision.relativeVelocity * 1.2f; // 1.2x Spieler-Geschwindigkeit als Stoßkraft
            rb.AddForce(force, ForceMode.Impulse);
        }
    }
}
