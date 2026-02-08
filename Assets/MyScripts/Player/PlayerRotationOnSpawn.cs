using UnityEngine;
using Unity.Netcode;

public class PlayerRotatorOnSpawn : NetworkBehaviour
{
    [Header("Rotación deseada (solo aplicada por el servidor)")]
    public Vector3 desiredRotation = new Vector3(0f, 180f, 0f);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // El servidor es dueño de la transformación inicial
            transform.rotation = Quaternion.Euler(desiredRotation);
        }
    }
}
