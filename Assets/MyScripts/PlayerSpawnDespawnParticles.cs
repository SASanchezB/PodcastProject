using UnityEngine;
using Unity.Netcode;

public class PlayerSpawnDespawnParticles : NetworkBehaviour
{
    [SerializeField] private ParticleSystem particle;

    public override void OnNetworkSpawn()
    {
        // Solo el servidor decide cuándo reproducir la partícula
        if (IsServer)
        {
            PlaySpawnParticleClientRpc();
        }
    }

    [ClientRpc]
    void PlaySpawnParticleClientRpc()
    {
        if (particle != null)
        {
            Debug.Log("[SpawnParticle] Playing spawn particle for player " + OwnerClientId);
            particle.Play();
        }
    }
}