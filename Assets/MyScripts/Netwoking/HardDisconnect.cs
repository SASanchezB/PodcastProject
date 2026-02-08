using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class HardDisconnect : MonoBehaviour
{
    //No se si quedo obsoleto o este funciona

    [SerializeField] private string mainMenuScene = "MainMenu";

    public void DisconnectAndCleanup()
    {
        // Apaga netcode
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
        }

        // BORRA TODO LOS PERMANENTNES
        DestroyAllDontDestroyOnLoad();

        // vuelve al menu
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
