// ScoreManager.cs (Modified - Exit Button Functionality)
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public int score = 0;
    public TMP_Text scoreText;
    public int winScore = 1000;
    public string winSceneName = "WinScene"; // If you have a WinScene

    public GameObject winScreenUI;
    public TMP_Text finalScoreTextUI;
    public string gameSceneName = "MainScene"; // Add this line, assuming your game scene is "MainScene"

    void Start()
    {
        // Make sure win screen is initially hidden
        if (winScreenUI != null)
        {
            winScreenUI.SetActive(false);
        }
    }

    public void AddScore(int points)
    {
        score += points;
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
        CheckWinCondition();
    }

    void CheckWinCondition()
    {
        if (score >= winScore)
        {
            Debug.Log("You Win! Score reached: " + score);

            // Activate the Win Screen UI
            if (winScreenUI != null)
            {
                winScreenUI.SetActive(true);

                // Optional: Update the Final Score Text on the Win Screen
                if (finalScoreTextUI != null)
                {
                    finalScoreTextUI.text = "Final Score: " + score.ToString();
                }
            }

            Time.timeScale = 0f; // Pause the game when win screen appears
        }
    }

    // New function for Restart Button
    public void RestartGame()
    {
        Time.timeScale = 1f; // Unpause the game
        SceneManager.LoadScene(gameSceneName); // Load your game scene (e.g., "MainScene")
    }

    // New function for Exit Button
    public void ExitGame()
    {
#if UNITY_EDITOR // Check if running in the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in the editor
#else // If running in a built game
        Application.Quit(); // Quit the application (works in builds)
#endif
        Debug.Log("Exit Game Button Clicked"); // Optional debug log
    }
}