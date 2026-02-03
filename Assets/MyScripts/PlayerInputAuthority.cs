using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputAuthority : NetworkBehaviour
{
    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            playerInput.enabled = false;
        }
        else
        {
            playerInput.enabled = true;
        }
    }
}
