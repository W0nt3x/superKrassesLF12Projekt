using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NetworkPlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 10f;
    public float sprintMultiplier = 1.5f;
    public float rotationSpeed = 10f;
    public float accelerationTime = 0.1f;
    public float decelerationTime = 0.1f;
    public float ballDetectionRadius = 2f;
    public float heightAdjustmentSpeed = 5f;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private Vector3 velocityRef;
    public NetworkVariable<int> teamID = new NetworkVariable<int>(-1);

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        if (!IsOwner) return;

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        moveDirection = new Vector3(-moveZ, 0f, moveX).normalized * currentSpeed;

        if (moveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }

        AdjustHeightToBall();
    }


    private void FixedUpdate()
    {
        if (!IsOwner || rb == null) return;

        rb.linearVelocity = Vector3.SmoothDamp(rb.linearVelocity, moveDirection, ref velocityRef, HasInput() ? accelerationTime : decelerationTime);

        // Sende Geschwindigkeit an den Server für physikalische Berechnungen
        SendPlayerVelocityServerRpc(rb.linearVelocity, NetworkManager.Singleton.LocalClientId);
    }


    private bool HasInput()
    {
        return moveDirection.magnitude > 0.1f;
    }

    private void AdjustHeightToBall()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, ballDetectionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Ball"))
            {
                float ballHeight = hitCollider.transform.position.y;
                float playerHeight = transform.position.y;
                float targetHeight = ballHeight > playerHeight ? ballHeight + 0.1f : playerHeight;
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, targetHeight, transform.position.z), heightAdjustmentSpeed * Time.deltaTime);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendPlayerVelocityServerRpc(Vector3 velocity, ulong clientId)
    {
        if (!IsServer) return;

        GameObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = velocity;
        }
    }



}
