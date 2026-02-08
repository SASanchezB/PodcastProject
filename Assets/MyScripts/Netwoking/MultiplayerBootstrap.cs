using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Vivox;
using UnityEngine;

public class MultiplayerBootstrap : MonoBehaviour
{

    // Obsoleto Tambien

    private async void Awake()
    {
        // Prende unity service
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }

        // Autenticacion
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        // Prende vivox
        await VivoxService.Instance.InitializeAsync();

        // Login Vivox
        if (!VivoxService.Instance.IsLoggedIn)
        {
            await VivoxService.Instance.LoginAsync();
        }
    }
}
