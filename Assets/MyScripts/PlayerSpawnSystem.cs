using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnSystem : NetworkBehaviour
{
    [Tooltip("Tag que usan tus spawn points en la escena")]
    public string spawnPointTag = "SpawnPoints";

    [Tooltip("Radio mínimo para considerar un spawn ocupado")]
    public float occupiedRadius = 1.2f;

    [Tooltip("Tiempo máximo (seg) para esperar a que PlayerObject exista")]
    public float waitForPlayerTimeout = 2f;

    [Tooltip("Rotación inicial que tendrán los jugadores al spawnear (Euler)")]
    public Vector3 spawnRotationEuler = Vector3.zero;

    private List<Transform> spawnPoints = new List<Transform>();
    private System.Random rng;

    public static PlayerSpawnSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Debug.LogWarning("Más de una instancia de PlayerSpawnSystem detectada.");

        rng = new System.Random();

        GameObject[] objs = GameObject.FindGameObjectsWithTag(spawnPointTag);
        spawnPoints = objs.Select(o => o.transform).ToList();

        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning($"PlayerSpawnSystem Awake -> NO se encontraron spawn points con tag '{spawnPointTag}'");
        }
        else
        {
            ShuffleList(spawnPoints);
            Debug.Log($"PlayerSpawnSystem Awake -> encontré {spawnPoints.Count} spawn points con tag '{spawnPointTag}'. Lista barajada.");
        }
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerJoin;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerJoin;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            StartCoroutine(TryMovePlayerCoroutine(client.ClientId));
        }
    }

    public void AssignSpawnForPlayer(NetworkObject playerObject)
    {
        if (!IsServer)
        {
            Debug.LogWarning("AssignSpawnForPlayer llamado desde un lugar que no es servidor. Ignorando.");
            return;
        }

        if (playerObject == null)
        {
            Debug.LogWarning("AssignSpawnForPlayer: playerObject es null.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(spawnPointTag);
            spawnPoints = objs.Select(o => o.transform).ToList();
            if (spawnPoints.Count == 0)
            {
                Debug.LogWarning("AssignSpawnForPlayer: No hay spawn points disponibles.");
                return;
            }
            ShuffleList(spawnPoints);
        }

        Transform chosen = GetFreeSpawnPoint(playerObject.OwnerClientId);

        if (chosen == null)
        {
            Debug.LogWarning("AssignSpawnForPlayer: No se encontró spawn. Se deja la posición actual.");
            return;
        }

        Debug.Log($"AssignSpawnForPlayer: moviendo client {playerObject.OwnerClientId} al spawn '{chosen.name}' en {chosen.position}");

        // Posición
        playerObject.transform.position = chosen.position;

        // Rotación: combinamos la rotación del spawn con la rotación pública que quieras
        Quaternion spawnRot = chosen.rotation;
        Quaternion extraRot = Quaternion.Euler(spawnRotationEuler);
        playerObject.transform.rotation = spawnRot * extraRot;
    }

    private void OnPlayerJoin(ulong clientId)
    {
        if (!IsServer) return;

        Debug.Log($"OnPlayerJoin: clientId={clientId}");
        StartCoroutine(TryMovePlayerCoroutine(clientId));
    }

    private IEnumerator TryMovePlayerCoroutine(ulong clientId)
    {
        float elapsed = 0f;

        while (elapsed < waitForPlayerTimeout)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client)
                && client.PlayerObject != null)
            {
                AssignSpawnForPlayer(client.PlayerObject);
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var connected) && connected.PlayerObject != null)
        {
            AssignSpawnForPlayer(connected.PlayerObject);
        }
        else
        {
            Debug.LogWarning($"PlayerSpawnSystem: no se encontró PlayerObject para client {clientId} después de esperar {waitForPlayerTimeout}s.");
        }
    }

    private Transform GetFreeSpawnPoint(ulong clientId)
    {
        if (spawnPoints == null || spawnPoints.Count == 0) return null;

        int count = spawnPoints.Count;
        int start = rng.Next(0, count);

        Debug.Log($"GetFreeSpawnPoint: startIndex = {start}");

        for (int i = 0; i < count; i++)
        {
            int idx = (start + i) % count;
            Transform t = spawnPoints[idx];

            if (!IsPointOccupied(t.position, clientId))
            {
                Debug.Log($"GetFreeSpawnPoint: elegido '{t.name}' (idx {idx})");
                return t;
            }
        }

        int fallbackIdx = rng.Next(0, count);
        Debug.LogWarning($"GetFreeSpawnPoint: todos ocupados, fallback '{spawnPoints[fallbackIdx].name}'");
        return spawnPoints[fallbackIdx];
    }

    private bool IsPointOccupied(Vector3 position, ulong ignoreClientId)
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId == ignoreClientId) continue;
            if (client.PlayerObject == null) continue;

            float d = Vector3.Distance(client.PlayerObject.transform.position, position);
            if (d < occupiedRadius) return true;
        }

        return false;
    }

    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
