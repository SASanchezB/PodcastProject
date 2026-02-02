using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.IO;

public class SessionSceneLoader : MonoBehaviour
{
    [Header("Scenario Settings")]
    [SerializeField] private ScenarioCustomizationSelector scenarioSelector;  // Asigna el GameObject con ScenarioCustomizationSelector en el Inspector
    [SerializeField] private string defaultScene = "Escena1";  // Escena por defecto (ej. "Escena1")

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

        // Fallback: Lee directamente del JSON
        if (!File.Exists(SavePath))
        {
            return defaultScene;
        }

        string json = File.ReadAllText(SavePath);
        ScenarioData data = JsonUtility.FromJson<ScenarioData>(json);

        int index = Mathf.Clamp(data.scenarioIndex, 1, 3) - 1;  // Ajuste para array (asume 3 escenas, ajusta si cambias)
        string[] fallbackScenes = new string[] { "Escena1", "Escena2", "Escena3" };  // Debe coincidir con el array en ScenarioCustomizationSelector

        if (index >= 0 && index < fallbackScenes.Length)
            return fallbackScenes[index];
        else
            return defaultScene;
    }
}