using UnityEngine;
using UnityEngine.UI;

public class ButtonKeyTrigger : MonoBehaviour
{
    [SerializeField] private Button targetButton;

    private PlayerKeyBool playerKeyBool;
    private bool alreadyTriggered = false;

    private void Update()
    {
        // Busca al jugador
        if (playerKeyBool == null)
        {
            TryFindPlayer();
            return;
        }
    }

    private void TryFindPlayer()
    {
        var allPlayers = FindObjectsByType<PlayerKeyBool>(FindObjectsSortMode.None);

        foreach (var player in allPlayers)
        {
            if (player.IsOwner)
            {
                playerKeyBool = player;

                playerKeyBool.triggeredButtonKey.OnValueChanged += OnTriggeredChanged;

                Debug.Log("Player local asignado correctamente");
                return;
            }
        }
    }

    private void OnTriggeredChanged(bool oldValue, bool newValue)
    {
        if (newValue && !alreadyTriggered)
        {
            SimulateClick();
            alreadyTriggered = true;
        }
    }

    private void SimulateClick()
    {
        if (targetButton != null)
        {
            targetButton.onClick.Invoke();
        }
        else
        {
            Debug.LogWarning("No hay botón asignado.");
        }
    }
}