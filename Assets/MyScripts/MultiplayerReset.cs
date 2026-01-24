using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class MultiplayerReset : MonoBehaviour
{
    private async void Awake()
    {
        // 1. Si había sesión de auth, cerrarla
        if (AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignOut();
        }

        // 2. Reinicializar Unity Services
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }
    }
}
