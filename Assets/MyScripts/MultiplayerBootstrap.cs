using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Vivox;
using UnityEngine;

public class MultiplayerBootstrap : MonoBehaviour
{
    private async void Awake()
    {
        // 1. Inicializar Unity Services
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }

        // 2. Autenticación
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        // 3. Inicializar Vivox (NO hay IsInitialized, se llama directo)
        await VivoxService.Instance.InitializeAsync();

        // 4. Login Vivox
        if (!VivoxService.Instance.IsLoggedIn)
        {
            await VivoxService.Instance.LoginAsync();
        }
    }
}
