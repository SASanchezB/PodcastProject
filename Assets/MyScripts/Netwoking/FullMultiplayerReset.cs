using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Vivox;
using Unity.Netcode;

public class FullMultiplayerReset : MonoBehaviour
{

    //NO SE SI ESTE SCRIPT SE USA O QUEDO OBSOLETO

    private void Awake()
    {
        // 1. Apagar Netcode
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
        }

        // 2. Cerrar Auth (esto sí)
        if (AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignOut();
        }

        // 3. Volver al menú
        SceneManager.LoadScene("MainMenu");
    }
}
