using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player1Movement : NetworkBehaviour
{
    public float moveSpeed = 10f;
    public float sprintMultiplier = 1.5f;
    public float rotationSpeed = 10f;
    [Tooltip("Zeit in Sekunden, um die maximale Geschwindigkeit zu erreichen")]
    public float BeschleunigungsZeit = 0.1f;
    [Tooltip("Zeit in Sekunden, um zum Stillstand zu kommen")]
    public float AbbremsZeit = 0.1f;
    public float ballDetectionRadius = 2f;
    public float heightAdjustmentSpeed = 5f;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private Vector3 aktuelleVelocity;


    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
        else
        {
            Debug.LogError("Rigidbody nicht gefunden!");
        }
    }
    void Start()
    {

    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        float moveX = Input.GetAxis("Horizontal"); // WASD - A und D
        float moveZ = Input.GetAxis("Vertical"); // WASD - W und S
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

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.SmoothDamp(rb.linearVelocity, moveDirection, ref aktuelleVelocity, HasInput() ? BeschleunigungsZeit : AbbremsZeit);
        }
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
                if (ballHeight > playerHeight)
                {
                    float targetHeight = ballHeight + 0.1f; // Füge einen kleinen Offset hinzu
                    transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, targetHeight, transform.position.z), heightAdjustmentSpeed * Time.deltaTime);
                }
                else
                {
                    // Behalte die aktuelle Höhe bei, wenn der Ball niedriger oder gleich hoch ist
                    transform.position = new Vector3(transform.position.x, playerHeight, transform.position.z);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, ballDetectionRadius);
    }
}
