using TMPro;
using UnityEngine;

public class PlayerNameUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;

    public static string PendingPlayerName;

    private const string PlayerNameKey = "PLAYER_NAME";

    private void Awake()
    {
        // cargar nombre del jugador al inicio (puede estar vacio si no se puso nada)
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

        // sub al evento de cambio de input
        if (nameInputField != null)
        {
            nameInputField.onValueChanged.AddListener(OnNameChanged);
        }
    }

    private void OnDestroy()
    {
        // FALLA -> Acordarte de limpiar porfavor
        if (nameInputField != null)
        {
            nameInputField.onValueChanged.RemoveListener(OnNameChanged);
        }
    }

    private void OnNameChanged(string newName)
    {
        PendingPlayerName = newName;

        PlayerPrefs.SetString(PlayerNameKey, newName);
        PlayerPrefs.Save();
    }
}
