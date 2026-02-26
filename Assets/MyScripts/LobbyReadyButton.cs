using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyReadyButton : NetworkBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private GameObject panelToDisable;

    private NetworkPlayerName localPlayer;
    private bool localReady = false;

    // 🔥 ESTADO GLOBAL SINCRONIZADO
    private NetworkVariable<bool> gameStarted =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn()
    {
        gameStarted.OnValueChanged += OnGameStartedChanged;

        // 🔥 Si alguien entra tarde y ya empezó
        if (gameStarted.Value)
        {
            panelToDisable.SetActive(false);
        }
    }

    private void Start()
    {
        button.onClick.AddListener(OnButtonClicked);
        NetworkPlayerName.OnAnyNameChanged += OnPlayerStateChanged;

        FindLocalPlayer();
        RefreshButton();
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnButtonClicked);
        NetworkPlayerName.OnAnyNameChanged -= OnPlayerStateChanged;

        gameStarted.OnValueChanged -= OnGameStartedChanged;
    }

    private void FindLocalPlayer()
    {
        if (NetworkManager.Singleton == null) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId == NetworkManager.Singleton.LocalClientId)
            {
                localPlayer = client.PlayerObject.GetComponent<NetworkPlayerName>();
                break;
            }
        }
    }

    private void OnButtonClicked()
    {
        if (localPlayer == null) return;
        if (gameStarted.Value) return; // 🔥 ya empezó

        localReady = !localReady;
        localPlayer.SetReadyServerRpc(localReady);

        RefreshButton();
    }

    private void RefreshButton()
    {
        if (gameStarted.Value)
        {
            button.interactable = false;
            return;
        }

        buttonText.text = localReady ? "Esperar" : "Listo";
        button.interactable = true;
    }

    private void OnPlayerStateChanged()
    {
        if (gameStarted.Value) return;

        if (AreAllPlayersReady())
        {
            if (IsServer)
            {
                StartGame();
            }
        }
    }

    private bool AreAllPlayersReady()
    {
        if (NetworkManager.Singleton == null) return false;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var player = client.PlayerObject.GetComponent<NetworkPlayerName>();

            if (!player.IsReady())
                return false;
        }

        return true;
    }

    private void StartGame()
    {
        gameStarted.Value = true; // 🔥 estado permanente
        ResetAllPlayersReady();
        panelToDisable.SetActive(false);
    }

    private void ResetAllPlayersReady()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var player = client.PlayerObject.GetComponent<NetworkPlayerName>();
            player.ResetReadyServerSide();
        }
    }

    private void OnGameStartedChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            panelToDisable.SetActive(false);
        }
    }
}