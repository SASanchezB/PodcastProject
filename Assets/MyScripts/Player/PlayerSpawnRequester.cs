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
                // Si existe, se sienta
                PlayerSpawnSystem.Instance.AssignSpawnForPlayer(this.NetworkObject);
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Warning por si falla
        Debug.LogWarning($"PlayerSpawnRequester: PlayerSpawnSystem.Instance no apareció después de {waitForInstanceTimeout}s. El player podría no posicionarse.");
    }
}