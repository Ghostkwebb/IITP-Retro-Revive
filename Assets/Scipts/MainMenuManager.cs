using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public string gameSceneName = "MainScene";
    public TMP_InputField winScoreInputField;
    public Slider volumeSlider;
    public int maxWinScore = 5000;

    private GameObject persistentMusicObject;

    // Start is called before the first frame update
    void Start()
    {
        volumeSlider.value = AudioListener.volume; // Initialize volume slider to current audio listener volume

        persistentMusicObject = GameObject.Find("PersistentBackgroundMusic"); // Find the persistent music object
        if (persistentMusicObject == null)
        {
            persistentMusicObject = new GameObject("PersistentBackgroundMusic"); // Create persistent music object if not found
            AudioSource audioSource = persistentMusicObject.AddComponent<AudioSource>(); // Add AudioSource to the created object
            Debug.LogWarning("PersistentBackgroundMusic GameObject not found in scene, creating it dynamically. Ensure it's properly configured in MainMenuScene.");
        }

        AudioSource musicSource = persistentMusicObject.GetComponent<AudioSource>(); // Get AudioSource from persistent music object
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.Play(); // Play music if it's not already playing
        }

        DontDestroyOnLoad(persistentMusicObject); // Make the music object persistent across scenes
    }

    // Starts the game, loads game scene and sets win score
    public void PlayGame()
    {
        int winScore = 1000; // Default win score
        if (int.TryParse(winScoreInputField.text, out int parsedScore)) // Try to parse win score from input field
        {
            winScore = Mathf.Max(1, parsedScore); // Ensure win score is at least 1
            winScore = Mathf.Min(winScore, maxWinScore); // Limit win score to maxWinScore
        }
        else
        {
            Debug.LogWarning("Invalid Win Score input, using default value.");
        }
        PlayerPrefs.SetInt("WinScore", winScore); // Save win score to PlayerPrefs
        PlayerPrefs.Save(); // Save PlayerPrefs to disk

        SceneManager.LoadScene(gameSceneName); // Load the game scene
    }

    // Sets the overall game volume based on the volume slider value
    public void SetVolume()
    {
        AudioListener.volume = volumeSlider.value; // Set AudioListener volume to slider value
    }

    // Exits the game application
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in editor
#else
        Application.Quit(); // Quit the application in a build
#endif
    }
}