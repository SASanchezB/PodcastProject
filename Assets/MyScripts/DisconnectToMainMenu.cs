using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DisconnectToMainMenu : MonoBehaviour
{
    [SerializeField] private string mainMenuScene = "MainMenu";

    public void Disconnect()
    {
        /*
        // 1. Salir de Vivox
        if (VivoxService.Instance != null && VivoxService.Instance.IsLoggedIn)
        {
            await VivoxService.Instance.LogoutAsync();
        }

        // 2. Apagar Netcode
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
        }

        // 3. Cerrar Auth
        if (AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignOut();
        }
        */

        // 4. Volver al menú
        SceneManager.LoadScene(mainMenuScene);
    }
}
