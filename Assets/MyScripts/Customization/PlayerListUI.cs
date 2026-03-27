using TMPro;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class PlayerListUI : MonoBehaviour
{
    [SerializeField] private List<Transform> containers;
    [SerializeField] private GameObject playerItemPrefab;

    private List<GameObject> spawnedItems = new List<GameObject>();

    private void Start()
    {
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
        if (NetworkManager.Singleton == null) return;

        // Limpiar instancias anteriores
        foreach (var item in spawnedItems)
        {
            Destroy(item);
        }
        spawnedItems.Clear();

        // Crear nueva lista
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObject = client.PlayerObject;

            if (playerObject != null)
            {
                NetworkPlayerName nameComponent = playerObject.GetComponent<NetworkPlayerName>();

                if (nameComponent != null)
                {
                    string playerName = nameComponent.GetCurrentName();
                    bool isReady = nameComponent.IsReady();

                    bool showKick = NetworkManager.Singleton.IsHost && client.ClientId != NetworkManager.Singleton.LocalClientId;

                    // Instanciar en TODOS los containers
                    for (int i = 0; i < containers.Count; i++)
                    {
                        var container = containers[i];

                        GameObject obj = Instantiate(playerItemPrefab, container);
                        PlayerListItemUI itemUI = obj.GetComponent<PlayerListItemUI>();

                        // Solo permitir botón si:
                        // - Es el primer container (i == 0)
                        // - O si sos host (y no sos vos mismo)
                        bool allowKickButton = (i == 0) && showKick;

                        itemUI.Setup(playerName, isReady, client.ClientId, allowKickButton);

                        spawnedItems.Add(obj);
                    }
                }
            }
        }
    }
}