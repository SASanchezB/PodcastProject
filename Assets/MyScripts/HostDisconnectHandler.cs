using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostDisconnectHandler : NetworkBehaviour
{
    [SerializeField] private string mainMenuScene = "MainMenu";

    private void Start()
    {
        // Agrega callback para manejar desconexiones en clientes
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnDestroy()
    {
        // Limpia el callback para evitar errores
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
    }

    public void HostDisconnect()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // Desconecta completamente la sesión
        NetworkManager.Singleton.Shutdown();

        // El host carga el menú después de desconectar
        StartCoroutine(LoadMenuAfterShutdown());
    }

    private System.Collections.IEnumerator LoadMenuAfterShutdown()
    {
        // Espera a que Shutdown termine
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(mainMenuScene);
    }

    private void OnClientDisconnect(ulong clientId)
    {
        // Si este cliente se desconecta (incluyendo por Shutdown del host), carga MainMenu
        if (clientId == NetworkManager.Singleton.LocalClientId && !NetworkManager.Singleton.IsServer)
        {
            SceneManager.LoadScene(mainMenuScene);
        }
    }
}