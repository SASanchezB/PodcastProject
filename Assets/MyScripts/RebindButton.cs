using UnityEngine;
using TMPro;

public class RebindButton : MonoBehaviour
{
    public string actionName;
    public int bindingIndex;

    public TextMeshProUGUI bindingText;
    public InputRebindManager rebindManager;

    private void Start()
    {
        Refresh();
    }

    public void StartRebind()
    {
        bindingText.text = "Enter input...";

        rebindManager.StartRebind(
            actionName,
            bindingIndex,
            OnRebindComplete
        );
    }

    private void OnRebindComplete(string newBinding)
    {
        bindingText.text = newBinding;
    }

    private void Refresh()
    {
        bindingText.text =
            rebindManager.GetBindingDisplay(actionName, bindingIndex);
    }

    public void ResetToDefault()
    {
        rebindManager.ResetBinding(actionName, bindingIndex);
        Refresh();
    }

}
