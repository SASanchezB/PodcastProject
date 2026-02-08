using UnityEngine;
using Unity.Netcode;

public class PlayerScalerOnSpawn : NetworkBehaviour
{
    public Vector3 desiredScale = new Vector3(50f, 50f, 50f);

    public override void OnNetworkSpawn()
    {
        // Solo el Owner modifica su propio objeto al spawnear
        if (!IsOwner) return;

        transform.localScale = desiredScale;
    }
}
