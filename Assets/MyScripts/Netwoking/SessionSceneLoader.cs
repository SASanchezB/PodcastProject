using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.IO;

public class SessionSceneLoader : MonoBehaviour
{
    [Header("Scenario Settings")]
    [SerializeField] private ScenarioCustomizationSelector scenarioSelector;
    [SerializeField] private string defaultScene = "Escena1";  // Escena por defecto por si hay una escena no cargada

    // Arrays de escenas por maxPlayers
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

        // Se selecciona la escena desde el JSON
        string sceneToLoad = GetSelectedSceneName();

        // Se usa network manager para hacer la sincronizacion entre todos
        NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);

        // En caso que la linea de arriba no funque, se tiene esta linea para notificarlos (se podria sacar esta linea, pero se deja por las dudas)
        NotifyClientsToLoadSceneClientRpc(sceneToLoad);
    }

    [ClientRpc]
    private void NotifyClientsToLoadSceneClientRpc(string sceneName)
    {
        // Los clientes cargan la escena si no son el host (el host ya la tiene cargada)
        if (!NetworkManager.Singleton.IsServer)
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private string GetSelectedSceneName()
    {
        // se lee desde el ScenarioCustomizationSelector
        if (scenarioSelector != null)
        {
            return scenarioSelector.GetCurrentSceneName();
        }

        // Si falla, se lee el json y se usa el array de max
        if (!File.Exists(SavePath))
        {
            return defaultScene;
        }

        string json = File.ReadAllText(SavePath);
        ScenarioData data = JsonUtility.FromJson<ScenarioData>(json);

        // Usa maxPlayersIndex para determinar el array de escenas, ya que los diferentes tamaños de lobby tienen diferentes escenas
        int maxPlayersIndex = Mathf.Clamp(data.maxPlayersIndex, 0, sceneNamesByMaxPlayers.Length - 1);
        string[] scenes = sceneNamesByMaxPlayers[maxPlayersIndex];

        int scenarioIndex = Mathf.Clamp(data.scenarioIndex - 1, 0, scenes.Length - 1); 

        return scenes[scenarioIndex];
    }
}