using Unity.Netcode;
using UnityEngine;

public class PlayerKeyBool : NetworkBehaviour
{
    public NetworkVariable<bool> triggeredButtonKey = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Update()
    {
        // Solo el dueño del player maneja input
        if (!IsOwner) return;

        // Script para probar con la P, la use en debug
        /*
        if (!triggeredButtonKey.Value && Input.GetKeyDown(KeyCode.P))
        {
            SubmitTriggerServerRpc();
        }
        */
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitTriggerServerRpc()
    {
        triggeredButtonKey.Value = true;
    }
}