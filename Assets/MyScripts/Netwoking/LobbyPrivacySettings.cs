using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class LobbyPrivacySettings : MonoBehaviour
{
    [SerializeField] private Toggle privateLobbyToggle;

    private static string FilePath =>
        Path.Combine(Application.persistentDataPath, "lobby_settings.json");

    [System.Serializable]
    private class LobbySettingsData
    {
        public bool isPrivate;
    }

    private void Awake()
    {
        Load();
        privateLobbyToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnDestroy()
    {
        privateLobbyToggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool value)
    {
        Save(value);
    }

    private void Save(bool isPrivate)
    {
        var data = new LobbySettingsData
        {
            isPrivate = isPrivate
        };

        File.WriteAllText(FilePath, JsonUtility.ToJson(data));
    }

    private void Load()
    {
        if (!File.Exists(FilePath))
        {
            privateLobbyToggle.isOn = false;
            return;
        }

        var json = File.ReadAllText(FilePath);
        var data = JsonUtility.FromJson<LobbySettingsData>(json);
        privateLobbyToggle.isOn = data.isPrivate;
    }

    public static bool IsPrivateLobby()
    {
        if (!File.Exists(FilePath)) return false;

        var json = File.ReadAllText(FilePath);
        return JsonUtility.FromJson<LobbySettingsData>(json).isPrivate;
    }
}
