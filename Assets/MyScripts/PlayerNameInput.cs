using TMPro;
using UnityEngine;

public class PlayerNameInput : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;

    public void SavePlayerName()
    {
        if (string.IsNullOrEmpty(nameInputField.text))
            return;

        PlayerPrefs.SetString("PlayerName", nameInputField.text);
        PlayerPrefs.Save();

        Debug.Log("Nombre guardado: " + nameInputField.text);
    }
}
