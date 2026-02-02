using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostDisconnectHandler : NetworkBehaviour
{
    [SerializeField] private string mainMenuScene = "MainMenu";

    public void HostDisconnect()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // Envía a todos los clientes al menú
        ForceClientsToMenuClientRpc();

        // El host también se va al menú
        SceneManager.LoadScene(mainMenuScene);
    }

    [ClientRpc]
    private void ForceClientsToMenuClientRpc()
    {
        // Evita que el host ejecute esto dos veces
        if (NetworkManager.Singleton.IsServer) return;

        SceneManager.LoadScene(mainMenuScene);
    }
}