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

    void Start()
    {
        Time.timeScale = 1f; 

        pacmanMovement = GetComponent<Movement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        scoreManager = FindFirstObjectByType<ScoreManager>();
        ghosts = Object.FindObjectsOfType<GhostAI>(true); 

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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ghost"))
        {
            GhostAI ghostAI = other.GetComponent<GhostAI>();
            if (isPoweredUp)
            {
                if (scoreManager != null)
                {
                    scoreManager.AddScore(ghostEatScore);
                }
                Destroy(other.gameObject); 
            }
            else
            {

                Time.timeScale = 0f;

                if (backgroundMusic != null)
                {
                    backgroundMusic.enabled = false;
                }
                else
                {
                    Debug.LogWarning("Background Music AudioSource is NOT assigned (but now expecting it on Pacman)!");
                }

                if (deathSoundEffect != null)
                {
                    backgroundMusic.Stop(); 
                    deathSoundEffect.Play();
                }
                else
                {
                    Debug.LogWarning("Death sound effect is NOT assigned! - Sound will not play.");
                }

                StartCoroutine(ReloadSceneWithDelay()); 
            }

        }
        else if (other.CompareTag("Coin"))
        {
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
        float startTime = Time.unscaledTime; 
        while (Time.unscaledTime < startTime + 3.2f) 
        {
            yield return null; // Wait for the next frame (unscaled)
        }
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentScene);
    }
}