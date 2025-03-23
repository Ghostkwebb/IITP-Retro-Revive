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
    private GhostAI[] ghosts;

    public AudioSource coinSoundEffect;
    public AudioSource deathSoundEffect;
    public AudioSource backgroundMusic;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1f; // Ensure time scale is normal at start

        pacmanMovement = GetComponent<Movement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        scoreManager = FindFirstObjectByType<ScoreManager>();
        ghosts = Object.FindObjectsOfType<GhostAI>(true); // Find all GhostAI objects, including inactive ones

        backgroundMusic = GetComponent<AudioSource>();
        if (backgroundMusic == null)
        {
            Debug.LogError("Background Music AudioSource NOT found on Pacman GameObject!");
        }
        else
        {
            if (!backgroundMusic.isPlaying)
            {
                backgroundMusic.Play();
                backgroundMusic.enabled = true;
            }
        }
    }

    // Called when Pacman's collider enters another collider
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ghost"))
        {
            GhostAI ghostAI = other.GetComponent<GhostAI>();
            if (isPoweredUp)
            {
                // Handle collision with ghost when powered up
                if (scoreManager != null)
                {
                    scoreManager.AddScore(ghostEatScore);
                }
                Destroy(other.gameObject); // Destroy the ghost
            }
            else
            {
                // Handle collision with ghost when not powered up (Pacman dies)
                Time.timeScale = 0f; // Freeze game time

                if (backgroundMusic != null)
                {
                    backgroundMusic.enabled = false; // Disable background music
                }
                else
                {
                    Debug.LogWarning("Background Music AudioSource is NOT assigned (but now expecting it on Pacman)!");
                }

                if (deathSoundEffect != null)
                {
                    backgroundMusic.Stop(); // Stop background music again to ensure it's stopped
                    deathSoundEffect.Play(); // Play death sound effect
                }
                else
                {
                    Debug.LogWarning("Death sound effect is NOT assigned! - Sound will not play.");
                }

                StartCoroutine(ReloadSceneWithDelay()); // Start coroutine to reload scene after delay
            }

        }
        else if (other.CompareTag("Coin"))
        {
            // Handle collision with a regular coin
            if (scoreManager != null)
            {
                scoreManager.AddScore(10);
            }
            if (coinSoundEffect != null)
            {
                coinSoundEffect.Play();
            }
            Destroy(other.gameObject); // Destroy the coin
        }
        else if (other.CompareTag("BigCoin"))
        {
            // Handle collision with a power-up coin
            if (scoreManager != null)
            {
                scoreManager.AddScore(50);
            }
            Destroy(other.gameObject); // Destroy the big coin
            StartPowerUp(); // Activate power-up mode
        }
    }

    // Activates Pacman's power-up mode
    void StartPowerUp()
    {
        isPoweredUp = true;

        foreach (GhostAI ghost in ghosts)
        {
            ghost.SetVulnerable(true); // Set ghosts to vulnerable state
        }

        if (powerUpCoroutine != null)
        {
            StopCoroutine(powerUpCoroutine); // Stop any existing power-up timer
        }
        powerUpCoroutine = StartCoroutine(PowerUpTimer()); // Start a new power-up timer coroutine
    }

    // Coroutine to manage the power-up duration
    IEnumerator PowerUpTimer()
    {
        yield return new WaitForSeconds(powerUpDuration);
        EndPowerUp(); // End power-up after duration
    }

    // Deactivates Pacman's power-up mode
    void EndPowerUp()
    {
        isPoweredUp = false;

        foreach (GhostAI ghost in ghosts)
        {
            ghost.SetVulnerable(false); // Revert ghosts to normal state
        }
    }

    // Public method to check if Pacman is powered up
    public bool IsPoweredUp()
    {
        return isPoweredUp;
    }

    // Coroutine to reload the current scene after a delay
    IEnumerator ReloadSceneWithDelay()
    {
        float startTime = Time.unscaledTime; // Record start time using unscaled time
        while (Time.unscaledTime < startTime + 3.2f) // Wait for delay using unscaled time
        {
            yield return null; // Wait for the next frame (unscaled)
        }
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentScene); // Reload the current scene
    }
}