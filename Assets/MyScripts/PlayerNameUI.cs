using TMPro;
using UnityEngine;

public class PlayerNameUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;

    public static string PendingPlayerName;

    public void ConfirmPlayerName()
    {
        Debug.Log("[PlayerNameUI] ConfirmPlayerName llamado");

        if (nameInputField == null) return;

        string newName = nameInputField.text;

        if (string.IsNullOrWhiteSpace(newName))
            return;

        // Guardamos el nombre aunque el player no exista todavía
        PendingPlayerName = newName;

        Debug.Log("Nombre guardado localmente: " + newName);
    }
}
