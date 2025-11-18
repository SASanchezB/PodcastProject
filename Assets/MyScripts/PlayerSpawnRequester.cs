using UnityEngine;
using Unity.Netcode;

public class PlayerSpawnRequester : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // SOLO en el servidor hacemos la asignación (evita race/client-side moves)
        if (IsServer)
        {
            // Pedimos al sistema central que nos coloque en un spawn
            if (PlayerSpawnSystem.Instance != null)
            {
                PlayerSpawnSystem.Instance.AssignSpawnForPlayer(this.NetworkObject);
            }
            else
            {
                Debug.LogWarning("PlayerSpawnRequester: PlayerSpawnSystem.Instance es null.");
            }
        }
    }
}
