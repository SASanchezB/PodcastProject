using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.IO;

public class SessionSceneLoader : MonoBehaviour
{
    [Header("Scenario Settings")]
    [SerializeField] private ScenarioCustomizationSelector scenarioSelector;  // Asigna el GameObject con ScenarioCustomizationSelector en el Inspector
    [SerializeField] private string defaultScene = "Escena1";  // Escena por defecto (ej. "Escena1")

    // Arrays de escenas por maxPlayers (debe coincidir con ScenarioCustomizationSelector)
    private readonly string[][] sceneNamesByMaxPlayers = new string[][] {
        new string[] { "Scene1_2P", "Scene2_2P" },  // Para 2 jugadores
        new string[] { "Scene1_4P", "Scene2_4P" },  // Para 4 jugadores
        new string[] { "Scene1_6P", "Scene2_6P" },  // Para 6 jugadores
        new string[] { "Scene1_8P", "Scene2_8P" }   // Para 8 jugadores
    };

    private string SavePath =>
        Path.Combine(Application.persistentDataPath, "scenario.json");

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        // SOLO el host cambia de escena y sincroniza a todos
        if (!NetworkManager.Singleton.IsHost)
            return;

        // Obtén el nombre de la escena desde el selector o JSON
        string sceneToLoad = GetSelectedSceneName();

        // CAMBIO: Usa NetworkManager.SceneManager para sincronizar la escena a todos los clientes
        NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);

        // OPCIONAL: Notifica explícitamente a los clientes (por si LoadScene no sincroniza inmediatamente)
        NotifyClientsToLoadSceneClientRpc(sceneToLoad);
    }

    [ClientRpc]
    private void NotifyClientsToLoadSceneClientRpc(string sceneName)
    {
        // Los clientes cargan la escena si no son el host (el host ya la cargó)
        if (!NetworkManager.Singleton.IsServer)
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private string GetSelectedSceneName()
    {
        // Primero, intenta obtener desde el ScenarioCustomizationSelector si está asignado
        if (scenarioSelector != null)
        {
            return scenarioSelector.GetCurrentSceneName();
        }

        // Fallback: Lee directamente del JSON y usa arrays por maxPlayers
        if (!File.Exists(SavePath))
        {
            return defaultScene;
        }

        string json = File.ReadAllText(SavePath);
        ScenarioData data = JsonUtility.FromJson<ScenarioData>(json);

        // Usa maxPlayersIndex para determinar el array de escenas
        int maxPlayersIndex = Mathf.Clamp(data.maxPlayersIndex, 0, sceneNamesByMaxPlayers.Length - 1);
        string[] scenes = sceneNamesByMaxPlayers[maxPlayersIndex];

        int scenarioIndex = Mathf.Clamp(data.scenarioIndex - 1, 0, scenes.Length - 1);  // Ajuste para array

        return scenes[scenarioIndex];
    }
}