using TMPro;
using Unity.Netcode;
using UnityEngine;
using System.Text;

public class PlayerListUI : MonoBehaviour
{
    [SerializeField] private TMP_Text playerListText;

    private void Start()
    {
        // Escuchamos cuando cualquier nombre cambia
        NetworkPlayerName.OnAnyNameChanged += UpdatePlayerList;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientChanged;
        }

        UpdatePlayerList();
    }

    private void OnDestroy()
    {
        NetworkPlayerName.OnAnyNameChanged -= UpdatePlayerList;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientChanged;
        }
    }

    private void OnClientChanged(ulong clientId)
    {
        UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        if (playerListText == null) return;
        if (NetworkManager.Singleton == null) return;

        StringBuilder sb = new StringBuilder();

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObject = client.PlayerObject;

            if (playerObject != null)
            {
                NetworkPlayerName nameComponent = playerObject.GetComponent<NetworkPlayerName>();

                if (nameComponent != null)
                {
                    string playerName = nameComponent.GetCurrentName();

                    if (nameComponent.IsReady())
                        sb.AppendLine($"<color=green>{playerName}</color>");
                    else
                        sb.AppendLine(playerName);
                }
            }
        }

        playerListText.text = sb.ToString();
    }
}