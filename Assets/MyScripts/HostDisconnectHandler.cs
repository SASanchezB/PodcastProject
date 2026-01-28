using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostDisconnectHandler : NetworkBehaviour
{
    [SerializeField] private string mainMenuScene = "MainMenu";

    public void HostDisconnect()
    {
        if (!IsServer) return;

        // Avisar a todos los clientes
        ForceClientsToMenuClientRpc();

        // El host también se va
        LeaveToMenu();
    }

    [ClientRpc]
    private void ForceClientsToMenuClientRpc()
    {
        // Evitamos que el host ejecute esto dos veces
        if (IsServer) return;

        LeaveToMenu();
    }

    private void LeaveToMenu()
    {

        SceneManager.LoadScene(mainMenuScene);
    }
}
