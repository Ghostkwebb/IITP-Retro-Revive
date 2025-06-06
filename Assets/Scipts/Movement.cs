using UnityEngine;
using UnityEngine.Tilemaps;

public class Movement : MonoBehaviour
{
    public float moveSpeed = 5f; // Movement speed
    Rigidbody2D rb2d;
    SpriteRenderer spriteRenderer;
    public TileBase wallTile; // Unused variable, consider removing

    private Vector2 targetPosition;
    private bool isMoving = false; // Moving flag
    private Vector2 moveDirection;
    public float raycastDistance = 1f;
    public LayerMask wallLayer;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        targetPosition = transform.position;
    }

    // FixedUpdate is called at a fixed interval, good for physics
    void FixedUpdate()
    {
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

    // Update is called once per frame
    void Update()
    {
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

                if (hit.collider == null)
                {
                    moveDirection = newMoveDirection;
                    targetPosition = (Vector2)transform.position + moveDirection;
                    isMoving = true;
                    UpdateRotationAndFlip(moveDirection);
                }
            }
            else if (!isMoving)
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

    // Updates rotation and sprite flip based on movement direction
    void UpdateRotationAndFlip(Vector2 direction)
    {
        if (direction.y > 0) transform.rotation = Quaternion.Euler(0, 0, 90);
        else if (direction.y < 0) transform.rotation = Quaternion.Euler(0, 0, -90);
        else if (direction.x > 0) transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (direction.x < 0) transform.rotation = Quaternion.Euler(0, 0, 0);

        if (spriteRenderer != null) spriteRenderer.flipX = direction.x < 0; // Flip sprite for left movement
    }
}