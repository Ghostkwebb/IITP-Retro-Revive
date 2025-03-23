using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class GhostAI : MonoBehaviour
{
    public float moveSpeed = 5f;
    Rigidbody2D rb2d;
    SpriteRenderer spriteRenderer; 
    public LayerMask wallLayer;
    public float raycastDistance = 1f;

    private Vector2 targetPosition;
    private bool isMoving = false;
    private Vector2 moveDirection;
    private GameObject pacman;
    private LevelGenerator levelGenerator;
    private Color originalColor;
    private bool isVulnerable = false; 

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        originalColor = spriteRenderer.color; 
        targetPosition = transform.position;
        FindPacman();
        FindLevelGenerator();
    }

    void FindPacman()
    {
        pacman = GameObject.FindGameObjectWithTag("Player");
        if (pacman == null)
        {
            Debug.LogError("Pacman GameObject not found with tag 'Player'. Make sure Pacman is tagged correctly.");
        }
    }

    void FindLevelGenerator()
    {
        levelGenerator = Object.FindFirstObjectByType<LevelGenerator>();
        if (levelGenerator == null)
        {
            Debug.LogError("LevelGenerator not found in the scene.");
        }
    }

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

    void Update()
    {
        if (pacman == null || levelGenerator == null) return;

        if (isVulnerable)
        {
            spriteRenderer.color = Color.green;
        }
        else
        {
            spriteRenderer.color = originalColor; 
        }

        if (!isMoving)
        {
            DecideMoveDirection();
        }
    }

    void DecideMoveDirection()
    {
        Vector2 pacmanPosition = pacman.transform.position;
        Vector2 ghostPosition = transform.position;
        Vector2 directionToPacman = (pacmanPosition - ghostPosition).normalized;

        Vector2Int[] possibleDirections = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        Vector2 bestDirection = Vector2.zero;
        float bestDirectionDot = -1f;

        foreach (Vector2Int dir in possibleDirections)
        {
            Vector2 rayDirection = dir;
            Vector2 origin = transform.position;
            Vector2 potentialNextPosition = origin + rayDirection;

            Collider2D wallColliderAtNextPos = Physics2D.OverlapPoint(potentialNextPosition, wallLayer);

            Color rayColor = Color.green;
            if (wallColliderAtNextPos != null)
            {
                rayColor = Color.red;
            }
            Debug.DrawRay(origin, rayDirection, rayColor, 0.1f);

            if (wallColliderAtNextPos == null)
            {
                float dotProduct = Vector2.Dot(directionToPacman, rayDirection);
                if (dotProduct > bestDirectionDot)
                {
                    bestDirectionDot = dotProduct;
                    bestDirection = rayDirection;
                }
            }
        }

        if (bestDirection != Vector2.zero)
        {
            SetMove(bestDirection);
            return;
        }

        List<Vector2> availableDirections = new List<Vector2>();
        foreach (Vector2Int dir in possibleDirections)
        {
            Vector2 rayDirection = dir;
            Vector2 origin = transform.position;
            Vector2 potentialNextPosition = origin + rayDirection;
            Collider2D wallColliderAtNextPos = Physics2D.OverlapPoint(potentialNextPosition, wallLayer);

            if (wallColliderAtNextPos == null)
            {
                availableDirections.Add(rayDirection);
            }
        }

        if (availableDirections.Count > 0)
        {
            Vector2 randomDirection = availableDirections[Random.Range(0, availableDirections.Count)];
            SetMove(randomDirection);
        }
    }

    void SetMove(Vector2 direction)
    {
        moveDirection = direction;
        targetPosition = (Vector2)transform.position + moveDirection;
        isMoving = true;
        UpdateRotationAndFlip(moveDirection);
    }


    void UpdateRotationAndFlip(Vector2 direction)
    {
        if (direction.y > 0) transform.rotation = Quaternion.Euler(0, 0, 90);
        else if (direction.y < 0) transform.rotation = Quaternion.Euler(0, 0, -90);
        else if (direction.x > 0) transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (direction.x < 0) transform.rotation = Quaternion.Euler(0, 0, 0);

        if (spriteRenderer != null) spriteRenderer.flipX = direction.x < 0;
    }


    public void SetVulnerable(bool vulnerable) 
    {
        isVulnerable = vulnerable;
    }
}