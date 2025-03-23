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

    void Start()
    {
        volumeSlider.value = AudioListener.volume;

        persistentMusicObject = GameObject.Find("PersistentBackgroundMusic");
        if (persistentMusicObject == null)
        {
            persistentMusicObject = new GameObject("PersistentBackgroundMusic");
            AudioSource audioSource = persistentMusicObject.AddComponent<AudioSource>();
            Debug.LogWarning("PersistentBackgroundMusic GameObject not found in scene, creating it dynamically. Ensure it's properly configured in MainMenuScene.");
        }

        AudioSource musicSource = persistentMusicObject.GetComponent<AudioSource>();
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }

        DontDestroyOnLoad(persistentMusicObject);
    }

    public void PlayGame()
    {
        int winScore = 1000; 
        if (int.TryParse(winScoreInputField.text, out int parsedScore))
        {
            winScore = Mathf.Max(1, parsedScore); 
            winScore = Mathf.Min(winScore, maxWinScore);
        }
        else
        {
            Debug.LogWarning("Invalid Win Score input, using default value.");
        }
        PlayerPrefs.SetInt("WinScore", winScore);
        PlayerPrefs.Save(); 

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