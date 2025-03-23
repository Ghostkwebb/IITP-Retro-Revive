using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Xml.Serialization;

public class ScoreManager : MonoBehaviour
{
    public int score = 0;
    public TMP_Text scoreText;
    public int winScore = 1000;
    public string winSceneName = "WinScene"; 

    public GameObject winScreenUI;
    public string gameSceneName = "MainScene"; 

    void Start()
    {
        if (PlayerPrefs.HasKey("WinScore"))
        {
            winScore = PlayerPrefs.GetInt("WinScore");
            Debug.Log("Win Score loaded from PlayerPrefs: " + winScore);
        }
        else
        {
            Debug.Log("Win Score PlayerPrefs not found, using default: " + winScore);
        }

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

            if (winScreenUI != null)
            {
                winScreenUI.SetActive(true);
            }

            Time.timeScale = 0f; 
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(gameSceneName); 
    }

    public void ExitGame()
    {
#if UNITY_EDITOR 
        UnityEditor.EditorApplication.isPlaying = false;
#else 
        Application.Quit();
#endif
        Debug.Log("Exit Game Button Clicked"); 
    }

    public void MainMenu(){
        SceneManager.LoadScene("MainMenuScene");
    }
}