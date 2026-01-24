using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class HardDisconnect : MonoBehaviour
{
    [SerializeField] private string mainMenuScene = "MainMenu";

    public void DisconnectAndCleanup()
    {
        // 1. Apagar Netcode (host o cliente)
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
        }

        // 2. Destruir TODO lo que esté en DontDestroyOnLoad
        DestroyAllDontDestroyOnLoad();

        // 3. Volver al menú (Widgets se reinician solos)
        SceneManager.LoadScene(mainMenuScene);
    }

    private void DestroyAllDontDestroyOnLoad()
    {
        GameObject temp = new GameObject("DDOL_Finder");
        DontDestroyOnLoad(temp);

        Scene ddolScene = temp.scene;
        Destroy(temp);

        foreach (GameObject obj in ddolScene.GetRootGameObjects())
        {
            Destroy(obj);
        }
    }
}
