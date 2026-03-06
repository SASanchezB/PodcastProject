using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

public class NetworkPlayerName : NetworkBehaviour
{
    [SerializeField] private TMP_Text nameText;

    public static System.Action OnAnyNameChanged;

    private NetworkVariable<FixedString128Bytes> playerName =
        new NetworkVariable<FixedString128Bytes>(
            new FixedString128Bytes(" "),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private NetworkVariable<bool> isReady =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn()
    {
        playerName.OnValueChanged += OnNameChanged;
        isReady.OnValueChanged += OnReadyChanged;

        UpdateVisual();
        OnAnyNameChanged?.Invoke();

        if (IsOwner && !string.IsNullOrWhiteSpace(PlayerNameUI.PendingPlayerName))
        {
            SubmitNameServerRpc(PlayerNameUI.PendingPlayerName);
        }
    }

    public override void OnNetworkDespawn()
    {
        playerName.OnValueChanged -= OnNameChanged;
        isReady.OnValueChanged -= OnReadyChanged;

        OnAnyNameChanged?.Invoke();
    }

    private void OnNameChanged(FixedString128Bytes oldValue, FixedString128Bytes newValue)
    {
        UpdateVisual();
        OnAnyNameChanged?.Invoke();
    }

    private void OnReadyChanged(bool oldValue, bool newValue)
    {
        UpdateVisual();
        OnAnyNameChanged?.Invoke();
    }

    private void UpdateVisual()
    {
        nameText.text = playerName.Value.ToString();
        nameText.color = isReady.Value ? Color.green : Color.black;
    }

    [ServerRpc]
    private void SubmitNameServerRpc(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            newName = " ";

        playerName.Value = new FixedString128Bytes(newName);
    }

    [ServerRpc]
    public void SetReadyServerRpc(bool value)
    {
        isReady.Value = value;
    }

    public void ResetReadyServerSide()
    {
        if (!IsServer) return;

        isReady.Value = false;
    }

    public bool IsReady()
    {
        return isReady.Value;
    }

    public string GetCurrentName()
    {
        return playerName.Value.ToString();
    }
}