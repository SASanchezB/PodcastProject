using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.IO;

public class SessionSceneLoader : MonoBehaviour
{
    [Header("Scenario Settings")]
    [SerializeField] private ScenarioCustomizationSelector scenarioSelector;  // Asigna el GameObject con ScenarioCustomizationSelector en el Inspector
    [SerializeField] private string defaultScene = "Gameplay";  // Escena por defecto si no hay selección

    private string SavePath =>
        Path.Combine(Application.persistentDataPath, "scenario.json");

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        // SOLO el host cambia de escena
        if (!NetworkManager.Singleton.IsHost)
            return;

        // Obtén el nombre de la escena desde el selector o JSON
        string sceneToLoad = GetSelectedSceneName();

        NetworkManager.Singleton.SceneManager.LoadScene(
            sceneToLoad,
            LoadSceneMode.Single
        );
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