using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostDisconnectHandler : NetworkBehaviour
{
    [SerializeField] private string mainMenuScene = "MainMenu";

    public void HostDisconnect()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // Envía a todos los clientes al menu
        ForceClientsToMenuClientRpc();

        // El host también se va al menu
        SceneManager.LoadScene(mainMenuScene);
    }

    [ClientRpc]
    private void ForceClientsToMenuClientRpc()
    {
        // Para evitar mandar lo mismo al host
        if (NetworkManager.Singleton.IsServer) return;

        SceneManager.LoadScene(mainMenuScene);
    }
}