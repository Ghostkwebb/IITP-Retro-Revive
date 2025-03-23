using UnityEngine;
using UnityEngine.SceneManagement;

public class PacmanCollision : MonoBehaviour
{
    public Sprite ghostSprite; // Assign the ghost sprite in the Inspector
    private Movement pacmanMovement;
    private SpriteRenderer spriteRenderer;
    private ScoreManager scoreManager;

    void Start()
    {
        pacmanMovement = GetComponent<Movement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        scoreManager = FindFirstObjectByType<ScoreManager>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ghost"))
        {
            int currentScene = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentScene);

        }
        else if (other.CompareTag("Coin"))
        {
            // Collect the coin
            if (scoreManager != null)
            {
                scoreManager.AddScore(10); // Add 10 points for a regular coin
            }
            Destroy(other.gameObject); // Destroy the coin
        }
        else if (other.CompareTag("BigCoin"))
        {
            // Collect the big coin
            if (scoreManager != null)
            {
                scoreManager.AddScore(50); // Add 50 points for a big coin
            }
            Destroy(other.gameObject); // Destroy the big coin
            // You might want to trigger the power-up effect here
        }
    }
}