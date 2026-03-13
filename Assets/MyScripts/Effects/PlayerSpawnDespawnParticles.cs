using UnityEngine;
using Unity.Netcode;

public class PlayerSpawnDespawnParticles : NetworkBehaviour
{
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private float despawnParticleLifetime = 2f;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            PlaySpawnParticleClientRpc();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            PlayDespawnParticleClientRpc();
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

    [ClientRpc]
    void PlayDespawnParticleClientRpc()
    {
        if (particle == null) return;

        Debug.Log("[DespawnParticle] Playing despawn particle for player " + OwnerClientId);

        Transform t = particle.transform;

        // guardar posición antes de desparentar
        Vector3 pos = t.position;

        t.SetParent(null);

        // restaurar transform limpio
        t.position = pos;
        t.rotation = Quaternion.identity;
        t.localScale = Vector3.one;

        particle.Play();

        Destroy(particle.gameObject, despawnParticleLifetime);
    }
}