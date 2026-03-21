using UnityEngine;
using UnityEngine.UI;

public class ButtonKeyTrigger : MonoBehaviour
{
    [SerializeField] private Button targetButton;

    private PlayerKeyBool playerKeyBool;
    private bool alreadyTriggered = false;

    private void Update()
    {
        // Si todavía no encontramos al player, lo buscamos
        if (playerKeyBool == null)
        {
            playerKeyBool = FindAnyObjectByType<PlayerKeyBool>();
            return;
        }

        // Si ya se activó una vez, no hacemos nada más
        if (alreadyTriggered)
            return;

        // Si el bool del player es true, simulamos el click
        if (playerKeyBool.triggeredButtonKey)
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