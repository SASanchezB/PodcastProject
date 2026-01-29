using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnRequester : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer && PlayerSpawnSystem.Instance != null)
        {
            PlayerSpawnSystem.Instance.OnPlayerObjectSpawned(NetworkObject);
        }
    }
}
