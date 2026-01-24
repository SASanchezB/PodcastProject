using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class SessionSceneLoader : MonoBehaviour
{
    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        // SOLO el host cambia de escena
        if (!NetworkManager.Singleton.IsHost)
            return;

        NetworkManager.Singleton.SceneManager.LoadScene(
            "Gameplay",
            LoadSceneMode.Single
        );
    }
}
