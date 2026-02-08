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
        SceneManager.LoadScene(mainMenuScene);
    }
}
