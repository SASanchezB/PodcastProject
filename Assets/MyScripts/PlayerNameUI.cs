using TMPro;
using UnityEngine;

public class PlayerNameUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;

    public static string PendingPlayerName;

    private const string PlayerNameKey = "PLAYER_NAME";

    private void Awake()
    {
        // Cargar nombre guardado
        if (PlayerPrefs.HasKey(PlayerNameKey))
        {
            string savedName = PlayerPrefs.GetString(PlayerNameKey);
            PendingPlayerName = savedName;

            if (nameInputField != null)
                nameInputField.text = savedName;

            Debug.Log("Nombre cargado desde PlayerPrefs: " + savedName);
        }
    }

    public void ConfirmPlayerName()
    {
        if (nameInputField == null) return;

        string newName = nameInputField.text;

        if (string.IsNullOrWhiteSpace(newName))
            return;

        // Guardar localmente
        PendingPlayerName = newName;

        // Persistir entre sesiones
        PlayerPrefs.SetString(PlayerNameKey, newName);
        PlayerPrefs.Save();

        Debug.Log("Nombre guardado y persistido: " + newName);
    }

    public void ClearSavedPlayerName()
    {
        PlayerPrefs.DeleteKey(PlayerNameKey);
        PlayerPrefs.Save();

        PendingPlayerName = null;

        if (nameInputField != null)
            nameInputField.text = string.Empty;

        Debug.Log("Nombre borrado de PlayerPrefs");
    }
}
