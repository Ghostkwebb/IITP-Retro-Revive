// PacmanCollision.cs (Modified - No Ghost Respawn)
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PacmanCollision : MonoBehaviour
{
    public Sprite ghostSprite;
    public float powerUpDuration = 5f;
    public int ghostEatScore = 200;

    private Movement pacmanMovement;
    private SpriteRenderer spriteRenderer;
    private ScoreManager scoreManager;
    private bool isPoweredUp = false;
    private Coroutine powerUpCoroutine;

    private GhostAI[] ghosts; // Array to hold all GhostAI scripts in the scene

    void Start()
    {
        pacmanMovement = GetComponent<Movement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        scoreManager = FindFirstObjectByType<ScoreManager>();
        ghosts = FindObjectsOfType<GhostAI>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ghost"))
        {
            GhostAI ghostAI = other.GetComponent<GhostAI>();
            if (isPoweredUp)
            {
                // Pacman is powered up, eat the ghost
                if (scoreManager != null)
                {
                    scoreManager.AddScore(ghostEatScore);
                }
                Destroy(other.gameObject); // Destroy the ghost GameObject - NO RESPAWN
                Debug.Log("Pacman ate a ghost! (No Respawn)"); // Updated log message
            }
            else
            {
                // Pacman is not powered up, game over
                int currentScene = SceneManager.GetActiveScene().buildIndex;
                SceneManager.LoadScene(currentScene);
                Debug.Log("Ghost ate Pacman!");
            }

        }
        else if (other.CompareTag("Coin"))
        {
            // Collect the coin
            if (scoreManager != null)
            {
                scoreManager.AddScore(10);
            }
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("BigCoin"))
        {
            // Collect the big coin and activate power-up
            if (scoreManager != null)
            {
                scoreManager.AddScore(50);
            }
            Destroy(other.gameObject);
            StartPowerUp();
        }
    }

    void StartPowerUp()
    {
        isPoweredUp = true;
        Debug.Log("Power-Up started!");

        foreach (GhostAI ghost in ghosts)
        {
            ghost.SetVulnerable(true);
        }

        if (powerUpCoroutine != null)
        {
            StopCoroutine(powerUpCoroutine);
        }
        powerUpCoroutine = StartCoroutine(PowerUpTimer());
    }

    IEnumerator PowerUpTimer()
    {
        yield return new WaitForSeconds(powerUpDuration);
        EndPowerUp();
    }

    void EndPowerUp()
    {
        isPoweredUp = false;
        Debug.Log("Power-Up ended!");

        foreach (GhostAI ghost in ghosts)
        {
            ghost.SetVulnerable(false);
        }
    }

    public bool IsPoweredUp()
    {
        return isPoweredUp;
    }
}