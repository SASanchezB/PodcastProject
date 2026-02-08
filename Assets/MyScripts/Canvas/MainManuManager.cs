using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("All Panels")]
    [SerializeField] private GameObject[] panels;

    private void Start()
    {
        ShowPanel(panels[0]); //La pantalla principal, no se si se puede trabar en uno, por las dudas

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowPanel(GameObject panelToShow)
    {
        foreach (var panel in panels)
        {
            panel.SetActive(panel == panelToShow);
        }
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego");
        Application.Quit();
    }
}
