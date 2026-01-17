using UnityEngine;
using Unity.Netcode;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject inGameUIPanel;

    //[Header("Opcional")]
    //[SerializeField] private string playerTag = "Player";

    private bool isPaused;
    private bool gameplayActive;

    private void Awake()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        UnlockCursor();
    }

    private void Update()
    {
        // Detecta si el jugador existe (esto para no pausar en la main screen)
        if (!gameplayActive)
        {
            TryDetectGameplay();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void TryDetectGameplay()
    {
        if (!NetworkManager.Singleton)
            return;

        if (!NetworkManager.Singleton.IsClient)
            return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
                continue;

            if (!client.PlayerObject.IsOwner)
                continue;

            gameplayActive = true;
            LockCursor();
            return;
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;

        if (pausePanel != null)
            pausePanel.SetActive(isPaused);

        if (isPaused)
            UnlockCursor();
        else
            LockCursor();
    }

    // funcion para salir de la sala
    public void Disconnect()
    {
        if (NetworkManager.Singleton == null)
            return;

        bool isHost = NetworkManager.Singleton.IsHost;

        if (isHost)
        {
            // HOST: cierra el server (expulsa a todos)
            NetworkManager.Singleton.Shutdown();

            // El host también vuelve al menú
            ShowMainMenu();
        }
        else
        {
            // CLIENTE: solo se desconecta él
            NetworkManager.Singleton.Shutdown();

            ShowMainMenu();
        }
    }

    // funcion para sacar la pausa (aunque con el escape hace lo mismo)
    public void UnpauseBTN()
    {
        pausePanel.SetActive(false);
        UnlockCursor();
        isPaused = false;
    }


    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ShowMainMenu()
    {
        if (inGameUIPanel != null)
            inGameUIPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        UnlockCursor();
    }
}
