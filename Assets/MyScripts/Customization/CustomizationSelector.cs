using System.IO;
using TMPro;
using UnityEngine;

public class CustomizationSelector : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text valueText;

    [Header("Customization Settings")]
    [SerializeField] private int minValue = 1;
    [SerializeField] private int maxValue = 3;

    private int currentValue;

    private string SavePath =>
        Path.Combine(Application.persistentDataPath, "customization.json");

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
        CustomizationData data = new CustomizationData
        {
            bodyIndex = currentValue
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
        CustomizationData data = JsonUtility.FromJson<CustomizationData>(json);

        currentValue = Mathf.Clamp(data.bodyIndex, minValue, maxValue);
    }
}
