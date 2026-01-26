using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostDisconnectHandler : NetworkBehaviour
{
    [SerializeField] private string mainMenuScene = "MainMenu";

    /// <summary>
    /// Llamar esto SOLO cuando el host decide salir
    /// </summary>
    public void HostDisconnect()
    {
        if (!IsServer) return;

        // Avisar a todos los clientes
        ForceClientsToMenuClientRpc();

        // El host también se va
        LeaveToMenu();
    }

    [ClientRpc]
    private void ForceClientsToMenuClientRpc(ClientRpcParams rpcParams = default)
    {
        // Evitamos que el host ejecute esto dos veces
        if (IsServer) return;

        LeaveToMenu();
    }

    private void LeaveToMenu()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        SceneManager.LoadScene(mainMenuScene);
    }
}
