using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NetworkUI : MonoBehaviour
{
    public Button HostButton;
    public Button ClientButton;

    void Start()
    {
        HostButton.onClick.AddListener(() => {
            Debug.Log("Host Button Clicked!"); // Debug Log
            NetworkManager.Singleton.StartHost();
            Debug.Log("Host Started");
        });

        ClientButton.onClick.AddListener(() => {
            Debug.Log("Client Button Clicked!"); // Debug Log
            NetworkManager.Singleton.StartClient();
            Debug.Log("Client Started");
        });
    }
}