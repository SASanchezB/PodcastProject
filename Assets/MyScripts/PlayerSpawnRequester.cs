using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerSpawnRequester : NetworkBehaviour
{
    [Tooltip("Tiempo máximo (seg) para esperar a que PlayerSpawnSystem.Instance exista")]
    public float waitForInstanceTimeout = 5f;  // Ajusta si es necesario

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // SOLO en el servidor intentamos asignar el spawn
        if (IsServer)
        {
            StartCoroutine(TryAssignSpawnCoroutine());
        }
    }

    private IEnumerator TryAssignSpawnCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < waitForInstanceTimeout)
        {
            if (PlayerSpawnSystem.Instance != null)
            {
                // Una vez que existe, asignamos el spawn
                PlayerSpawnSystem.Instance.AssignSpawnForPlayer(this.NetworkObject);
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Si no apareció después del timeout, logueamos un warning
        Debug.LogWarning($"PlayerSpawnRequester: PlayerSpawnSystem.Instance no apareció después de {waitForInstanceTimeout}s. El player podría no posicionarse.");
    }
}