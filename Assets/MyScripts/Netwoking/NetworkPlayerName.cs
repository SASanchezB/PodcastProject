using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

public class NetworkPlayerName : NetworkBehaviour
{
    [SerializeField] private TMP_Text nameText;

    // Evento global para avisar cuando cualquier nombre cambia
    public static System.Action OnAnyNameChanged;

    private NetworkVariable<FixedString128Bytes> playerName =
        new NetworkVariable<FixedString128Bytes>(
            new FixedString128Bytes(" "),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn()
    {
        playerName.OnValueChanged += OnNameChanged;
        nameText.text = playerName.Value.ToString();

        // Avisamos que este jugador ya existe
        OnAnyNameChanged?.Invoke();

        if (IsOwner && !string.IsNullOrWhiteSpace(PlayerNameUI.PendingPlayerName))
        {
            SubmitNameServerRpc(PlayerNameUI.PendingPlayerName);
        }
    }

    public override void OnNetworkDespawn()
    {
        playerName.OnValueChanged -= OnNameChanged;

        // Avisamos que un jugador se fue
        OnAnyNameChanged?.Invoke();
    }

    private void OnNameChanged(FixedString128Bytes oldValue, FixedString128Bytes newValue)
    {
        nameText.text = newValue.ToString();

        // Avisamos que cambió un nombre
        OnAnyNameChanged?.Invoke();
    }

    [ServerRpc]
    private void SubmitNameServerRpc(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            newName = " ";

        playerName.Value = new FixedString128Bytes(newName);
    }

    public string GetCurrentName()
    {
        return playerName.Value.ToString();
    }
}