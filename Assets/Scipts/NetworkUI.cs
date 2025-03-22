using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections; // Required for Coroutines

public class NetworkUI : MonoBehaviour
{
    public Button HostButton;
    public Button ClientButton;
    public GameObject levelGeneratorPrefab;

    void Start()
    {
        HostButton.onClick.AddListener(() => {
            Debug.Log("Host Button Clicked!");
            StartCoroutine(StartHostAndSpawnLevel()); // Use Coroutine for delay and order
        });

        ClientButton.onClick.AddListener(() => {
            Debug.Log("Client Button Clicked!");
            NetworkManager.Singleton.StartClient();
            Debug.Log("Client Started");
        });
    }

    // Coroutine to start host and then spawn LevelGenerator with a small delay
    IEnumerator StartHostAndSpawnLevel()
    {
        NetworkManager.Singleton.StartHost(); // Start Host FIRST
        Debug.Log("Host Started (Coroutine)");
        yield return new WaitForSeconds(0.1f); // Small delay to ensure NetworkManager is ready
        SpawnLevelGenerator(); // Then spawn LevelGenerator
    }


    void SpawnLevelGenerator()
    {
        if (levelGeneratorPrefab != null)
        {
            GameObject levelGeneratorObject = Instantiate(levelGeneratorPrefab);
            NetworkObject levelGeneratorNetworkObject = levelGeneratorObject.GetComponent<NetworkObject>();

            if (levelGeneratorNetworkObject != null)
            {
                levelGeneratorNetworkObject.Spawn();
                Debug.Log("LevelGenerator NetworkObject Spawned!");
            }
            else
            {
                Debug.LogError("LevelGenerator prefab does not have a NetworkObject component!");
            }
        }
        else
        {
            Debug.LogError("Level Generator Prefab is not assigned in NetworkUI!");
        }
    }
}