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

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        targetPosition = transform.position;
        FindPacman(); // Find and store reference to Pacman GameObject
        FindLevelGenerator(); // Find and store reference to LevelGenerator
    }

    // Finds and stores a reference to the Pacman GameObject
    void FindPacman()
    {
        pacman = GameObject.FindGameObjectWithTag("Player");
        if (pacman == null)
        {
            Debug.LogError("Pacman GameObject not found with tag 'Player'. Make sure Pacman is tagged correctly.");
        }
    }

    // Finds and stores a reference to the LevelGenerator GameObject
    void FindLevelGenerator()
    {
        levelGenerator = Object.FindFirstObjectByType<LevelGenerator>();
        if (levelGenerator == null)
        {
            Debug.LogError("LevelGenerator not found in the scene.");
        }
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
        if (pacman == null || levelGenerator == null) return;

        if (isVulnerable)
        {
            spriteRenderer.color = Color.green; // Change color to green when vulnerable
        }
        else
        {
            spriteRenderer.color = originalColor; // Revert to original color when not vulnerable
        }

        if (!isMoving)
        {
            DecideMoveDirection(); // Decide the next move direction if not currently moving
        }
    }

    // Decides the next move direction for the Ghost AI
    void DecideMoveDirection()
    {
        Vector2 pacmanPosition = pacman.transform.position;
        Vector2 ghostPosition = transform.position;
        Vector2 directionToPacman = (pacmanPosition - ghostPosition).normalized; // Direction from ghost to Pacman

        Vector2Int[] possibleDirections = new Vector2Int[] // Possible movement directions: Up, Down, Left, Right
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        Vector2 bestDirection = Vector2.zero;
        float bestDirectionDot = -1f;

        // Iterate through possible directions to find the best one towards Pacman
        foreach (Vector2Int dir in possibleDirections)
        {
            Vector2 rayDirection = dir;
            Vector2 origin = transform.position;
            Vector2 potentialNextPosition = origin + rayDirection;

            Collider2D wallColliderAtNextPos = Physics2D.OverlapPoint(potentialNextPosition, wallLayer); // Check for walls in the potential next position

            Color rayColor = Color.green;
            if (wallColliderAtNextPos != null)
            {
                rayColor = Color.red;
            }
            Debug.DrawRay(origin, rayDirection, rayColor, 0.1f); // Draw debug rays for visualization

            if (wallColliderAtNextPos == null) // If no wall in the way
            {
                float dotProduct = Vector2.Dot(directionToPacman, rayDirection); // Calculate dot product to measure direction alignment with Pacman
                if (dotProduct > bestDirectionDot) // If this direction is more aligned with Pacman
                {
                    bestDirectionDot = dotProduct;
                    bestDirection = rayDirection;
                }
            }
        }

        if (bestDirection != Vector2.zero) // If a best direction towards Pacman is found
        {
            SetMove(bestDirection); // Move in the best direction
            return;
        }

        // If no direct path to Pacman, choose a random available direction
        List<Vector2> availableDirections = new List<Vector2>();
        foreach (Vector2Int dir in possibleDirections)
        {
            Vector2 rayDirection = dir;
            Vector2 origin = transform.position;
            Vector2 potentialNextPosition = origin + rayDirection;
            Collider2D wallColliderAtNextPos = Physics2D.OverlapPoint(potentialNextPosition, wallLayer); // Check for walls in the potential next position

            if (wallColliderAtNextPos == null) // If no wall in the way
            {
                availableDirections.Add(rayDirection); // Add direction to available directions
            }
        }

        if (availableDirections.Count > 0) // If there are available directions
        {
            Vector2 randomDirection = availableDirections[Random.Range(0, availableDirections.Count)]; // Choose a random direction
            SetMove(randomDirection); // Move in the random direction
        }
    }

    // Sets the move direction and target position for the Ghost
    void SetMove(Vector2 direction)
    {
        moveDirection = direction;
        targetPosition = (Vector2)transform.position + moveDirection;
        isMoving = true;
        UpdateRotationAndFlip(moveDirection); // Update rotation and sprite flip based on direction
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

    // Sets the vulnerability state of the Ghost
    public void SetVulnerable(bool vulnerable)
    {
        isVulnerable = vulnerable;
    }
}