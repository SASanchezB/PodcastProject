using TMPro;
using UnityEngine;

public class PlayerNameUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;

    public static string PendingPlayerName;

    private const string PlayerNameKey = "PLAYER_NAME";

    private void Awake()
    {
        // Cargar nombre guardado (puede ser vacío)
        if (PlayerPrefs.HasKey(PlayerNameKey))
        {
            string savedName = PlayerPrefs.GetString(PlayerNameKey);
            PendingPlayerName = savedName;

            if (nameInputField != null)
                nameInputField.text = savedName;
        }
        else
        {
            PendingPlayerName = "";
        }

        // Suscribirse al cambio del input
        if (nameInputField != null)
        {
            nameInputField.onValueChanged.AddListener(OnNameChanged);
        }
    }

    private void OnDestroy()
    {
        // Limpio listener para evitar leaks
        if (nameInputField != null)
        {
            nameInputField.onValueChanged.RemoveListener(OnNameChanged);
        }
    }

    private void OnNameChanged(string newName)
    {
        // Acepta vacío o espacios
        PendingPlayerName = newName;

        PlayerPrefs.SetString(PlayerNameKey, newName);
        PlayerPrefs.Save();

        //Debug.Log($"Nombre actualizado automáticamente: '{newName}'");
    }
}
