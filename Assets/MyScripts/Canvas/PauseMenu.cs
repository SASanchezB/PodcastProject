using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject listPanel;

    [SerializeField] private bool isPaused = false;

    private void Awake()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (listPanel != null)
            listPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void UnPause()
    {
        TogglePause();
    }

    public void UnPauseShowList()
    {
        listPanel.SetActive(true);
    }

    private void TogglePause()
    {
        isPaused = !isPaused;

        if (pausePanel != null)
            pausePanel.SetActive(isPaused);

        Cursor.lockState = isPaused
            ? CursorLockMode.None
            : CursorLockMode.Locked;

        Cursor.visible = isPaused;
    }
}
