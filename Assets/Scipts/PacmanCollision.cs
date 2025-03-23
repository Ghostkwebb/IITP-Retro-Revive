// PacmanCollision.cs (Updated - Restart Background Music on Scene Load)
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

    // Audio Sources for sound effects (Drag these in Inspector)
    public AudioSource coinSoundEffect;
    public AudioSource deathSoundEffect;
    public AudioSource backgroundMusic; // Background music is now on Pacman itself

    void Start()
    {
        Time.timeScale = 1f; // **RESET Time.timeScale to 1f at the start of the scene**

        pacmanMovement = GetComponent<Movement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        scoreManager = FindFirstObjectByType<ScoreManager>();
        ghosts = Object.FindObjectsOfType<GhostAI>(true); // Obsolete warning fix

        // Get background music AudioSource from THIS GameObject (Pacman)
        backgroundMusic = GetComponent<AudioSource>();
        if (backgroundMusic == null)
        {
            Debug.LogError("Background Music AudioSource NOT found on Pacman GameObject!");
        }
        else
        {
            if (!backgroundMusic.isPlaying) // **START BACKGROUND MUSIC IF NOT ALREADY PLAYING - ADDED HERE**
            {
                backgroundMusic.Play();
                Debug.Log("Background Music Started (or restarted on scene load).");
            }
        }
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
                Debug.Log("Pacman ate a ghost! (No Respawn)");
            }
            else
            {
                // Pacman is not powered up, game over
                Debug.Log("Pacman is about to die! - Checking death sound effect");

                Time.timeScale = 0f; // **IMMEDIATE GAME PAUSE - ADDED HERE**

                if (backgroundMusic != null)
                {
                    backgroundMusic.Stop(); // Stop background music on death
                    Debug.Log("Background Music Stopped.");
                }
                else
                {
                    Debug.LogWarning("Background Music AudioSource is NOT assigned (but now expecting it on Pacman)!");
                }

                if (deathSoundEffect != null)
                {
                    Debug.Log("Death sound effect IS assigned.");
                    Debug.Log("Attempting to play death sound effect...");
                    deathSoundEffect.Play();
                    Debug.Log("deathSoundEffect.Play() called.");
                }
                else
                {
                    Debug.LogWarning("Death sound effect is NOT assigned! - Sound will not play.");
                }

                StartCoroutine(ReloadSceneWithDelay()); // Call coroutine for delayed reload
                Debug.Log("Ghost ate Pacman! - Scene Reload initiated with delay.");
            }

        }
        else if (other.CompareTag("Coin"))
        {
            // Collect the coin
            if (scoreManager != null)
            {
                scoreManager.AddScore(10);
            }
            if (coinSoundEffect != null)
            {
                coinSoundEffect.Play();
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

    IEnumerator ReloadSceneWithDelay()
    {
        float startTime = Time.unscaledTime; // Record unscaled start time
        while (Time.unscaledTime < startTime + 3.2f) // Loop based on unscaled time
        {
            yield return null; // Wait for the next frame (unscaled)
        }
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentScene);
    }
}