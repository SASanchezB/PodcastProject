using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputRebindManager : MonoBehaviour
{
    public InputActionAsset inputActions;

    private const string RebindsKey = "INPUT_REBINDS";

    private void Awake()
    {
        LoadRebinds();
    }

    public void StartRebind(
        string actionName,
        int bindingIndex,
        Action<string> onRebindComplete)
    {
        var action = inputActions.FindAction(actionName);
        if (action == null)
        {
            Debug.LogError($"Action '{actionName}' no encontrada");
            return;
        }

        action.Disable();

        action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse")
            .OnComplete(op =>
            {
                op.Dispose();
                action.Enable();

                SaveRebinds();

                string display =
                    action.GetBindingDisplayString(bindingIndex);

                onRebindComplete?.Invoke(display);
            })
            .Start();
    }

    public string GetBindingDisplay(string actionName, int bindingIndex)
    {
        var action = inputActions.FindAction(actionName);
        return action.GetBindingDisplayString(bindingIndex);
    }

    private void SaveRebinds()
    {
        string json = inputActions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(RebindsKey, json);
        PlayerPrefs.Save();
    }

    private void LoadRebinds()
    {
        if (!PlayerPrefs.HasKey(RebindsKey)) return;

        string json = PlayerPrefs.GetString(RebindsKey);
        inputActions.LoadBindingOverridesFromJson(json);
    }

    public void ResetBinding(string actionName, int bindingIndex)
    {
        var action = inputActions.FindAction(actionName);

        if (action == null)
        {
            Debug.LogError($"Action '{actionName}' no encontrada");
            return;
        }

        if (bindingIndex < 0 || bindingIndex >= action.bindings.Count)
        {
            Debug.LogError("BindingIndex fuera de rango");
            return;
        }

        action.RemoveBindingOverride(bindingIndex);
        SaveRebinds();
    }

}
