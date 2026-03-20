using TMPro;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class PlayerListUI : MonoBehaviour
{
    [SerializeField] private Transform container; // objeto con HorizontalLayoutGroup
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

        // 🔴 Limpiar lista anterior
        foreach (var item in spawnedItems)
        {
            Destroy(item);
        }
        spawnedItems.Clear();

        // 🔴 Crear nuevos items
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

                    GameObject obj = Instantiate(playerItemPrefab, container);

                    PlayerListItemUI itemUI = obj.GetComponent<PlayerListItemUI>();

                    bool showKick = true;

                    itemUI.Setup(playerName, isReady, client.ClientId, showKick);

                    spawnedItems.Add(obj);
                }
            }
        }
    }
}