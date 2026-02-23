using TMPro;
using UnityEngine;

public class RoomNameUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField roomInputField;

    public static string PendingRoomName;

    private const string RoomNameKey = "ROOM_NAME";

    private void Awake()
    {
        if (PlayerPrefs.HasKey(RoomNameKey))
        {
            string savedName = PlayerPrefs.GetString(RoomNameKey);
            PendingRoomName = savedName;

            if (roomInputField != null)
                roomInputField.text = savedName;
        }
        else
        {
            PendingRoomName = "";
        }

        if (roomInputField != null)
        {
            roomInputField.onValueChanged.AddListener(OnRoomNameChanged);
        }
    }

    private void OnDestroy()
    {
        if (roomInputField != null)
        {
            roomInputField.onValueChanged.RemoveListener(OnRoomNameChanged);
        }
    }

    private void OnRoomNameChanged(string newName)
    {
        PendingRoomName = newName;

        PlayerPrefs.SetString(RoomNameKey, newName);
        PlayerPrefs.Save();
    }
}