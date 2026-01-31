using System.IO;
using TMPro;
using UnityEngine;

public class ScenarioCustomizationSelector : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text valueText;

    [Header("Customization Settings")]
    [SerializeField] private int minValue = 1;
    [SerializeField] private int maxValue = 3;  // Ajusta según cuántos escenarios tengas
    [SerializeField] private string[] sceneNames = new string[] { "Escena1", "Escena2", "Escena3" };  // Nombres de escenas configurables desde el Inspector

    private int currentValue;

    private string SavePath =>
        Path.Combine(Application.persistentDataPath, "scenario.json");

    private void Awake()
    {
        Load();
        UpdateUI();
    }

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

    private void UpdateUI()
    {
        if (valueText != null)
            valueText.text = currentValue.ToString();
    }

    private void Save()
    {
        ScenarioData data = new ScenarioData
        {
            scenarioIndex = currentValue
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    private void Load()
    {
        if (!File.Exists(SavePath))
        {
            currentValue = minValue;
            return;
        }

        string json = File.ReadAllText(SavePath);
        ScenarioData data = JsonUtility.FromJson<ScenarioData>(json);

        currentValue = Mathf.Clamp(data.scenarioIndex, minValue, maxValue);
    }

    // Método público para obtener el nombre de la escena actual (usado por SessionSceneLoader)
    public string GetCurrentSceneName()
    {
        int index = currentValue - 1;  // Ajuste porque los arrays empiezan en 0
        if (index >= 0 && index < sceneNames.Length)
            return sceneNames[index];
        else
            return "Gameplay";  // Fallback si el índice no coincide
    }
}

// Clase de datos para el JSON (similar a CustomizationData)
[System.Serializable]
public class ScenarioData
{
    public int scenarioIndex;
}