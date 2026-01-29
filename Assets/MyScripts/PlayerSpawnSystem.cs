using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnSystem : NetworkBehaviour
{
    public string spawnPointTag = "SpawnPoints";

    public static PlayerSpawnSystem Instance { get; private set; }

    private List<Transform> spawnPoints = new();

    // Estado autoritativo
    private Dictionary<ulong, int> clientToSpawn = new();
    private Dictionary<int, ulong> spawnToClient = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Debug.LogWarning("Más de un PlayerSpawnSystem.");

        spawnPoints = GameObject
            .FindGameObjectsWithTag(spawnPointTag)
            .Select(o => o.transform)
            .ToList();

        if (spawnPoints.Count == 0)
            Debug.LogError("No hay SpawnPoints en la escena.");
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        // Host inicial
        ulong hostId = NetworkManager.Singleton.LocalClientId;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(hostId, out var host) &&
            host.PlayerObject != null)
        {
            AssignSpawn(host.PlayerObject);
        }
    }

    // 🔔 LLAMADO DESDE PlayerSpawnRequester
    public void OnPlayerObjectSpawned(NetworkObject playerObject)
    {
        if (!IsServer || playerObject == null)
            return;

        AssignSpawn(playerObject);
    }

    // ================= SPAWN CORE =================

    private void AssignSpawn(NetworkObject playerObject)
    {
        ulong clientId = playerObject.OwnerClientId;

        // 🔒 Ya tiene spawn → no tocar
        if (clientToSpawn.TryGetValue(clientId, out int spawnIndex))
        {
            playerObject.transform.position = spawnPoints[spawnIndex].position;
            return;
        }

        int freeIndex = GetFreeSpawnIndex();
        if (freeIndex == -1)
        {
            Debug.LogError("No hay spawn points libres.");
            return;
        }

        clientToSpawn[clientId] = freeIndex;
        spawnToClient[freeIndex] = clientId;

        playerObject.transform.position = spawnPoints[freeIndex].position;
    }

    private int GetFreeSpawnIndex()
    {
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (!spawnToClient.ContainsKey(i))
                return i;
        }
        return -1;
    }

    // ================= DISCONNECT =================

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!clientToSpawn.TryGetValue(clientId, out int spawnIndex))
            return;

        clientToSpawn.Remove(clientId);
        spawnToClient.Remove(spawnIndex);
    }
}
