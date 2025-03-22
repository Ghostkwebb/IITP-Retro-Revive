using UnityEngine;
using Mirror;

public class NetworkManagerCustom : NetworkManager
{
    public GameObject levelGeneratorPrefab; // Assign your LevelGenerator prefab in the Inspector

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (levelGeneratorPrefab != null)
        {
            GameObject levelGeneratorInstance = Instantiate(levelGeneratorPrefab);
            NetworkServer.Spawn(levelGeneratorInstance);
        }
        else
        {
            Debug.LogError("LevelGenerator Prefab is not assigned in the NetworkManager!");
        }
    }
}