using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Xml.Serialization; // Note: This namespace is imported but not used in the code. Consider removing it.

public class ScoreManager : MonoBehaviour
{
    public int score = 0;
    public TMP_Text scoreText;
    public int winScore = 1000;
    public string winSceneName = "WinScene";

    public GameObject winScreenUI;
    public string gameSceneName = "MainScene";

    // Start is called before the first frame update
    void Start()
    {
        // Load win score from PlayerPrefs if available, otherwise use default
        if (PlayerPrefs.HasKey("WinScore"))
        {
            winScore = PlayerPrefs.GetInt("WinScore");
            Debug.Log("Win Score loaded from PlayerPrefs: " + winScore);
        }
        else
        {
            Debug.Log("Win Score PlayerPrefs not found, using default: " + winScore);
        }

        // Ensure win screen UI is initially hidden
        if (winScreenUI != null)
        {
            winScreenUI.SetActive(false);
        }
    }

    // Adds points to the current score and updates the score text
    public void AddScore(int points)
    {
        score += points;
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
        CheckWinCondition(); // Check if win condition is met after adding score
    }

    // Checks if the player has reached the win score
    void CheckWinCondition()
    {
        if (score >= winScore)
        {
            Debug.Log("You Win! Score reached: " + score);

            if (winScreenUI != null)
            {
                winScreenUI.SetActive(true); // Show win screen UI
            }

            Time.timeScale = 0f; // Pause the game when win condition is met
        }
    }

    // Restarts the game by loading the game scene
    public void RestartGame()
    {
        Time.timeScale = 1f; // Ensure time scale is normal before restarting
        SceneManager.LoadScene(gameSceneName); // Load the main game scene
    }

    // Exits the game application
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in editor
#else
        Application.Quit(); // Quit the application in a build
#endif
        Debug.Log("Exit Game Button Clicked"); // Debug log for exit button click
    }

    // Loads the main menu scene
    public void MainMenu(){
        SceneManager.LoadScene("MainMenuScene"); // Load the main menu scene
    }
}