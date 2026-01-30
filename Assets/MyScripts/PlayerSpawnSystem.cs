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

    private List<Transform> spawnPoints = new List<Transform>();
    private System.Random rng;

    // Singleton simple para que el Player pueda pedir el spawn
    public static PlayerSpawnSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Debug.LogWarning("Más de una instancia de PlayerSpawnSystem detectada.");

        // inicializa el RNG
        rng = new System.Random();

        // Cargar spawn points
        GameObject[] objs = GameObject.FindGameObjectsWithTag(spawnPointTag);
        spawnPoints = objs.Select(o => o.transform).ToList();

        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning($"PlayerSpawnSystem Awake -> NO se encontraron spawn points con tag '{spawnPointTag}'");
        }
        else
        {
            // Barajar lista para evitar patrones repetitivos
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

        // Eliminé el loop que re-ubicaba a todos los clientes conectados.
        // Ahora solo se ubican nuevos jugadores cuando se unen, y los existentes se quedan en su lugar.
        // Si necesitas ubicar a players ya conectados al inicio (por ejemplo, si el GameManager se spawnea tarde),
        // puedes agregar lógica aquí, pero por ahora, asumimos que los players se ubican solo al unirse.
    }

    // ------------Api publica [robustazo]------------
    /// <summary>
    /// Llamar desde el Player cuando su NetworkObject ya existe en el servidor.
    /// </summary>
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

        // Asegurarnos de que tenemos spawn points cargados
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

        playerObject.transform.position = chosen.position;
        //playerObject.transform.rotation = chosen.rotation;
    }

    // ------------Manejo por callbacks (fallbacks)------------
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
                // Si ya existe, asignamos mediante la API pública
                AssignSpawnForPlayer(client.PlayerObject);
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Intento final: si apareció después del timeout
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var connected)
            && connected.PlayerObject != null)
        {
            AssignSpawnForPlayer(connected.PlayerObject);
        }
        else
        {
            Debug.LogWarning($"PlayerSpawnSystem: no se encontró PlayerObject para client {clientId} después de esperar {waitForPlayerTimeout}s.");
        }
    }

    // -----------LOGICA DE SELECCION DE SILLA DE SPAWN-------------
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

        // FALLBACK ALEATORIO
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

    // Fisher–Yates shuffle usando rng
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