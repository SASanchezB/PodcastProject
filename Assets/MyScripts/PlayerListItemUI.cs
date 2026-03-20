using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private Button kickButton;

    private ulong clientId;

    public void Setup(string playerName, bool isReady, ulong id, bool showKick)
    {
        clientId = id;

        if (isReady)
            playerNameText.text = $"<color=green>{playerName}</color>";
        else
            playerNameText.text = playerName;

        kickButton.gameObject.SetActive(showKick);
        kickButton.onClick.AddListener(HostCall);
    }

    // En PlayerListItemUI.cs
    private void HostCall()
    {
        // 
    }
}