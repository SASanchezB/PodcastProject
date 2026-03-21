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
        kickButton.onClick.AddListener(ActiveBool);
    }

    private void ActiveBool()
    {
        var networkManager = NetworkManager.Singleton;

        if (networkManager == null)
        {
            Debug.LogWarning("No hay NetworkManager.");
            return;
        }

        if (networkManager.ConnectedClients.TryGetValue(clientId, out var client))
        {
            var playerObject = client.PlayerObject;

            if (playerObject != null)
            {
                PlayerKeyBool playerKeyBool = playerObject.GetComponent<PlayerKeyBool>();

                if (playerKeyBool != null)
                {
                    // Sincronizacion global
                    playerKeyBool.triggeredButtonKey.Value = true;

                    Debug.Log($"Bool activado para el cliente {clientId}");
                }
                else
                {
                    Debug.LogWarning("El Player no tiene PlayerKeyBool.");
                }
            }
            else
            {
                Debug.LogWarning("PlayerObject es null.");
            }
        }
        else
        {
            Debug.LogWarning($"No se encontró el cliente con ID {clientId}");
        }
    }
}