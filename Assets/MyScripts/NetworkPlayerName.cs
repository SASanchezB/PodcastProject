using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

public class NetworkPlayerName : NetworkBehaviour
{
    [SerializeField] private TMP_Text nameText;

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

        if (IsOwner && !string.IsNullOrWhiteSpace(PlayerNameUI.PendingPlayerName))
        {
            SubmitNameServerRpc(PlayerNameUI.PendingPlayerName);

            // Opcional: limpiar cache
            // PlayerNameUI.PendingPlayerName = null;
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
            newName = " ";

        playerName.Value = new FixedString128Bytes(newName);
    }
}
