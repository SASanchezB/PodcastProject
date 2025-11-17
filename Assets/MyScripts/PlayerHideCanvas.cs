using Unity.Netcode;
using UnityEngine;

public class PlayerHideCanvas : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        // Busca todos los objetos con tag Canvas
        GameObject[] canvases = GameObject.FindGameObjectsWithTag("Canvas");

        foreach (var canvas in canvases)
        {
            canvas.SetActive(false);
        }
    }
}
