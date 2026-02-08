using TMPro;
using UnityEngine;

public class PlayerNameDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;

    void Start()
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "Player");

        nameText.text = playerName;
    }
}
