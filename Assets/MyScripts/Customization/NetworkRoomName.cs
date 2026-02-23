using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;

public class NetworkRoomName : NetworkBehaviour
{
    [SerializeField] private List<TMP_Text> roomNameTexts = new List<TMP_Text>();

    private NetworkVariable<FixedString128Bytes> roomName =
        new NetworkVariable<FixedString128Bytes>(
            new FixedString128Bytes(" "),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn()
    {
        roomName.OnValueChanged += OnRoomNameChanged;

        // Actualizar todos los textos con el valor actual
        UpdateAllTexts(roomName.Value.ToString());

        // Solo el Host setea el nombre inicial
        if (IsServer && !string.IsNullOrWhiteSpace(RoomNameUI.PendingRoomName))
        {
            roomName.Value = new FixedString128Bytes(RoomNameUI.PendingRoomName);
        }
    }

    private void OnDestroy()
    {
        roomName.OnValueChanged -= OnNameChangedSafe;
    }

    private void OnNameChangedSafe(FixedString128Bytes oldValue, FixedString128Bytes newValue)
    {
        UpdateAllTexts(newValue.ToString());
    }

    private void OnRoomNameChanged(FixedString128Bytes oldValue, FixedString128Bytes newValue)
    {
        UpdateAllTexts(newValue.ToString());
    }

    private void UpdateAllTexts(string newText)
    {
        foreach (var text in roomNameTexts)
        {
            if (text != null)
                text.text = newText;
        }
    }
}