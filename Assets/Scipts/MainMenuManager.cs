using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public string gameSceneName = "MainScene"; // Name of your game scene
    public TMP_InputField winScoreInputField;
    public Slider volumeSlider;

    void Start()
    {
        // Initialize volume slider to current AudioListener volume
        volumeSlider.value = AudioListener.volume;
    }

    public void PlayGame()
    {
        // Save Win Score to PlayerPrefs
        int winScore = 1000; // Default win score
        if (int.TryParse(winScoreInputField.text, out int parsedScore))
        {
            winScore = Mathf.Max(1, parsedScore); // Ensure win score is at least 1
        }
        PlayerPrefs.SetInt("WinScore", winScore);
        PlayerPrefs.Save(); // Important to save PlayerPrefs

        SceneManager.LoadScene(gameSceneName);
    }

    public void SetVolume()
    {
        AudioListener.volume = volumeSlider.value;
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}