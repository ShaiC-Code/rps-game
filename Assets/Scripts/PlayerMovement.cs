using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    private Rigidbody2D rb;
    private Vector2 moveInput;

    [SerializeField] private float moveSpeed = 100f;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        moveInput = new Vector2(
            Mathf.RoundToInt(input.x),
            Mathf.RoundToInt(input.y)
        );
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = Time.deltaTime * moveSpeed * moveInput;
    }
}
