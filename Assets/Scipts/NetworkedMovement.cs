using UnityEngine;
using Unity.Netcode;

public class NetworkedMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;
    Rigidbody2D rb2d;
    SpriteRenderer spriteRenderer;
    public LayerMask wallLayer;
    private Vector2 targetPosition;
    private bool isMoving = false;
    private Vector2 moveDirection;
    public float raycastDistance = 1f;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        targetPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }


        if (isMoving)
        {
            rb2d.MovePosition(Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.fixedDeltaTime));
            if (Vector2.Distance(transform.position, targetPosition) < 0.01f)
            {
                isMoving = false;
                rb2d.linearVelocity = Vector2.zero;
                transform.position = targetPosition;
            }
        }
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }


        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        Vector2 newMoveDirection = Vector2.zero;

        if (Mathf.Abs(moveX) > 0.5f && Mathf.Abs(moveY) < 0.5f)
        {
            newMoveDirection = new Vector2(moveX, 0f).normalized;
        }
        else if (Mathf.Abs(moveY) > 0.5f && Mathf.Abs(moveX) < 0.5f)
        {
            newMoveDirection = new Vector2(0f, moveY).normalized;
        }

        if (newMoveDirection != Vector2.zero)
        {
            if (newMoveDirection != moveDirection)
            {
                Vector2 origin = transform.position;
                RaycastHit2D hit = Physics2D.Raycast(origin, newMoveDirection, raycastDistance, wallLayer);
                Debug.DrawRay(origin, newMoveDirection * raycastDistance, Color.red, 0.5f);

                if (hit.collider == null)
                {
                    moveDirection = newMoveDirection;
                    targetPosition = (Vector2)transform.position + moveDirection;
                    isMoving = true;
                    UpdateRotationAndFlip(moveDirection);
                    RequestMoveServerRpc(targetPosition); // Request move from server
                }
            }
            else if (!isMoving) // Removed continuous move for simplicity for now
            {
                Vector2 origin = transform.position;
                RaycastHit2D hit = Physics2D.Raycast(origin, moveDirection, raycastDistance, wallLayer);

                if (hit.collider == null)
                {
                    targetPosition = (Vector2)transform.position + moveDirection;
                    isMoving = true;
                }
            }
        }
        if (!isMoving && Mathf.Abs(moveX) < 0.1f && Mathf.Abs(moveY) < 0.1f)
        {
            moveDirection = Vector2.zero;
        }
    }

    [ServerRpc] // This function runs on the server
    void RequestMoveServerRpc(Vector2 targetPos, ServerRpcParams rpcParams = default)
    {
        // Server-side validation could go here

        targetPosition = targetPos; // Server sets the target position
        isMoving = true; // Server sets isMoving
    }

    void UpdateRotationAndFlip(Vector2 direction)
    {
        if (direction.y > 0) transform.rotation = Quaternion.Euler(0, 0, 90);
        else if (direction.y < 0) transform.rotation = Quaternion.Euler(0, 0, -90);
        else if (direction.x > 0) transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (direction.x < 0) transform.rotation = Quaternion.Euler(0, 0, 0);

        if (spriteRenderer != null) spriteRenderer.flipX = direction.x < 0;
    }
}