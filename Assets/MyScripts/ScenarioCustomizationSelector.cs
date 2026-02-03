using System.IO;
using TMPro;
using UnityEngine;
using Unity.Multiplayer.Widgets;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ScenarioCustomizationSelector : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text valueText;

    [Header("Customization Settings")]
    [SerializeField] private int minValue = 1;
    [SerializeField] private int maxValue = 3;  // Ajusta según cuántos escenarios tengas por maxPlayers

    // Nueva referencia: Asigna el WidgetConfiguration aquí
    [Header("Multiplayer Configuration")]
    [SerializeField] private WidgetConfiguration widgetConfiguration;  // Asigna el ScriptableObject en el Inspector

    // Nuevos arrays: uno por cada maxPlayers (índice 0 = 2 players, 1 = 4, 2 = 6, 3 = 8)
    [Header("Scene Names by Max Players")]
    [Tooltip("Escenas para 2 jugadores")]
    [SerializeField] private string[] sceneNamesFor2Players = new string[] { "Scene1_2P", "Scene2_2P" };
    [Tooltip("Escenas para 4 jugadores")]
    [SerializeField] private string[] sceneNamesFor4Players = new string[] { "Scene1_4P", "Scene2_4P" };
    [Tooltip("Escenas para 6 jugadores")]
    [SerializeField] private string[] sceneNamesFor6Players = new string[] { "Scene1_6P", "Scene2_6P" };
    [Tooltip("Escenas para 8 jugadores")]
    [SerializeField] private string[] sceneNamesFor8Players = new string[] { "Scene1_8P", "Scene2_8P" };

    // Array combinado para acceso fácil (no editable en Inspector)
    private string[][] sceneNamesByMaxPlayers;

    // Valores fijos para maxPlayers
    private readonly int[] maxPlayersOptions = { 2, 4, 6, 8 };
    private int currentMaxPlayersIndex = 1;  // Por defecto: 4 (índice 1)

    private int currentValue;  // Índice del escenario (1-based en UI, 0-based en array)

    private string SavePath =>
        Path.Combine(Application.persistentDataPath, "scenario.json");

    private void Awake()
    {
        // Inicializar el array combinado
        sceneNamesByMaxPlayers = new string[][] {
            sceneNamesFor2Players,
            sceneNamesFor4Players,
            sceneNamesFor6Players,
            sceneNamesFor8Players
        };

        Load();
        UpdateUI();
        UpdateWidgetConfiguration();  // Actualiza MaxPlayers al inicio
        Debug.Log($"[ScenarioCustomizationSelector] Awake completado. MaxPlayers actual: {GetCurrentMaxPlayers()}");
    }

    // Métodos para el escenario (igual que antes)
    public void Increase()
    {
        currentValue++;
        if (currentValue > maxValue)
            currentValue = minValue;

        Save();
        UpdateUI();
    }

    public void Decrease()
    {
        currentValue--;
        if (currentValue < minValue)
            currentValue = maxValue;

        Save();
        UpdateUI();
    }

    // Nuevos métodos para maxPlayers
    public void IncreaseMaxPlayers()
    {
        currentMaxPlayersIndex++;
        if (currentMaxPlayersIndex >= maxPlayersOptions.Length)
            currentMaxPlayersIndex = 0;

        Save();
        UpdateUI();
        UpdateWidgetConfiguration();
        Debug.Log($"[ScenarioCustomizationSelector] MaxPlayers aumentado a: {GetCurrentMaxPlayers()}");
    }

    public void DecreaseMaxPlayers()
    {
        currentMaxPlayersIndex--;
        if (currentMaxPlayersIndex < 0)
            currentMaxPlayersIndex = maxPlayersOptions.Length - 1;

        Save();
        UpdateUI();
        UpdateWidgetConfiguration();
        Debug.Log($"[ScenarioCustomizationSelector] MaxPlayers disminuido a: {GetCurrentMaxPlayers()}");
    }

    private void UpdateUI()
    {
        if (valueText != null)
        {
            int currentMaxPlayers = maxPlayersOptions[currentMaxPlayersIndex];
            valueText.text = $" {currentValue} -  {currentMaxPlayers} ";
        }
    }

    // Nuevo método: Actualiza WidgetConfiguration.MaxPlayers
    private void UpdateWidgetConfiguration()
    {
        if (widgetConfiguration != null)
        {
            widgetConfiguration.MaxPlayers = maxPlayersOptions[currentMaxPlayersIndex];
            Debug.Log($"[ScenarioCustomizationSelector] WidgetConfiguration.MaxPlayers actualizado a: {widgetConfiguration.MaxPlayers} (Instancia ID: {widgetConfiguration.GetInstanceID()})");

#if UNITY_EDITOR
            // Fuerza la actualización del Inspector
            EditorUtility.SetDirty(widgetConfiguration);
            AssetDatabase.SaveAssets();
#endif
        }
        else
        {
            Debug.LogError("[ScenarioCustomizationSelector] widgetConfiguration no asignado!");
        }
    }

    private void Save()
    {
        ScenarioData data = new ScenarioData
        {
            scenarioIndex = currentValue,
            maxPlayersIndex = currentMaxPlayersIndex
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    private void Load()
    {
        if (!File.Exists(SavePath))
        {
            currentValue = minValue;
            currentMaxPlayersIndex = 1;  // Por defecto: 4
            return;
        }

        string json = File.ReadAllText(SavePath);
        ScenarioData data = JsonUtility.FromJson<ScenarioData>(json);

        currentValue = Mathf.Clamp(data.scenarioIndex, minValue, maxValue);
        currentMaxPlayersIndex = Mathf.Clamp(data.maxPlayersIndex, 0, maxPlayersOptions.Length - 1);
    }

    // Método público para obtener el nombre de la escena actual (usado por SessionSceneLoader)
    public string GetCurrentSceneName()
    {
        string[] currentScenes = sceneNamesByMaxPlayers[currentMaxPlayersIndex];
        int index = currentValue - 1;  // Ajuste porque los arrays empiezan en 0
        if (index >= 0 && index < currentScenes.Length)
            return currentScenes[index];
        else
            return "Gameplay";  // Fallback si el índice no coincide
    }

    // Método público para obtener el maxPlayers actual (útil para configurar el WidgetConfiguration)
    public int GetCurrentMaxPlayers()
    {
        return maxPlayersOptions[currentMaxPlayersIndex];
    }
}

// Clase de datos para el JSON (modificada)
[System.Serializable]
public class ScenarioData
{
    public int scenarioIndex;
    public int maxPlayersIndex;  // Nuevo: índice del maxPlayers
}