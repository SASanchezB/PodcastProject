using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class MultiplayerReset : MonoBehaviour
{

    //Creo que obsoleto, no cerraba para el segundo

    private async void Awake()
    {
        // 1 - Si había sesion de auth -> cerrarla
        if (AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignOut();
        }

        // 2 - Reiniciar Unity Services
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }
    }
}
