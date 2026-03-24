using UnityEngine;

public class PanelSwitch : MonoBehaviour
{
    [SerializeField] private GameObject listPanel;

    public void SwitchPanel()
    {
        listPanel.SetActive(false );
    }
}
