using UnityEngine;
using Unity.Netcode;
using Unity.Services.Vivox;

public class VoiceParticleSync : NetworkBehaviour
{

    //Nota, funciona en online pero deberia probar con 2 pcs diferentes

    [SerializeField] private ParticleSystem talkingParticle;

    private VivoxParticipant localParticipant;

    private NetworkVariable<bool> isTalking = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[VoiceParticle] Player spawned {OwnerClientId}");

        isTalking.OnValueChanged += OnTalkingChanged;

        if (IsOwner)
        {
            VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
            Debug.Log("[VoiceParticle] Listening for Vivox participants...");
        }
    }

    void OnParticipantAdded(VivoxParticipant participant)
    {
        Debug.Log($"[VoiceParticle] Participant joined channel: {participant.DisplayName}");

        if (!participant.IsSelf) return;

        Debug.Log("[VoiceParticle] Local participant detected");

        localParticipant = participant;
        participant.ParticipantSpeechDetected += OnSpeechDetected;
    }

    void OnSpeechDetected()
    {
        if (localParticipant == null) return;

        bool speaking = localParticipant.SpeechDetected;

        Debug.Log($"[VoiceParticle] Speech detected: {speaking}");

        SetTalkingServerRpc(speaking);
    }

    [ServerRpc]
    void SetTalkingServerRpc(bool talking)
    {
        Debug.Log($"[VoiceParticle] Server received talking: {talking}");

        isTalking.Value = talking;
    }

    void OnTalkingChanged(bool oldValue, bool newValue)
    {
        Debug.Log($"[VoiceParticle] Talking state changed: {newValue}");

        if (talkingParticle == null)
        {
            Debug.LogWarning("[VoiceParticle] Particle not assigned!");
            return;
        }

        if (newValue)
        {
            Debug.Log("[VoiceParticle] PLAY particle");
            talkingParticle.Play();
        }
        else
        {
            Debug.Log("[VoiceParticle] STOP particle");
            talkingParticle.Stop();
        }
    }

    public override void OnDestroy()
    {
        if (localParticipant != null)
        {
            localParticipant.ParticipantSpeechDetected -= OnSpeechDetected;
        }

        if (VivoxService.Instance != null)
        {
            VivoxService.Instance.ParticipantAddedToChannel -= OnParticipantAdded;
        }

        isTalking.OnValueChanged -= OnTalkingChanged;

        base.OnDestroy();
    }
}