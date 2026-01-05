using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

public class NetworkPlayerName : NetworkBehaviour
{
    [SerializeField] private TMP_Text nameText;

    private NetworkVariable<FixedString128Bytes> playerName =
        new NetworkVariable<FixedString128Bytes>(
            new FixedString128Bytes("Player"),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn()
    {
        playerName.OnValueChanged += OnNameChanged;

        nameText.text = playerName.Value.ToString();

        // 🔥 SOLO el player local aplica el nombre pendiente
        if (IsOwner && !string.IsNullOrWhiteSpace(PlayerNameUI.PendingPlayerName))
        {
            Debug.Log("[NetworkPlayerName] Aplicando nombre pendiente: " +
                      PlayerNameUI.PendingPlayerName);

            SubmitNameServerRpc(PlayerNameUI.PendingPlayerName);
        }
    }

    private void OnNameChanged(FixedString128Bytes oldValue, FixedString128Bytes newValue)
    {
        nameText.text = newValue.ToString();
    }

    [ServerRpc]
    private void SubmitNameServerRpc(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            newName = "Player";

        playerName.Value = new FixedString128Bytes(newName);
    }
}
